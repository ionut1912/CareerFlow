"""Tests for app/document_reader/router.py"""

from __future__ import annotations

import io
from unittest.mock import AsyncMock, MagicMock, patch

import pytest
from httpx import AsyncClient

from app.courses.schema import ChapterSkeleton
from app.document_reader.extractor import DocumentContent
from app.document_reader.schema import (
    AnalysisAndSkeleton,
    DocumentAnalysis,
    FullChapterResponse,
    LearningPlanSkeleton,
    SubchapterContent,
)
from tests.conftest import make_quiz_question

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _make_analysis() -> DocumentAnalysis:
    return DocumentAnalysis(
        title="Test Doc",
        summary="A summary.",
        key_topics=["topic1", "topic2"],
    )


def _make_doc_skeleton(num_chapters: int = 2) -> LearningPlanSkeleton:
    chapters = [
        ChapterSkeleton(title=f"Ziua {i + 1}", core_concept=f"Concept {i + 1}", day=i + 1) for i in range(num_chapters)
    ]
    return LearningPlanSkeleton(topic="Test", chapters=chapters)


def _make_analysis_and_skeleton(num_chapters: int = 2) -> AnalysisAndSkeleton:
    return AnalysisAndSkeleton(
        analysis=_make_analysis(),
        skeleton=_make_doc_skeleton(num_chapters),
    )


def _make_full_chapter_response() -> FullChapterResponse:
    q = make_quiz_question()
    sub = SubchapterContent(
        title="Sub 1",
        content_summary="Summary",
        theory_html="<p>Theory</p>",
        quiz=[q, q, q],
    )
    return FullChapterResponse(subchapters=[sub], recap_quiz=[q] * 10)


def _parsed_response(obj: object) -> MagicMock:
    msg = MagicMock()
    msg.parsed = obj
    choice = MagicMock()
    choice.message = msg
    comp = MagicMock()
    comp.choices = [choice]
    return comp


# ---------------------------------------------------------------------------
# Unit tests
# ---------------------------------------------------------------------------


