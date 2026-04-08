import pytest
from unittest.mock import AsyncMock, patch
from openai import RateLimitError
import httpx

from app.document_reader.schemas import AnalysisAndSkeleton, FullChapterResponse, DocumentAnalysis, LearningPlanSkeleton
from app.courses.schemas import ChapterSkeleton
from app.document_reader.extractor import DocumentContent
from app.document_reader.service import (
    _select_chunks,
    analyze_and_skeleton,
    generate_full_chapter,
)
from tests.conftest import make_parsed_response


def get_rate_limit_error() -> RateLimitError:
    request = httpx.Request("POST", "https://api.openai.com/v1/chat/completions")
    response = httpx.Response(429, request=request)
    return RateLimitError("Rate limited", response=response, body=None)


@pytest.fixture
def mock_document_content() -> DocumentContent:
    return DocumentContent(
        filename="test.pdf", 
        total_pages=10, 
        text="A" * 15000, 
        pages=["A" * 15000]
    )


def test_select_chunks_empty_list() -> None:
    assert _select_chunks([], "query") == ""


def test_select_chunks_sorting_logic() -> None:
    chunks = [
        "This chunk is entirely about apples and bananas.",
        "This chunk talks about cars.",
        "This chunk mentions apples once."
    ]
    query = "apples and bananas"
    result = _select_chunks(chunks, query, max_chars=100)
    
    assert "entirely about apples" in result
    assert "mentions apples once" in result
    assert "cars" not in result


def test_select_chunks_max_chars_cutoff() -> None:
    chunks = ["Chunk one has exactly 32 chars.", "Chunk two has exactly 32 chars."]
    query = "chunk"
    result = _select_chunks(chunks, query, max_chars=50)
    
    assert "Chunk one" in result
    assert "Chunk two" not in result


def test_select_chunks_fallback_large_first_chunk() -> None:
    chunks = [
        "This is an initial default chunk that is very very long.",
        "A highly relevant chunk but it is incredibly long and exceeds the max limit immediately."
    ]
    query = "relevant"
    result = _select_chunks(chunks, query, max_chars=20)
    assert result == chunks[0][:20]


@pytest.mark.asyncio
async def test_analyze_and_skeleton_happy_path(mock_openai_client: AsyncMock, mock_document_content: DocumentContent) -> None:
    expected_data = AnalysisAndSkeleton(
        analysis=DocumentAnalysis(title="A", summary="B", key_topics=["C"]),
        skeleton=LearningPlanSkeleton(topic="D", chapters=[])
    )
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(expected_data)
    
    result = await analyze_and_skeleton(mock_openai_client, mock_document_content)
    
    assert result == expected_data
    call_args = mock_openai_client.beta.chat.completions.parse.await_args[1]
    assert len(call_args["messages"][1]["content"]) < 12100 


@pytest.mark.asyncio
async def test_analyze_and_skeleton_parse_failure(mock_openai_client: AsyncMock, mock_document_content: DocumentContent) -> None:
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(None)
    with pytest.raises(RuntimeError, match="OpenAI failed to parse AnalysisAndSkeleton"):
        await analyze_and_skeleton(mock_openai_client, mock_document_content)


@pytest.mark.asyncio
@patch("asyncio.sleep", new_callable=AsyncMock)
async def test_analyze_and_skeleton_retry_logic(mock_sleep: AsyncMock, mock_openai_client: AsyncMock, mock_document_content: DocumentContent) -> None:
    expected_data = AnalysisAndSkeleton(
        analysis=DocumentAnalysis(title="A", summary="B", key_topics=["C"]),
        skeleton=LearningPlanSkeleton(topic="D", chapters=[])
    )
    
    mock_openai_client.beta.chat.completions.parse.side_effect = [
        get_rate_limit_error(),
        make_parsed_response(expected_data)
    ]
    
    result = await analyze_and_skeleton(mock_openai_client, mock_document_content)
    
    assert result == expected_data
    assert mock_openai_client.beta.chat.completions.parse.call_count == 2
    mock_sleep.assert_called_once()


@pytest.mark.asyncio
async def test_generate_full_chapter_happy_path(mock_openai_client: AsyncMock) -> None:
    expected_data = FullChapterResponse(subchapters=[], recap_quiz=[])
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(expected_data)
    
    mock_chapter = ChapterSkeleton(title="Intro", core_concept="Basics")
    
    chunks = ["Irrelevant chunk", "Intro to Basics chunk"]
    result = await generate_full_chapter(mock_openai_client, chunks, mock_chapter)
    
    assert result == expected_data
    call_args = mock_openai_client.beta.chat.completions.parse.await_args[1]
    prompt_content = call_args["messages"][1]["content"]
    assert "Intro to Basics chunk" in prompt_content


@pytest.mark.asyncio
async def test_generate_full_chapter_parse_failure(mock_openai_client: AsyncMock) -> None:
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(None)
    mock_chapter = ChapterSkeleton(title="Intro", core_concept="Basics")
    
    with pytest.raises(RuntimeError, match="OpenAI failed to parse FullChapterResponse"):
        await generate_full_chapter(mock_openai_client, ["Chunk"], mock_chapter)