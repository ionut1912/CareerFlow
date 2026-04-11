"""Tests for app/document_reader/service.py"""

from __future__ import annotations

from typing import Any
from unittest.mock import AsyncMock, MagicMock, patch

import pytest
from openai import RateLimitError
from tenacity import retry, retry_if_exception_type, stop_after_attempt, wait_none

from app.courses.schema import ChapterSkeleton
from app.document_reader import service
from app.document_reader.extractor import DocumentContent
from app.document_reader.schema import (
    AnalysisAndSkeleton,
    DocumentAnalysis,
    FullChapterResponse,
    LearningPlanSkeleton,
    SubchapterContent,
)
from tests.conftest import make_parsed_response, make_quiz_question

_FAST_RETRY = retry(
    retry=retry_if_exception_type(RateLimitError),
    wait=wait_none(),
    stop=stop_after_attempt(8),
    reraise=True,
)


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _make_analysis() -> DocumentAnalysis:
    return DocumentAnalysis(title="Doc Title", summary="A summary.", key_topics=["a", "b"])


def _make_doc_skeleton(n: int = 2) -> LearningPlanSkeleton:
    return LearningPlanSkeleton(
        topic="Topic",
        chapters=[ChapterSkeleton(title=f"Ziua {i + 1}", core_concept=f"Concept {i + 1}", day=i + 1) for i in range(n)],
    )


def _make_aas(n: int = 2) -> AnalysisAndSkeleton:
    return AnalysisAndSkeleton(
        analysis=_make_analysis(),
        skeleton=_make_doc_skeleton(n),
    )


def _make_full_chapter() -> FullChapterResponse:
    q = make_quiz_question()
    sub = SubchapterContent(
        title="Sub",
        content_summary="Summary",
        theory_html="<p>Theory</p>",
        quiz=[q, q, q],
    )
    return FullChapterResponse(subchapters=[sub], recap_quiz=[q] * 10)


def _client_returning(obj: object) -> AsyncMock:
    client = AsyncMock()
    client.beta.chat.completions.parse = AsyncMock(return_value=make_parsed_response(obj))
    return client


def _doc_content(text: str = "Paragraph one.\n\nParagraph two.") -> DocumentContent:
    return DocumentContent(filename="test.pdf", total_pages=1, text=text, pages=[text])


def _rate_limit_error() -> RateLimitError:
    return RateLimitError("rate limit", response=MagicMock(status_code=429), body={})


# ---------------------------------------------------------------------------
# Unit tests  analyze_and_skeleton
# ---------------------------------------------------------------------------


class TestUnitAnalyzeAndSkeleton:
    """Unit tests for service.analyze_and_skeleton."""

    @pytest.mark.anyio
    async def test_happy_path_returns_analysis_and_skeleton(self) -> None:
        """analyze_and_skeleton returns AnalysisAndSkeleton on success."""
        result = await service.analyze_and_skeleton(_client_returning(_make_aas(3)), _doc_content())
        assert isinstance(result, AnalysisAndSkeleton)
        assert len(result.skeleton.chapters) == 3

    @pytest.mark.anyio
    async def test_raises_runtime_error_when_parsed_none(self) -> None:
        """analyze_and_skeleton raises RuntimeError when parsed is None."""
        with pytest.raises(RuntimeError, match="AnalysisAndSkeleton"):
            await service.analyze_and_skeleton(_client_returning(None), _doc_content())

    @pytest.mark.anyio
    async def test_response_format_is_correct(self) -> None:
        """analyze_and_skeleton passes response_format=AnalysisAndSkeleton."""
        client = _client_returning(_make_aas())
        await service.analyze_and_skeleton(client, _doc_content())

        _, kwargs = client.beta.chat.completions.parse.call_args
        assert kwargs.get("response_format") is AnalysisAndSkeleton

    @pytest.mark.anyio
    async def test_long_text_is_truncated(self) -> None:
        """Text longer than 12 000 chars is truncated before being sent to the API."""
        client = _client_returning(_make_aas())
        await service.analyze_and_skeleton(client, _doc_content(text="A" * 20_000))

        _, kwargs = client.beta.chat.completions.parse.call_args
        user_msg = next(m["content"] for m in kwargs["messages"] if m["role"] == "user")
        assert len(user_msg) < 20_000 + 200

    @pytest.mark.anyio
    async def test_filename_and_pages_in_prompt(self) -> None:
        """Filename and page count appear in the user message."""
        client = _client_returning(_make_aas())
        content = DocumentContent(filename="my_file.pdf", total_pages=42, text="Some text.", pages=["Some text."])
        await service.analyze_and_skeleton(client, content)

        _, kwargs = client.beta.chat.completions.parse.call_args
        user_msg = next(m["content"] for m in kwargs["messages"] if m["role"] == "user")
        assert "my_file.pdf" in user_msg
        assert "42" in user_msg

    @pytest.mark.anyio
    async def test_rate_limit_retries_and_succeeds(self) -> None:
        """analyze_and_skeleton retries on RateLimitError and succeeds on the second attempt.

        _retry is patched with wait_none() so tenacity never calls asyncio.sleep,
        making the test instant regardless of how tenacity resolves its sleep reference.
        """
        aas = _make_aas()
        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(side_effect=[_rate_limit_error(), make_parsed_response(aas)])

        with patch("app.document_reader.service._retry", _FAST_RETRY):
            result = await service.analyze_and_skeleton(client, _doc_content())

        assert isinstance(result, AnalysisAndSkeleton)
        assert client.beta.chat.completions.parse.call_count == 2

    @pytest.mark.anyio
    async def test_exhausted_retries_reraise(self) -> None:
        """analyze_and_skeleton re-raises RateLimitError after all 8 retry attempts.

        _retry is patched with wait_none() so all 8 attempts complete instantly
        """
        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(side_effect=_rate_limit_error())

        with patch("app.document_reader.service._retry", _FAST_RETRY), pytest.raises(RateLimitError):
            await service.analyze_and_skeleton(client, _doc_content())