class TestUnitDocumentReaderRouter:
    """Unit tests for /document-courses endpoints."""

    @pytest.mark.anyio
    async def test_upload_unsupported_extension(self, api_client: AsyncClient) -> None:
        """Uploading a .txt file returns 400 unsupported type."""
        response = await api_client.post(
            "/document-courses/upload-and-analyze",
            files={"file": ("notes.txt", io.BytesIO(b"hello"), "text/plain")},
        )
        assert response.status_code == 400
        assert "Unsupported" in response.json()["detail"]

    @pytest.mark.anyio
    async def test_upload_empty_text_returns_422(self, api_client: AsyncClient) -> None:
        """If extracted text is blank the endpoint returns 422."""
        empty_content = DocumentContent(filename="empty.pdf", total_pages=1, text="   ", pages=["   "])
        with patch(
            "app.document_reader.router._save_and_extract",
            return_value=(MagicMock(), empty_content, "abc123"),
        ):
            response = await api_client.post(
                "/document-courses/upload-and-analyze",
                files={"file": ("empty.pdf", io.BytesIO(b"%PDF fake"), "application/pdf")},
            )
        assert response.status_code == 422

    @pytest.mark.anyio
    async def test_upload_and_analyze_happy_path(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """upload-and-analyze returns document_id, analysis, skeleton, estimated_days."""
        doc_content = DocumentContent(
            filename="doc.pdf", total_pages=3, text="Some content here.", pages=["Some content here."]
        )
        aas = _make_analysis_and_skeleton(num_chapters=3)
        mock_openai_client.beta.chat.completions.parse.return_value = _parsed_response(aas)

        with patch(
            "app.document_reader.router._save_and_extract",
            return_value=(MagicMock(unlink=MagicMock()), doc_content, "docid123"),
        ):
            response = await api_client.post(
                "/document-courses/upload-and-analyze",
                files={"file": ("doc.pdf", io.BytesIO(b"%PDF"), "application/pdf")},
            )

        assert response.status_code == 200
        body = response.json()
        assert body["document_id"] == "docid123"
        assert body["estimated_days"] == 3
        assert "analysis" in body
        assert "skeleton" in body

    @pytest.mark.anyio
    async def test_expand_chapter_document_not_found(self, api_client: AsyncClient) -> None:
        """expand_chapter returns 404 when document_id is not in cache."""
        with patch("app.document_reader.router.get_cached_content", return_value=None):
            response = await api_client.post(
                "/document-courses/chapters/expand",
                json={
                    "chapter_title": "Ziua 1",
                    "core_concept": "Concept",
                    "document_id": "nonexistent",
                },
            )
        assert response.status_code == 404
        assert "not found" in response.json()["detail"].lower()

    @pytest.mark.anyio
    async def test_expand_chapter_happy_path(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """expand_chapter returns FullChapterResponse when document is cached."""
        cached = DocumentContent(filename="doc.pdf", total_pages=2, text="Paragraph one.\n\nParagraph two.", pages=[])
        full = _make_full_chapter_response()
        mock_openai_client.beta.chat.completions.parse.return_value = _parsed_response(full)

        with patch("app.document_reader.router.get_cached_content", return_value=cached):
            response = await api_client.post(
                "/document-courses/chapters/expand",
                json={
                    "chapter_title": "Ziua 1",
                    "core_concept": "Concept",
                    "document_id": "valid_id",
                },
            )

        assert response.status_code == 200
        body = response.json()
        assert "subchapters" in body
        assert "recap_quiz" in body
        assert len(body["recap_quiz"]) == 10

    @pytest.mark.anyio
    async def test_expand_chapter_missing_fields(self, api_client: AsyncClient) -> None:
        """expand_chapter without required body fields returns 422."""
        response = await api_client.post(
            "/document-courses/chapters/expand",
            json={"chapter_title": "Ziua 1"},
        )
        assert response.status_code == 422

    @pytest.mark.anyio
    async def test_upload_no_filename(self, api_client: AsyncClient) -> None:
        """File upload without a filename is rejected.

        FastAPI's multipart parser rejects an empty filename at the framework
        layer (422) before our extension-check code (400) is reached.
        Both codes indicate the request is invalid, so we accept either.
        """
        response = await api_client.post(
            "/document-courses/upload-and-analyze",
            files={"file": ("", io.BytesIO(b"data"), "application/octet-stream")},
        )
        assert response.status_code in (400, 422)

    @pytest.mark.anyio
    async def test_upload_docx_extension_accepted(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """A .docx file passes extension validation and reaches the service layer."""
        doc_content = DocumentContent(filename="doc.docx", total_pages=1, text="Hello world.", pages=["Hello world."])
        aas = _make_analysis_and_skeleton(1)
        mock_openai_client.beta.chat.completions.parse.return_value = _parsed_response(aas)

        with patch(
            "app.document_reader.router._save_and_extract",
            return_value=(MagicMock(unlink=MagicMock()), doc_content, "id1"),
        ):
            response = await api_client.post(
                "/document-courses/upload-and-analyze",
                files={
                    "file": (
                        "doc.docx",
                        io.BytesIO(b"PK fake docx"),
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    )
                },
            )
        assert response.status_code == 200


# ---------------------------------------------------------------------------
# Integration tests
# ---------------------------------------------------------------------------


class TestIntegrationDocumentReaderRouter:
    """Integration tests: upload → expand workflow."""

    @pytest.mark.anyio
    async def test_full_upload_then_expand_workflow(
        self, api_client: AsyncClient, mock_openai_client: AsyncMock
    ) -> None:
        """Upload a doc, get doc_id, then expand a chapter using that doc_id."""
        doc_content = DocumentContent(
            filename="doc.pdf",
            total_pages=2,
            text="Line one.\n\nLine two.",
            pages=["Line one.", "Line two."],
        )
        aas = _make_analysis_and_skeleton(num_chapters=1)
        full = _make_full_chapter_response()

        mock_openai_client.beta.chat.completions.parse.side_effect = [
            _parsed_response(aas),
            _parsed_response(full),
        ]

        with (
            patch(
                "app.document_reader.router._save_and_extract",
                return_value=(MagicMock(unlink=MagicMock()), doc_content, "real_id"),
            ),
            patch(
                "app.document_reader.router.get_cached_content",
                return_value=doc_content,
            ),
        ):
            upload_resp = await api_client.post(
                "/document-courses/upload-and-analyze",
                files={"file": ("doc.pdf", io.BytesIO(b"%PDF"), "application/pdf")},
            )
            assert upload_resp.status_code == 200

            expand_resp = await api_client.post(
                "/document-courses/chapters/expand",
                json={
                    "chapter_title": "Ziua 1",
                    "core_concept": "Concept",
                    "document_id": "real_id",
                },
            )
            assert expand_resp.status_code == 200
            assert "subchapters" in expand_resp.json()

    @pytest.mark.anyio
    @pytest.mark.parametrize("ext", [".pdf", ".docx", ".doc"])
    async def test_supported_extensions(
        self,
        api_client: AsyncClient,
        mock_openai_client: AsyncMock,
        ext: str,
    ) -> None:
        """All SUPPORTED_EXTENSIONS pass validation and reach the service layer."""
        doc_content = DocumentContent(filename=f"file{ext}", total_pages=1, text="Content.", pages=["Content."])
        aas = _make_analysis_and_skeleton(1)
        mock_openai_client.beta.chat.completions.parse.return_value = _parsed_response(aas)

        with patch(
            "app.document_reader.router._save_and_extract",
            return_value=(MagicMock(unlink=MagicMock()), doc_content, "id"),
        ):
            response = await api_client.post(
                "/document-courses/upload-and-analyze",
                files={"file": (f"file{ext}", io.BytesIO(b"data"), "application/octet-stream")},
            )
        assert response.status_code == 200