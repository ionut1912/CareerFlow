from typing import Any

from fastapi import APIRouter, Depends
from openai import AsyncOpenAI

from app.courses import service
from app.courses.schema import ChapterRequest, ChapterSkeleton, CourseSkeletonRequest
from app.dependencies import get_openai_client

router = APIRouter(prefix="/courses", tags=["Generare AI"])


@router.post("/skeleton")
async def create_skeleton(
    request: CourseSkeletonRequest,
    client: AsyncOpenAI = Depends(get_openai_client),
) -> dict[str, Any]:
    skeleton = await service.generate_skeleton(client, request.topic)
    return {
        "skeleton": skeleton.model_dump(),
        "estimated_days": len(skeleton.chapters),
    }


@router.post("/chapters/expand")
async def expand_chapter(
    request: ChapterRequest,
    client: AsyncOpenAI = Depends(get_openai_client),
) -> dict[str, Any]:
    chapter = ChapterSkeleton(
        title=request.chapter_title,
        core_concept=request.core_concept,
    )
    return await service.build_full_chapter(client, request.topic, chapter)