# ---------------------------------------------------------------------------
# Unit tests generate_full_chapter
# ---------------------------------------------------------------------------


class TestUnitGenerateFullChapter:
    """Unit tests for service.generate_full_chapter."""

    @pytest.mark.anyio
    async def test_happy_path_returns_full_chapter_response(self) -> None:
        """generate_full_chapter returns a FullChapterResponse."""
        client = _client_returning(_make_full_chapter())
        chapter = ChapterSkeleton(title="Ziua 1", core_concept="Concept", day=1)

        result = await service.generate_full_chapter(client, ["chunk one", "chunk two"], chapter)
        assert isinstance(result, FullChapterResponse)
        assert len(result.recap_quiz) == 10

    @pytest.mark.anyio
    async def test_raises_runtime_error_when_parsed_none(self) -> None:
        """generate_full_chapter raises RuntimeError when parsed is None."""
        client = _client_returning(None)
        chapter = ChapterSkeleton(title="Ziua 1", core_concept="Concept", day=1)

        with pytest.raises(RuntimeError, match="FullChapterResponse"):
            await service.generate_full_chapter(client, ["chunk"], chapter)

    @pytest.mark.anyio
    async def test_response_format_is_full_chapter_response(self) -> None:
        """generate_full_chapter passes response_format=FullChapterResponse."""
        client = _client_returning(_make_full_chapter())
        chapter = ChapterSkeleton(title="Ziua 1", core_concept="Concept", day=1)
        await service.generate_full_chapter(client, ["chunk"], chapter)

        _, kwargs = client.beta.chat.completions.parse.call_args
        assert kwargs.get("response_format") is FullChapterResponse

    @pytest.mark.anyio
    async def test_chapter_title_and_concept_in_prompt(self) -> None:
        """Chapter title and core_concept appear in the user message."""
        client = _client_returning(_make_full_chapter())
        chapter = ChapterSkeleton(title="UniqueTitle99", core_concept="UniqueConcept88", day=1)
        await service.generate_full_chapter(client, ["context chunk"], chapter)

        _, kwargs = client.beta.chat.completions.parse.call_args
        user_msg = next(m["content"] for m in kwargs["messages"] if m["role"] == "user")
        assert "UniqueTitle99" in user_msg
        assert "UniqueConcept88" in user_msg

    @pytest.mark.anyio
    async def test_empty_chunks_does_not_raise(self) -> None:
        """generate_full_chapter handles an empty chunk list gracefully."""
        client = _client_returning(_make_full_chapter())
        chapter = ChapterSkeleton(title="Ziua 1", core_concept="Concept", day=1)
        result = await service.generate_full_chapter(client, [], chapter)
        assert isinstance(result, FullChapterResponse)

    @pytest.mark.anyio
    @pytest.mark.parametrize("num_chunks", [1, 3, 10])
    async def test_various_chunk_counts(self, num_chunks: int) -> None:
        """generate_full_chapter works regardless of how many chunks are supplied."""
        client = _client_returning(_make_full_chapter())
        chapter = ChapterSkeleton(title="Ziua 1", core_concept="Concept", day=1)
        chunks = [f"chunk {i} " * 100 for i in range(num_chunks)]

        result = await service.generate_full_chapter(client, chunks, chapter)
        assert isinstance(result, FullChapterResponse)

    @pytest.mark.anyio
    async def test_rate_limit_retries_and_succeeds(self) -> None:
        """generate_full_chapter retries on RateLimitError and succeeds on second attempt."""
        full = _make_full_chapter()
        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(side_effect=[_rate_limit_error(), make_parsed_response(full)])
        chapter = ChapterSkeleton(title="Ziua 1", core_concept="Concept", day=1)

        with patch("app.document_reader.service._retry", _FAST_RETRY):
            result = await service.generate_full_chapter(client, ["chunk"], chapter)

        assert isinstance(result, FullChapterResponse)
        assert client.beta.chat.completions.parse.call_count == 2

    @pytest.mark.anyio
    async def test_exhausted_retries_reraise(self) -> None:
        """generate_full_chapter re-raises RateLimitError after all retry attempts."""
        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(side_effect=_rate_limit_error())
        chapter = ChapterSkeleton(title="Ziua 1", core_concept="Concept", day=1)

        with patch("app.document_reader.service._retry", _FAST_RETRY), pytest.raises(RateLimitError):
            await service.generate_full_chapter(client, ["chunk"], chapter)


