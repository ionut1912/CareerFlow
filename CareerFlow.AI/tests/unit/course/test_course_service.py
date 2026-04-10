"""Tests for app/courses/service.py"""
from __future__ import annotations

from typing import Any
from unittest.mock import AsyncMock, MagicMock, patch

import pytest
from openai import RateLimitError
from tenacity import retry, retry_if_exception_type, stop_after_attempt, wait_none

from app.courses import service
from app.courses.schemas import (
    ChapterSkeleton,
    LearningPlanSkeleton,
)
from tests.conftest import (
    make_chapter_quiz_response,
    make_chapter_skeleton,
    make_expanded_day,
    make_learning_plan_skeleton,
    make_parsed_response,
    make_subchapter_content_response,
)

# ---------------------------------------------------------------------------
# Fast retry: identical behaviour to service._retry but with wait_none().
# Patch app.courses.service._retry with this during any retry-path test to
# prevent tenacity's exponential back-off from blocking the runner.
# ---------------------------------------------------------------------------
_FAST_RETRY = retry(
    retry=retry_if_exception_type(RateLimitError),
    wait=wait_none(),
    stop=stop_after_attempt(8),
    reraise=True,
)


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _client_returning(obj: object) -> AsyncMock:
    client = AsyncMock()
    client.beta.chat.completions.parse = AsyncMock(
        return_value=make_parsed_response(obj)
    )
    return client


def _rate_limit_error() -> RateLimitError:
    return RateLimitError("rate limit", response=MagicMock(status_code=429), body={})


def _make_dispatch(responses: list[Any]) -> AsyncMock:
    """Return a client whose parse mock pops the next response on each call."""
    idx = [0]

    def _dispatch(**_kwargs: Any) -> Any:
        r = responses[idx[0]]
        idx[0] += 1
        return make_parsed_response(r)

    client = AsyncMock()
    client.beta.chat.completions.parse = AsyncMock(side_effect=_dispatch)
    return client


# ---------------------------------------------------------------------------
# Unit tests  generate_skeleton
# ---------------------------------------------------------------------------

class TestUnitGenerateSkeleton:
    """Unit tests for service.generate_skeleton."""

    @pytest.mark.anyio
    async def test_happy_path_returns_learning_plan_skeleton(self) -> None:
        """generate_skeleton returns a LearningPlanSkeleton on success."""
        client = _client_returning(make_learning_plan_skeleton(topic="Python", num_chapters=3))
        result = await service.generate_skeleton(client, "Python")

        assert isinstance(result, LearningPlanSkeleton)
        assert result.topic == "Python"
        assert len(result.chapters) == 3

    @pytest.mark.anyio
    async def test_calls_parse_with_correct_response_format(self) -> None:
        """generate_skeleton passes response_format=LearningPlanSkeleton."""
        client = _client_returning(make_learning_plan_skeleton())
        await service.generate_skeleton(client, "Rust")

        _, kwargs = client.beta.chat.completions.parse.call_args
        assert kwargs.get("response_format") is LearningPlanSkeleton

    @pytest.mark.anyio
    async def test_raises_runtime_error_when_parsed_is_none(self) -> None:
        """generate_skeleton raises RuntimeError if OpenAI returns parsed=None."""
        with pytest.raises(RuntimeError, match="LearningPlanSkeleton"):
            await service.generate_skeleton(_client_returning(None), "Python")

    @pytest.mark.anyio
    async def test_topic_forwarded_to_user_message(self) -> None:
        """The user message in the API call contains the requested topic."""
        client = _client_returning(make_learning_plan_skeleton(topic="Docker"))
        await service.generate_skeleton(client, "Docker")

        _, kwargs = client.beta.chat.completions.parse.call_args
        user_content = next(
            m["content"]
            for m in kwargs.get("messages", [])
            if m["role"] == "user"
        )
        assert "Docker" in user_content

    @pytest.mark.anyio
    async def test_rate_limit_retries_and_succeeds(self) -> None:
        """generate_skeleton retries on RateLimitError and returns on second attempt.

        _retry is patched with wait_none() so tenacity never calls asyncio.sleep.
        """
        skeleton = make_learning_plan_skeleton()
        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(
            side_effect=[_rate_limit_error(), make_parsed_response(skeleton)]
        )

        with patch("app.courses.service._retry", _FAST_RETRY):
            result = await service.generate_skeleton(client, "Python")

        assert isinstance(result, LearningPlanSkeleton)
        assert client.beta.chat.completions.parse.call_count == 2

    @pytest.mark.anyio
    async def test_exhausted_retries_reraise(self) -> None:
        """generate_skeleton re-raises RateLimitError after all 8 retry attempts.
        """
        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(side_effect=_rate_limit_error())

        with patch("app.courses.service._retry", _FAST_RETRY), pytest.raises(RateLimitError):
            await service.generate_skeleton(client, "Python")

    @pytest.mark.anyio
    @pytest.mark.parametrize("num_chapters", [1, 5, 10])
    async def test_various_chapter_counts(self, num_chapters: int) -> None:
        """generate_skeleton returns plans with the correct chapter count."""
        client = _client_returning(make_learning_plan_skeleton(num_chapters=num_chapters))
        result = await service.generate_skeleton(client, "Topic")
        assert len(result.chapters) == num_chapters


# ---------------------------------------------------------------------------
# Unit tests  build_full_chapter
# ---------------------------------------------------------------------------

