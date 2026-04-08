from __future__ import annotations

import asyncio
from collections.abc import Awaitable
from typing import Any, TypeVar

from openai import AsyncOpenAI, RateLimitError
from tenacity import retry, retry_if_exception_type, stop_after_attempt, wait_exponential

from app.courses.schemas import (
    ChapterQuizResponse,
    ChapterSkeleton,
    ExpandedDay,
    LearningPlanSkeleton,
    SubchapterContentResponse,
    SubchapterSkeleton,
)

_ai_semaphore = asyncio.Semaphore(12)

_retry = retry(
    retry=retry_if_exception_type(RateLimitError),
    wait=wait_exponential(multiplier=2, min=4, max=120),
    stop=stop_after_attempt(8),
    reraise=True,
)

T = TypeVar("T")


async def _throttled(coro: Awaitable[T]) -> T:
    async with _ai_semaphore:
        return await coro


async def generate_skeleton(client: AsyncOpenAI, topic: str) -> LearningPlanSkeleton:
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> LearningPlanSkeleton:
        response = await _throttled(_parse(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": (
                    "Ești un expert în educație. "
                    "Mai întâi estimează câte zile sunt necesare pentru a învăța subiectul "
                    "de la zero la expert (ritm de 2-3 ore/zi, maxim 90 de zile). "
                    "Apoi creează planul de învățare progresiv cu exact acel număr de zile. "
                    "RĂSPUNDE STRICT ÎN LIMBA ROMÂNĂ."
                )},
                {"role": "user", "content": f"Vreau un plan de la zero la expert pentru: {topic}"},
            ],
            response_format=LearningPlanSkeleton,
        ))
        parsed = response.choices[0].message.parsed
        if parsed is None:
            raise RuntimeError("OpenAI failed to parse LearningPlanSkeleton")
        return parsed

    return await _call()


async def _expand_chapter(client: AsyncOpenAI, topic: str, chapter: ChapterSkeleton) -> ExpandedDay:
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> ExpandedDay:
        response = await _throttled(_parse(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": (
                    "Împarte subiectul zilei în subcapitole logice. "
                    "TOATE TITLURILE ȘI DESCRIERILE TREBUIE SĂ FIE STRICT ÎN LIMBA ROMÂNĂ."
                )},
                {"role": "user", "content": (
                    f"Curs: {topic}\nSubiect zi: {chapter.title}\n"
                    f"Concept: {chapter.core_concept}\nGenerează subcapitolele."
                )},
            ],
            response_format=ExpandedDay,
        ))
        parsed = response.choices[0].message.parsed
        if parsed is None:
            raise RuntimeError("OpenAI failed to parse ExpandedDay")
        return parsed

    return await _call()


async def _generate_subchapter_content(
    client: AsyncOpenAI, topic: str, chapter_title: str, subchapter: SubchapterSkeleton
) -> SubchapterContentResponse:
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> SubchapterContentResponse:
        response = await _throttled(_parse(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": (
                    "Ești un profesor expert. Generează teorie detaliată formatată în HTML "
                    "(folosind tag-uri h2, h3, p, ul, li, b, i) și un mini quiz cu exact 3 întrebări "
                    "(cu 4 opțiuni fiecare). TOTUL STRICT ÎN LIMBA ROMÂNĂ."
                )},
                {"role": "user", "content": (
                    f"Curs: {topic}\nCapitol: {chapter_title}\n"
                    f"Subcapitol: {subchapter.title}\nDescriere: {subchapter.content_summary}\n\n"
                    "Generează conținutul teoretic (HTML) și quiz-ul."
                )},
            ],
            response_format=SubchapterContentResponse,
        ))
        parsed = response.choices[0].message.parsed
        if parsed is None:
            raise RuntimeError("OpenAI failed to parse SubchapterContentResponse")
        return parsed

    return await _call()


async def _generate_chapter_quiz(
    client: AsyncOpenAI, topic: str, chapter_title: str, subchapters: list[SubchapterSkeleton]
) -> ChapterQuizResponse:
    subchapters_text = "\n".join(
        f"- {s.title}: {s.content_summary}" for s in subchapters
    )
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> ChapterQuizResponse:
        response = await _throttled(_parse(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": (
                    "Generează un quiz recapitulativ de final de capitol cu exact 10 întrebări. "
                    "Fiecare întrebare trebuie să aibă 4 variante de răspuns. "
                    "TOATE ÎNTREBĂRILE ȘI RĂSPUNSURILE EXCLUSIV ÎN LIMBA ROMÂNĂ."
                )},
                {"role": "user", "content": (
                    f"Curs: {topic}\nCapitol: {chapter_title}\n"
                    f"Subcapitole acoperite:\n{subchapters_text}\n\nGenerează quiz-ul de 10 întrebări."
                )},
            ],
            response_format=ChapterQuizResponse,
        ))
        parsed = response.choices[0].message.parsed
        if parsed is None:
            raise RuntimeError("OpenAI failed to parse ChapterQuizResponse")
        return parsed

    return await _call()


async def build_full_chapter(client: AsyncOpenAI, topic: str, chapter: ChapterSkeleton) -> dict[str, Any]:
    expanded = await _expand_chapter(client, topic, chapter)

    tasks = [
        asyncio.create_task(_generate_subchapter_content(client, topic, chapter.title, sub))
        for sub in expanded.subchapters
    ]
    quiz_task = asyncio.create_task(
        _generate_chapter_quiz(client, topic, chapter.title, expanded.subchapters)
    )

    subchapter_contents = await asyncio.gather(*tasks)
    quiz = await quiz_task

    return {
        "chapter": chapter.model_dump(),
        "expanded": expanded.model_dump(),
        "subchapter_contents": [sc.model_dump() for sc in subchapter_contents],
        "quiz": quiz.model_dump(),
    }