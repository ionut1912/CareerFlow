import pytest
from httpx import AsyncClient
from fastapi import HTTPException
from unittest.mock import AsyncMock, MagicMock, patch
from pathlib import Path

from app.document_reader.extractor import DocumentContent


@pytest.fixture
def mock_document_content() -> DocumentContent:
    return DocumentContent(
        filename="test.pdf",
        total_pages=1,
        text="Valid document text content",
        pages=["Valid document text content"]
    )


def test_save_and_extract_unsupported_file() -> None:
    from app.document_reader.router import _save_and_extract
    mock_upload = MagicMock()
    mock_upload.filename = "malicious_script.exe"
    
    with patch("app.document_reader.router.SUPPORTED_EXTENSIONS", {".pdf", ".txt"}):
        with pytest.raises(HTTPException) as exc_info:
            _save_and_extract(mock_upload)
            
        assert exc_info.value.status_code == 400
        assert "Unsupported file type" in exc_info.value.detail


@pytest.mark.asyncio
@patch("app.document_reader.router._save_and_extract")
@patch("app.document_reader.service.analyze_and_skeleton", new_callable=AsyncMock)
async def test_upload_and_analyze_happy_path(
    mock_analyze: AsyncMock, 
    mock_save_extract: MagicMock, 
    api_client: AsyncClient, 
    mock_document_content: DocumentContent
) -> None:
    mock_tmp_path = MagicMock(spec=Path)
    mock_save_extract.return_value = (mock_tmp_path, mock_document_content, "hash_123")
    
    mock_combined = MagicMock()
    mock_combined.analysis.model_dump.return_value = {"summary": "A good book"}
    mock_combined.skeleton.model_dump.return_value = {"chapters": ["Ch1", "Ch2"]}
    mock_combined.skeleton.chapters = ["Ch1", "Ch2"]
    mock_analyze.return_value = mock_combined
    
    files = {"file": ("test.pdf", b"dummy content", "application/pdf")}
    response = await api_client.post("/document-courses/upload-and-analyze", files=files)
    
    assert response.status_code == 200
    data = response.json()
    assert data["document_id"] == "hash_123"
    assert data["estimated_days"] == 2
    mock_tmp_path.unlink.assert_called_once_with(missing_ok=True)


@pytest.mark.asyncio
@patch("app.document_reader.router._save_and_extract")
async def test_upload_and_analyze_empty_document_422(
    mock_save_extract: MagicMock, 
    api_client: AsyncClient
) -> None:
    mock_tmp_path = MagicMock(spec=Path)
    empty_content = DocumentContent(filename="empty.pdf", total_pages=1, text="   \n  ", pages=[" "])
    mock_save_extract.return_value = (mock_tmp_path, empty_content, "hash_456")
    
    files = {"file": ("empty.pdf", b"dummy", "application/pdf")}
    response = await api_client.post("/document-courses/upload-and-analyze", files=files)
    
    assert response.status_code == 422
    assert "Could not extract text" in response.json()["detail"]
    mock_tmp_path.unlink.assert_called_once_with(missing_ok=True)


@pytest.mark.asyncio
@patch("app.document_reader.router.get_cached_content")
@patch("app.document_reader.router.chunk_text")
@patch("app.document_reader.service.generate_full_chapter", new_callable=AsyncMock)
async def test_expand_chapter_happy_path(
    mock_generate: AsyncMock, 
    mock_chunk: MagicMock, 
    mock_get_cache: MagicMock,
    api_client: AsyncClient, 
    mock_document_content: DocumentContent
) -> None:
    mock_get_cache.return_value = mock_document_content
    mock_chunk.return_value = ["chunk 1", "chunk 2"]
    
    mock_result = MagicMock()
    mock_result.model_dump.return_value = {"content": "Expanded HTML"}
    mock_generate.return_value = mock_result
    
    payload = {
        "document_id": "valid_hash_123",
        "chapter_title": "Introduction",
        "core_concept": "Basics"
    }
    
    response = await api_client.post("/document-courses/chapters/expand", json=payload)
    
    assert response.status_code == 200
    assert response.json() == {"content": "Expanded HTML"}
    mock_generate.assert_awaited_once()


@pytest.mark.asyncio
@patch("app.document_reader.router.get_cached_content")
async def test_expand_chapter_cache_miss_404(
    mock_get_cache: MagicMock, 
    api_client: AsyncClient
) -> None:
    mock_get_cache.return_value = None
    
    payload = {
        "document_id": "expired_or_invalid_hash",
        "chapter_title": "Intro",
        "core_concept": "Basics"
    }
    
    response = await api_client.post("/document-courses/chapters/expand", json=payload)
    assert response.status_code == 404
    assert "not found in cache" in response.json()["detail"]