class TestUnitBuildFullChapter:
    """Unit tests for service.build_full_chapter."""

    @pytest.mark.anyio
    async def test_happy_path_returns_dict_with_expected_keys(self) -> None:
        """build_full_chapter returns dict with chapter/expanded/subchapter_contents/quiz."""
        chapter = make_chapter_skeleton()
        expanded = make_expanded_day(num_subchapters=2)
        sub = make_subchapter_content_response()
        quiz = make_chapter_quiz_response()

        client = _make_dispatch([expanded, sub, sub, quiz])
        result = await service.build_full_chapter(client, "Python", chapter)

        assert set(result.keys()) == {"chapter", "expanded", "subchapter_contents", "quiz"}
        assert len(result["subchapter_contents"]) == 2
        assert len(result["quiz"]["questions"]) == 10

    @pytest.mark.anyio
    async def test_subchapter_content_count_matches_expanded(self) -> None:
        """Number of subchapter_content items equals subchapters in ExpandedDay."""
        chapter = make_chapter_skeleton()
        expanded = make_expanded_day(num_subchapters=3)
        sub = make_subchapter_content_response()
        quiz = make_chapter_quiz_response()

        client = _make_dispatch([expanded, sub, sub, sub, quiz])
        result = await service.build_full_chapter(client, "Python", chapter)
        assert len(result["subchapter_contents"]) == 3

    @pytest.mark.anyio
    async def test_expand_parsed_none_raises(self) -> None:
        """If _expand_chapter gets parsed=None it raises RuntimeError."""
        with pytest.raises(RuntimeError):
            await service.build_full_chapter(_client_returning(None), "Python", make_chapter_skeleton())

    @pytest.mark.anyio
    async def test_chapter_data_preserved_in_result(self) -> None:
        """The 'chapter' key in the result matches the ChapterSkeleton passed in."""
        chapter = ChapterSkeleton(title="Custom Title", core_concept="Custom Concept")
        expanded = make_expanded_day(num_subchapters=1)
        sub = make_subchapter_content_response()
        quiz = make_chapter_quiz_response()

        client = _make_dispatch([expanded, sub, quiz])
        result = await service.build_full_chapter(client, "Python", chapter)
        assert result["chapter"]["title"] == "Custom Title"
        assert result["chapter"]["core_concept"] == "Custom Concept"

    @pytest.mark.anyio
    async def test_subchapter_content_has_theory_and_quiz(self) -> None:
        """Each subchapter_content entry contains theory_html and quiz fields."""
        chapter = make_chapter_skeleton()
        expanded = make_expanded_day(num_subchapters=1)
        sub = make_subchapter_content_response()
        quiz = make_chapter_quiz_response()

        client = _make_dispatch([expanded, sub, quiz])
        result = await service.build_full_chapter(client, "Python", chapter)
        sc = result["subchapter_contents"][0]
        assert "theory_html" in sc
        assert "quiz" in sc

    @pytest.mark.anyio
    async def test_expand_chapter_rate_limit_retries(self) -> None:
        """_expand_chapter retries on RateLimitError inside build_full_chapter.

        _retry is patched with wait_none() to prevent real back-off delays.
        """
        expanded = make_expanded_day(num_subchapters=1)
        sub = make_subchapter_content_response()
        quiz = make_chapter_quiz_response()

        client: AsyncMock = AsyncMock()
        client.beta.chat.completions.parse = AsyncMock(
            side_effect=[
                _rate_limit_error(),
                make_parsed_response(expanded),
                make_parsed_response(sub),
                make_parsed_response(quiz),
            ]
        )

        with patch("app.courses.service._retry", _FAST_RETRY):
            result = await service.build_full_chapter(
                client, "Python", make_chapter_skeleton()
            )

        assert len(result["subchapter_contents"]) == 1


# ---------------------------------------------------------------------------
# Integration tests
# ---------------------------------------------------------------------------

class TestIntegrationCoursesService:
    """Integration: generate_skeleton → build_full_chapter pipeline."""

    @pytest.mark.anyio
    async def test_skeleton_feeds_build_full_chapter(self) -> None:
        """Chapters produced by generate_skeleton are valid inputs for build_full_chapter."""
        skeleton = make_learning_plan_skeleton(num_chapters=1)
        expanded = make_expanded_day(num_subchapters=1)
        sub = make_subchapter_content_response()
        quiz = make_chapter_quiz_response()

        client = _make_dispatch([skeleton, expanded, sub, quiz])
        plan = await service.generate_skeleton(client, "Python")
        result = await service.build_full_chapter(client, "Python", plan.chapters[0])

        assert result["chapter"]["title"] == plan.chapters[0].title

    @pytest.mark.anyio
    @pytest.mark.parametrize("num_subs", [1, 2, 4])
    async def test_build_full_chapter_various_subchapter_counts(
        self, num_subs: int
    ) -> None:
        """build_full_chapter handles 1, 2, and 4 subchapters correctly."""
        chapter = make_chapter_skeleton()
        expanded = make_expanded_day(num_subchapters=num_subs)
        sub = make_subchapter_content_response()
        quiz = make_chapter_quiz_response()

        client = _make_dispatch([expanded] + [sub] * num_subs + [quiz])
        result = await service.build_full_chapter(client, "Python", chapter)
        assert len(result["subchapter_contents"]) == num_subs