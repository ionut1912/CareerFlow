from __future__ import annotations

import asyncio
from collections.abc import Awaitable
from typing import Any, TypeVar

from openai import AsyncOpenAI, RateLimitError
from tenacity import retry, retry_if_exception_type, stop_after_attempt, wait_exponential

from app.courses.schema import (
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
        response = await _throttled(
            _parse(
                model="gpt-4o-mini",
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "Fă două lucruri simultan:\n"
                            "1. ESTIMARE ZILE: Estimează câte zile sunt necesare pentru a învăța subiectul "
                            "de la zero la expert (ritm de 2-3 ore/zi, minim 1, maxim 90 de zile). "
                            "Alege un număr realist.\n"
                            "2. PLAN: Creează planul de învățare progresiv cu exact acel număr de zile.\n"
                            "OBLIGATORIU — CÂMPURI COMPLETE: Fiecare câmp din răspuns trebuie completat. "
                            "Niciun câmp nu poate fi gol (\"\"), null, sau listă vidă ([]). "
                            "Dacă informația nu reiese explicit, deduce sau sintetizează din context "
                            "— dar nu lăsa absolut nimic necompletat.\n"
                            "RĂSPUNDE ÎN ROMÂNĂ."
                        ),
                    },
                    {"role": "user", "content": f"Vreau un plan de la zero la expert pentru: {topic}"},
                ],
                response_format=LearningPlanSkeleton,
            )
        )
        parsed = response.choices[0].message.parsed
        if parsed is None:
            raise RuntimeError("OpenAI failed to parse LearningPlanSkeleton")
        return parsed

    return await _call()


async def _expand_chapter(client: AsyncOpenAI, topic: str, chapter: ChapterSkeleton) -> ExpandedDay:
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> ExpandedDay:
        response = await _throttled(
            _parse(
                model="gpt-4o-mini",
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "Împarte capitolul în 2-4 subcapitole logice pe baza subiectului "
                            "și conceptului central.\n"
                            "OBLIGATORIU — CÂMPURI COMPLETE: Fiecare câmp din răspuns trebuie completat. "
                            "Niciun câmp nu poate fi gol (\"\"), null, sau listă vidă ([]). "
                            "Fiecare subcapitol trebuie să aibă titlu și descriere completă a conținutului "
                            "— nu lăsa absolut nimic necompletat.\n"
                            "TOTUL ÎN ROMÂNĂ."
                        ),
                    },
                    {
                        "role": "user",
                        "content": (
                            f"Curs: {topic}\nSubiect zi: {chapter.title}\n"
                            f"Concept: {chapter.core_concept}\nGenerează subcapitolele."
                        ),
                    },
                ],
                response_format=ExpandedDay,
            )
        )
        parsed = response.choices[0].message.parsed
        if parsed is None:
            raise RuntimeError("OpenAI failed to parse ExpandedDay")
        return parsed

    return await _call()


async def _generate_subchapter_content(
    client: AsyncOpenAI,
    topic: str,
    chapter_title: str,
    subchapter: SubchapterSkeleton,
) -> SubchapterContentResponse:
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> SubchapterContentResponse:
        response = await _throttled(
            _parse(
                model="gpt-4o-mini",
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "Generează teorie detaliată formatată în HTML (h2,h3,p,ul,li,b,i) "
                            "și un mini quiz cu 3 întrebări (4 opțiuni).\n\n"
                            "REGULI STRICTE PENTRU ÎNTREBĂRI:\n"
                            "- Titluri sub 100 car. Întrebări sub 300 car. Răspunsuri sub 100 car.\n"
                            "- INTERZIS: întrebări care conțin răspunsul în enunț, întrebări de tipul "
                            "'Care este definiția lui X?' când definiția e evidentă, sau la care se poate "
                            "răspunde fără a parcurge materialul.\n"
                            "- OBLIGATORIU: întrebări care testează relații cauză-efect, comparații "
                            "între concepte, aplicarea cunoștințelor. Cel puțin 50% din întrebări "
                            "trebuie să înceapă cu 'De ce…', 'Ce s-ar întâmpla dacă…', "
                            "'Care este diferența dintre…', 'În ce situație…'.\n"
                            "- RANDOMIZARE RĂSPUNSURI: Pentru mini quiz-ul de 3 întrebări folosește "
                            "tiparul indexului corect: 2,0,1. NU pune răspunsul corect mereu pe prima poziție.\n"
                            "- Distractorii (răspunsurile greșite) trebuie să fie plauzibili, "
                            "nu absurzi — să reflecte greșeli tipice de înțelegere.\n"
                            "OBLIGATORIU — CÂMPURI COMPLETE: Fiecare câmp din răspuns trebuie completat. "
                            "Niciun câmp nu poate fi gol (\"\"), null, sau listă vidă ([]). "
                            "Conținutul HTML nu poate fi gol. Mini quiz-ul trebuie să conțină exact "
                            "3 întrebări, fiecare cu exact 4 opțiuni și un răspuns corect specificat "
                            "— nu lăsa absolut nimic necompletat.\n"
                            "TOTUL ÎN ROMÂNĂ."
                        ),
                    },
                    {
                        "role": "user",
                        "content": (
                            f"Curs: {topic}\nCapitol: {chapter_title}\n"
                            f"Subcapitol: {subchapter.title}\nDescriere: {subchapter.content_summary}\n\n"
                            "Generează conținutul teoretic (HTML) și quiz-ul, respectând cu strictețe "
                            "regulile de calitate și randomizare."
                        ),
                    },
                ],
                response_format=SubchapterContentResponse,
            )
        )
        parsed = response.choices[0].message.parsed
        if parsed is None:
            raise RuntimeError("OpenAI failed to parse SubchapterContentResponse")
        return parsed

    return await _call()


