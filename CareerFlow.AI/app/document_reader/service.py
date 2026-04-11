from __future__ import annotations

import asyncio
from collections.abc import Awaitable
from typing import TypeVar

from openai import AsyncOpenAI, RateLimitError
from tenacity import retry, retry_if_exception_type, stop_after_attempt, wait_exponential

from app.courses.schema import ChapterSkeleton
from app.document_reader.extractor import DocumentContent
from app.document_reader.schema import AnalysisAndSkeleton, FullChapterResponse

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


def _select_chunks(chunks: list[str], query: str, max_chars: int = 6000) -> str:
    if not chunks:
        return ""

    query_words = set(query.lower().split())

    scored = sorted(
        chunks,
        key=lambda c: len(query_words.intersection(c.lower().split()[:200])),
        reverse=True,
    )

    selected, length = [], 0
    for c in scored:
        if length + len(c) > max_chars:
            break
        selected.append(c)
        length += len(c)

    return "\n\n".join(selected) or chunks[0][:max_chars]


async def analyze_and_skeleton(client: AsyncOpenAI, content: DocumentContent) -> AnalysisAndSkeleton:
    truncated = content.text[:12000]
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> AnalysisAndSkeleton:
        response = await _throttled(
            _parse(
                model="gpt-4o-mini",
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "Fă trei lucruri simultan:\n"
                            "1. ANALIZĂ: Identifică titlul, rezumat scurt, subiecte cheie\n"
                            "2. ESTIMARE ZILE: Estimează câte zile sunt necesare pentru a învăța "
                            "conținutul documentului de la zero la expert (ritm de 2-3 ore/zi, "
                            "minim 1, maxim 90 de zile). Alege un număr realist.\n"
                            "3. PLAN: Creează planul de învățare progresiv cu exact acel număr de zile.\n"
                            "RĂSPUNDE ÎN ROMÂNĂ."
                        ),
                    },
                    {
                        "role": "user",
                        "content": (f"Document ({content.filename}, {content.total_pages} pag):\n\n{truncated}"),
                    },
                ],
                response_format=AnalysisAndSkeleton,
            )
        )
        parsed_data = response.choices[0].message.parsed
        if parsed_data is None:
            raise RuntimeError("OpenAI failed to parse AnalysisAndSkeleton")
        return parsed_data

    return await _call()


async def generate_full_chapter(
    client: AsyncOpenAI,
    chunks: list[str],
    chapter: ChapterSkeleton,
) -> FullChapterResponse:
    context = await asyncio.to_thread(_select_chunks, chunks, f"{chapter.title} {chapter.core_concept}")
    _parse = client.beta.chat.completions.parse

    @_retry
    async def _call() -> FullChapterResponse:
        response = await _throttled(
            _parse(
                model="gpt-4o-mini",
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "Pe baza documentului, generează un capitol complet:\n"
                            "1. Împarte capitolul în 2-4 subcapitole\n"
                            "2. Pentru fiecare subcapitol generează teorie HTML (h2,h3,p,ul,li,b,i) "
                            "și mini quiz cu 3 întrebări (4 opțiuni)\n"
                            "3. Generează un quiz recapitulativ cu 10 întrebări (4 opțiuni)\n\n"
                            "REGULI STRICTE PENTRU ÎNTREBĂRI:\n"
                            "- Titluri sub 100 car. Întrebări sub 300 car. Răspunsuri sub 100 car.\n"
                            "- INTERZIS: întrebări care conțin răspunsul în enunț, întrebări de tipul "
                            "'Care este definiția lui X?' când definiția e citată textual, întrebări "
                            "cu ani/date deja menționate în întrebare, sau întrebări la care se poate "
                            "răspunde fără a fi citit materialul.\n"
                            "- OBLIGATORIU: întrebări care testează relații cauză-efect, comparații "
                            "între concepte, aplicarea cunoștințelor în scenarii noi, sau analiza "
                            "consecințelor. Cel puțin 50% din întrebări trebuie să înceapă cu "
                            "'De ce…', 'Ce s-ar întâmpla dacă…', 'Care este diferența dintre…', "
                            "'În ce situație…'.\n"
                            "- RANDOMIZARE RĂSPUNSURI: Pentru fiecare întrebare, alege ALEATORIU "
                            "indexul răspunsului corect (0, 1, 2 sau 3). Urmează strict acest tipar "
                            "pentru cele 10 întrebări recapitulative: pozițiile corecte să fie "
                            "aproximativ: 3,1,0,2,1,3,0,2,0,1. Pentru mini quiz-uri de 3 întrebări "
                            "folosește tiparul: 2,0,1. NU pune răspunsul corect mereu pe prima poziție.\n"
                            "- Distractorii (răspunsurile greșite) trebuie să fie plauzibili, "
                            "nu absurzi — să reflecte greșeli tipice de înțelegere.\n"
                            "TOTUL ÎN ROMÂNĂ."
                        ),
                    },
                    {
                        "role": "user",
                        "content": (
                            f"Context:\n{context}\n\n"
                            f"Ziua: {chapter.day}\n"
                            f"Capitol: {chapter.title}\n"
                            f"Concept: {chapter.core_concept}\n\n"
                            "Generează capitolul complet respectând"
                            " cu strictețe regulile de calitate și randomizare a testelor."
                        ),
                    },
                ],
                response_format=FullChapterResponse,
                max_tokens=16000,
            )
        )
        parsed_data = response.choices[0].message.parsed
        if parsed_data is None:
            raise RuntimeError("OpenAI failed to parse FullChapterResponse")
        return parsed_data

    return await _call()