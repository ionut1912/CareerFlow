import pytest
from unittest.mock import AsyncMock, patch
import httpx
from openai import RateLimitError

from app.courses.schemas import (
    ChapterSkeleton,
    ExpandedDay,
    LearningPlanSkeleton,
    SubchapterContentResponse,
    SubchapterSkeleton,
    ChapterQuizResponse,
)
from app.courses.service import (
    generate_skeleton,
    _expand_chapter,
    _generate_subchapter_content,
    _generate_chapter_quiz,
    build_full_chapter,
)
from tests.conftest import make_parsed_response


def get_rate_limit_error() -> RateLimitError:
    request = httpx.Request("POST", "https://api.openai.com/v1/chat/completions")
    response = httpx.Response(429, request=request)
    return RateLimitError("Rate limited", response=response, body=None)


@pytest.mark.asyncio
async def test_generate_skeleton_happy_path(mock_openai_client: AsyncMock) -> None:
    expected_plan = LearningPlanSkeleton(topic="Python", chapters=[])
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(expected_plan)
    
    result = await generate_skeleton(mock_openai_client, "Python")
    assert result == expected_plan
    mock_openai_client.beta.chat.completions.parse.assert_called_once()


@pytest.mark.asyncio
async def test_generate_skeleton_parsing_failure(mock_openai_client: AsyncMock) -> None:
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(None)
    with pytest.raises(RuntimeError, match="OpenAI failed to parse LearningPlanSkeleton"):
        await generate_skeleton(mock_openai_client, "Python")


@pytest.mark.asyncio
@patch("asyncio.sleep", new_callable=AsyncMock)
async def test_generate_skeleton_retries_on_rate_limit(mock_sleep: AsyncMock, mock_openai_client: AsyncMock) -> None:
    expected_plan = LearningPlanSkeleton(topic="Python", chapters=[])
    mock_openai_client.beta.chat.completions.parse.side_effect = [
        get_rate_limit_error(),
        make_parsed_response(expected_plan)
    ]
    
    result = await generate_skeleton(mock_openai_client, "Python")
    assert result == expected_plan
    assert mock_openai_client.beta.chat.completions.parse.call_count == 2
    mock_sleep.assert_called_once()


@pytest.mark.asyncio
async def test_expand_chapter_happy_path(mock_openai_client: AsyncMock) -> None:
    chapter = ChapterSkeleton(title="Ch1", core_concept="Intro")
    expected_expanded = ExpandedDay(subchapters=[])
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(expected_expanded)
    
    result = await _expand_chapter(mock_openai_client, "Python", chapter)
    assert result == expected_expanded


@pytest.mark.asyncio
async def test_expand_chapter_parsing_failure(mock_openai_client: AsyncMock) -> None:
    chapter = ChapterSkeleton(title="Ch1", core_concept="Intro")
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(None)
    with pytest.raises(RuntimeError, match="OpenAI failed to parse ExpandedDay"):
        await _expand_chapter(mock_openai_client, "Python", chapter)


@pytest.mark.asyncio
async def test_generate_subchapter_content_happy_path(mock_openai_client: AsyncMock) -> None:
    subchapter = SubchapterSkeleton(title="Sub1", content_summary="Sum")
    expected_content = SubchapterContentResponse(theory_html="<p>Test</p>", quiz=[])
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(expected_content)
    
    result = await _generate_subchapter_content(mock_openai_client, "Python", "Ch1", subchapter)
    assert result == expected_content


@pytest.mark.asyncio
async def test_generate_chapter_quiz_happy_path(mock_openai_client: AsyncMock) -> None:
    subchapters = [SubchapterSkeleton(title="Sub1", content_summary="Sum")]
    expected_quiz = ChapterQuizResponse(questions=[])
    mock_openai_client.beta.chat.completions.parse.return_value = make_parsed_response(expected_quiz)
    
    result = await _generate_chapter_quiz(mock_openai_client, "Python", "Ch1", subchapters)
    assert result == expected_quiz


@pytest.mark.asyncio
@patch("app.courses.service._generate_chapter_quiz")
@patch("app.courses.service._generate_subchapter_content")
@patch("app.courses.service._expand_chapter")
async def test_build_full_chapter_orchestration(
    mock_expand: AsyncMock, 
    mock_gen_subcontent: AsyncMock, 
    mock_gen_quiz: AsyncMock, 
    mock_openai_client: AsyncMock
) -> None:
    chapter = ChapterSkeleton(title="Ch1", core_concept="Intro")
    sub1 = SubchapterSkeleton(title="Sub1", content_summary="Sum1")
    sub2 = SubchapterSkeleton(title="Sub2", content_summary="Sum2")
    expanded_day = ExpandedDay(subchapters=[sub1, sub2])
    mock_expand.return_value = expanded_day
    
    mock_subcontent_response = SubchapterContentResponse(theory_html="<p>Data</p>", quiz=[])
    mock_gen_subcontent.return_value = mock_subcontent_response
    
    mock_quiz_response = ChapterQuizResponse(questions=[])
    mock_gen_quiz.return_value = mock_quiz_response
    
    result = await build_full_chapter(mock_openai_client, "Python", chapter)
    
    mock_expand.assert_awaited_once_with(mock_openai_client, "Python", chapter)
    assert mock_gen_subcontent.call_count == 2
    mock_gen_quiz.assert_awaited_once_with(mock_openai_client, "Python", "Ch1", expanded_day.subchapters)
    
    assert result["chapter"] == chapter.model_dump()
    assert result["expanded"] == expanded_day.model_dump()
    assert len(result["subchapter_contents"]) == 2