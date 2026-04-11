import asyncio
import shutil
import tempfile
from pathlib import Path
from typing import Any

from fastapi import APIRouter, Depends, File, HTTPException, UploadFile
from openai import AsyncOpenAI

from app.courses.schema import ChapterSkeleton
from app.dependencies import get_openai_client
from app.document_reader import service
from app.document_reader.extractor import (
    SUPPORTED_EXTENSIONS,
    DocumentContent,
    chunk_text,
    extract_text_from_document,
    file_hash,
    get_cached_content,
)
from app.document_reader.schema import DocumentChapterRequest

router = APIRouter(prefix="/document-courses", tags=["Document Course Generation"])


def _save_and_extract(upload: UploadFile) -> tuple[Path, DocumentContent, str]:
    filename = upload.filename or ""
    ext = Path(filename).suffix.lower()
    if ext not in SUPPORTED_EXTENSIONS:
        raise HTTPException(400, f"Unsupported file type '{ext}'. Accepted: {', '.join(SUPPORTED_EXTENSIONS)}")
    tmp = Path(tempfile.mktemp(suffix=ext))
    with tmp.open("wb") as f:
        shutil.copyfileobj(upload.file, f)
    content = extract_text_from_document(tmp)
    doc_id = file_hash(tmp)
    return tmp, content, doc_id


@router.post("/upload-and-analyze")
async def upload_and_analyze(
    file: UploadFile = File(...),
    client: AsyncOpenAI = Depends(get_openai_client),
) -> dict[str, Any]:
    tmp, content, doc_id = await asyncio.to_thread(_save_and_extract, file)
    try:
        if not content.text.strip():
            raise HTTPException(422, "Could not extract text from document")
        combined = await service.analyze_and_skeleton(client, content)
        return {
            "document_id": doc_id,
            "analysis": combined.analysis.model_dump(),
            "skeleton": combined.skeleton.model_dump(),
            "estimated_days": len(combined.skeleton.chapters),
        }
    finally:
        tmp.unlink(missing_ok=True)


@router.post("/chapters/expand")
async def expand_chapter(
    request: DocumentChapterRequest,
    client: AsyncOpenAI = Depends(get_openai_client),
) -> dict[str, Any]:
    cached = get_cached_content(request.document_id)
    if cached is None:
        raise HTTPException(404, "Document not found in cache. Re-upload the file.")

    chunks = await asyncio.to_thread(chunk_text, cached.text, 3000)

    chapter = ChapterSkeleton(
        title=request.chapter_title,
        core_concept=request.core_concept,
    )
    result = await service.generate_full_chapter(client, chunks, chapter)
    return result.model_dump()