# ---------------------------------------------------------------------------
# Unit tests _select_chunks
# ---------------------------------------------------------------------------


class TestUnitSelectChunks:
    """Unit tests for the private _select_chunks helper."""

    def test_empty_chunks_returns_empty_string(self) -> None:
        """_select_chunks returns empty string for an empty list."""
        assert service._select_chunks([], "query") == ""

    def test_single_chunk_returned_if_fits(self) -> None:
        """A single small chunk is returned as-is."""
        assert "hello world" in service._select_chunks(["hello world"], "hello")

    def test_total_length_does_not_exceed_max_chars(self) -> None:
        """Returned string does not exceed max_chars (plus join overhead)."""
        chunks = ["word " * 500 for _ in range(10)]
        result = service._select_chunks(chunks, "word", max_chars=6000)
        assert len(result) <= 6500

    def test_relevant_chunks_are_prioritised(self) -> None:
        """Chunks containing query words score higher than irrelevant chunks."""
        chunks = ["irrelevant content", "Python tutorial introduction", "Python basics"]
        assert "Python" in service._select_chunks(chunks, "Python")

    def test_fallback_to_first_chunk_when_none_match(self) -> None:
        """When no chunk matches, the first chunk is returned truncated."""
        result = service._select_chunks(["abcdefghij"], "zzz", max_chars=5)
        assert len(result) <= 5


# ---------------------------------------------------------------------------
# Integration tests
# ---------------------------------------------------------------------------


class TestIntegrationDocumentReaderService:
    """Integration tests: analyze → generate_full_chapter pipeline."""

    @pytest.mark.anyio
    async def test_analyze_then_generate_full_chapter(self) -> None:
        """Chapters from analyze_and_skeleton are valid inputs for generate_full_chapter."""
        aas = _make_aas(n=1)
        full = _make_full_chapter()
        responses: list[Any] = [make_parsed_response(aas), make_parsed_response(full)]
        idx = [0]

        def _dispatch(**_kwargs: Any) -> Any:
            r = responses[idx[0]]
            idx[0] += 1
            return r

        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(side_effect=_dispatch)

        content = _doc_content()
        plan = await service.analyze_and_skeleton(client, content)

        from app.document_reader.extractor import chunk_text

        result = await service.generate_full_chapter(client, chunk_text(content.text), plan.skeleton.chapters[0])
        assert isinstance(result, FullChapterResponse)
        assert len(result.subchapters) >= 1

    @pytest.mark.anyio
    @pytest.mark.parametrize("text_length", [100, 5000, 15000])
    async def test_various_document_lengths(self, text_length: int) -> None:
        """Service handles short, medium, and long documents without error."""
        client = _client_returning(_make_aas(1))
        text = "Word content here. " * (text_length // 20)
        content = DocumentContent(filename="doc.pdf", total_pages=1, text=text, pages=[text])
        result = await service.analyze_and_skeleton(client, content)
        assert isinstance(result, AnalysisAndSkeleton)
