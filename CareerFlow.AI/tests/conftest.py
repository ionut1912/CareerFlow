from __future__ import annotations

from collections import deque
from collections.abc import AsyncGenerator, Callable
from typing import Any
from unittest.mock import AsyncMock, MagicMock

import pytest
from httpx import ASGITransport, AsyncClient

# Adjust 'main' to match where your FastAPI 'app' instance actually lives
from main import app 
from app.dependencies import get_openai_client
from app.courses.schemas import (
    ChapterQuizResponse,
    ChapterSkeleton,
    ExpandedDay,
    LearningPlanSkeleton,
    QuizQuestion,
    QuizOption,
    SubchapterContentResponse,
    SubchapterSkeleton,
)


def make_parsed_response(parsed_object: object) -> MagicMock:
    message = MagicMock()
    message.parsed = parsed_object
    choice = MagicMock()
    choice.message = message
    completion = MagicMock()
    completion.choices = [choice]
    return completion


def make_format_dispatcher(
    responses_by_format: dict[type, list[Any]],
) -> Callable[..., MagicMock]:
    queues: dict[type, deque[Any]] = {
        fmt: deque(items) for fmt, items in responses_by_format.items()
    }

    def _dispatch(**kwargs: Any) -> MagicMock:
        rf = kwargs.get("response_format")
        if not isinstance(rf, type):
            raise ValueError(f"response_format must be a type, got {rf!r}")
        q = queues.get(rf)
        if not q:
            raise ValueError(f"No mock response for response_format={rf!r}")
        return make_parsed_response(q.popleft())

    return _dispatch


def make_quiz_option(label: str = "Un limbaj de programare", is_correct: bool = True) -> QuizOption:
    return QuizOption(label=label, is_correct=is_correct)


def make_quiz_question(
    question: str = "Ce este Python?",
    options: list[QuizOption] | None = None,
) -> QuizQuestion:
    return QuizQuestion(
        question=question,
        options=options or [
            make_quiz_option(),
            make_quiz_option(label="Un sarpe", is_correct=False),
            make_quiz_option(label="Un editor", is_correct=False),
            make_quiz_option(label="Un OS", is_correct=False),
        ],
    )


def make_subchapter_skeleton(
    title: str = "Introducere in Python",
    content_summary: str = "Bazele limbajului Python.",
) -> SubchapterSkeleton:
    return SubchapterSkeleton(
        title=title,
        content_summary=content_summary,
    )


def make_chapter_skeleton(
    title: str = "Ziua 1: Fundamente",
    core_concept: str = "Sintaxa de baza",
) -> ChapterSkeleton:
    return ChapterSkeleton(title=title, core_concept=core_concept)


def make_learning_plan_skeleton(
    topic: str = "Python",
    num_chapters: int = 2,
) -> LearningPlanSkeleton:
    chapters = [
        make_chapter_skeleton(title=f"Ziua {i + 1}")
        for i in range(num_chapters)
    ]
    return LearningPlanSkeleton(topic=topic, chapters=chapters)


def make_expanded_day(num_subchapters: int = 2) -> ExpandedDay:
    return ExpandedDay(
        subchapters=[
            make_subchapter_skeleton(title=f"Subcapitol {i + 1}")
            for i in range(num_subchapters)
        ],
    )


def make_subchapter_content_response() -> SubchapterContentResponse:
    return SubchapterContentResponse(
        theory_html="<h2>Teoria</h2><p>Continut.</p>",
        quiz=[make_quiz_question() for _ in range(3)],
    )


def make_chapter_quiz_response() -> ChapterQuizResponse:
    return ChapterQuizResponse(
        questions=[make_quiz_question(question=f"Intrebarea {i + 1}?") for i in range(10)],
    )


def wire_course(
    mock_client: AsyncMock,
    num_days: int = 1,
    num_subchapters: int = 1,
) -> None:
    skeleton = make_learning_plan_skeleton(num_chapters=num_days)
    mock_client.beta.chat.completions.parse.side_effect = make_format_dispatcher(
        {
            LearningPlanSkeleton: [skeleton],
            ExpandedDay: [
                make_expanded_day(num_subchapters=num_subchapters)
                for _ in range(num_days)
            ],
            SubchapterContentResponse: [
                make_subchapter_content_response()
                for _ in range(num_days * num_subchapters)
            ],
            ChapterQuizResponse: [make_chapter_quiz_response() for _ in range(num_days)],
        }
    )


@pytest.fixture
def mock_openai_client() -> AsyncMock:
    client = AsyncMock()
    client.beta = AsyncMock()
    client.beta.chat = AsyncMock()
    client.beta.chat.completions = AsyncMock()
    return client


@pytest.fixture
async def api_client(mock_openai_client: AsyncMock) -> AsyncGenerator[AsyncClient, None]:
    app.dependency_overrides[get_openai_client] = lambda: mock_openai_client
    async with AsyncClient(
        transport=ASGITransport(app=app, raise_app_exceptions=False),
        base_url="http://test",
    ) as client:
        yield client
    app.dependency_overrides.clear()