async def _generate_chapter_quiz(
    client: AsyncOpenAI,
    topic: str,
    chapter_title: str,
    subchapters: list[SubchapterSkeleton],
) -> ChapterQuizResponse:
    subchapters_text = "\n".join(f"- {s.title}: {s.content_summary}" for s in subchapters)
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> ChapterQuizResponse:
        response = await _throttled(
            _parse(
                model="gpt-4o-mini",
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "Generează un quiz recapitulativ de final de capitol cu 10 întrebări (4 opțiuni).\n\n"
                            "REGULI STRICTE PENTRU ÎNTREBĂRI:\n"
                            "- Titluri sub 100 car. Întrebări sub 300 car. Răspunsuri sub 100 car.\n"
                            "- INTERZIS: întrebări care conțin răspunsul în enunț, întrebări de tipul "
                            "'Care este definiția lui X?', întrebări cu ani/date deja menționate "
                            "în întrebare, sau la care se poate răspunde din cunoștințe generale.\n"
                            "- OBLIGATORIU: întrebări care testează relații cauză-efect, comparații "
                            "între concepte, aplicarea cunoștințelor în scenarii noi, sau analiza "
                            "consecințelor. Cel puțin 50% din întrebări trebuie să înceapă cu "
                            "'De ce…', 'Ce s-ar întâmpla dacă…', 'Care este diferența dintre…', "
                            "'În ce situație…'.\n"
                            "- RANDOMIZARE RĂSPUNSURI: Pentru fiecare întrebare, alege ALEATORIU "
                            "indexul răspunsului corect (0, 1, 2 sau 3). Urmează strict acest tipar "
                            "pentru cele 10 întrebări recapitulative: pozițiile corecte să fie "
                            "aproximativ: 3,1,0,2,1,3,0,2,0,1. NU pune răspunsul corect mereu pe prima poziție.\n"
                            "- Distractorii (răspunsurile greșite) trebuie să fie plauzibili, "
                            "nu absurzi — să reflecte greșeli tipice de înțelegere.\n"
                            "OBLIGATORIU — CÂMPURI COMPLETE: Fiecare câmp din răspuns trebuie completat. "
                            "Niciun câmp nu poate fi gol (\"\"), null, sau listă vidă ([]). "
                            "Quiz-ul trebuie să conțină exact 10 întrebări, fiecare cu exact 4 opțiuni "
                            "și un răspuns corect specificat — nu lăsa absolut nimic necompletat.\n"
                            "TOTUL ÎN ROMÂNĂ."
                        ),
                    },
                    {
                        "role": "user",
                        "content": (
                            f"Curs: {topic}\nCapitol: {chapter_title}\n"
                            f"Subcapitole acoperite:\n{subchapters_text}\n\n"
                            "Generează quiz-ul de 10 întrebări respectând cu strictețe "
                            "regulile de randomizare și complexitate."
                        ),
                    },
                ],
                response_format=ChapterQuizResponse,
            )
        )
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
    quiz_task = asyncio.create_task(_generate_chapter_quiz(client, topic, chapter.title, expanded.subchapters))

    subchapter_contents = await asyncio.gather(*tasks)
    quiz = await quiz_task

    return {
        "chapter": chapter.model_dump(),
        "expanded": expanded.model_dump(),
        "subchapter_contents": [sc.model_dump() for sc in subchapter_contents],
        "quiz": quiz.model_dump(),
    }