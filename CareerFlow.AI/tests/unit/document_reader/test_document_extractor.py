from __future__ import annotations

import threading
from collections.abc import Generator
from pathlib import Path
from unittest.mock import MagicMock, patch

import pytest

from app.document_reader.extractor import (
    DocumentContent,
    _cache_lock,
    _content_cache,
    chunk_text,
    convert_doc_to_docx,
    extract_text_from_document,
    extract_text_from_docx,
    extract_text_from_pdf,
    file_hash,
    get_cached_content,
    set_cached_content,
)


@pytest.fixture(autouse=True)
def clear_cache() -> Generator[None, None, None]:
    with _cache_lock:
        _content_cache.clear()
    yield
    with _cache_lock:
        _content_cache.clear()


@pytest.fixture
def tmp_pdf(tmp_path: Path) -> Path:
    p = tmp_path / "test.pdf"
    p.write_bytes(b"%PDF-1.4 fake content")
    return p


@pytest.fixture
def tmp_docx(tmp_path: Path) -> Path:
    p = tmp_path / "test.docx"
    p.write_bytes(b"PK fake docx content")
    return p


@pytest.fixture
def sample_content() -> DocumentContent:
    return DocumentContent(
        filename="sample.pdf",
        total_pages=2,
        text="Page one.\n\nPage two.",
        pages=["Page one.", "Page two."],
    )


class TestUnitFileHash:
    def test_returns_sha256_hex_string(self, tmp_pdf: Path) -> None:
        h = file_hash(tmp_pdf)
        assert isinstance(h, str)
        assert len(h) == 64

    def test_same_content_same_hash(self, tmp_path: Path) -> None:
        a = tmp_path / "a.pdf"
        b = tmp_path / "b.pdf"
        a.write_bytes(b"identical")
        b.write_bytes(b"identical")
        assert file_hash(a) == file_hash(b)

    def test_different_content_different_hash(self, tmp_path: Path) -> None:
        a = tmp_path / "a.pdf"
        b = tmp_path / "b.pdf"
        a.write_bytes(b"content A")
        b.write_bytes(b"content B")
        assert file_hash(a) != file_hash(b)

    def test_empty_file(self, tmp_path: Path) -> None:
        f = tmp_path / "empty.pdf"
        f.write_bytes(b"")
        assert len(file_hash(f)) == 64


class TestUnitCacheHelpers:
    def test_set_then_get_returns_content(self, sample_content: DocumentContent) -> None:
        set_cached_content("key1", sample_content)
        assert get_cached_content("key1") is sample_content

    def test_get_missing_key_returns_none(self) -> None:
        assert get_cached_content("no_such_key") is None

    def test_stale_entry_is_evicted(self, sample_content: DocumentContent) -> None:
        with _cache_lock:
            _content_cache["stale"] = (sample_content, 0.0)

        with patch("app.document_reader.extractor.time.monotonic", return_value=9999999999.0):
            assert get_cached_content("stale") is None

    def test_overwrite_updates_value(self, sample_content: DocumentContent) -> None:
        other = DocumentContent(filename="other.pdf", total_pages=1, text="other", pages=["other"])
        set_cached_content("k", sample_content)
        set_cached_content("k", other)
        assert get_cached_content("k") is other

    def test_thread_safety(self, sample_content: DocumentContent) -> None:
        errors: list[Exception] = []

        def worker(i: int) -> None:
            try:
                set_cached_content(f"k{i}", sample_content)
                get_cached_content(f"k{i}")
            except Exception as exc:
                errors.append(exc)

        threads = [threading.Thread(target=worker, args=(i,)) for i in range(20)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()
        assert not errors


class TestUnitChunkText:
    def test_short_text_returned_as_single_chunk(self) -> None:
        assert chunk_text("short text", max_chars=1000) == ["short text"]

    def test_long_text_split_into_multiple_chunks(self) -> None:
        text = "\n\n".join([f"paragraph {i}" for i in range(100)])
        assert len(chunk_text(text, max_chars=200)) > 1

    def test_each_chunk_within_reasonable_size(self) -> None:
        text = "\n\n".join(["x" * 500 for _ in range(10)])
        for chunk in chunk_text(text, max_chars=1200):
            assert len(chunk) <= 1702

    def test_empty_string_returns_list_with_empty_string(self) -> None:
        assert chunk_text("", max_chars=100) == [""]

    def test_no_paragraph_break_single_chunk(self) -> None:
        text = "a" * 100
        assert chunk_text(text, max_chars=200) == [text]

    @pytest.mark.parametrize("max_chars", [100, 500, 12000])
    def test_various_max_chars(self, max_chars: int) -> None:
        text = "\n\n".join(["word " * 50 for _ in range(20)])
        result = chunk_text(text, max_chars=max_chars)
        assert isinstance(result, list)
        assert all(isinstance(c, str) for c in result)


class TestUnitExtractTextFromPdf:
    def _mock_pdf(self, pages_text: list[str | None]) -> MagicMock:
        mock_pages = []
        for t in pages_text:
            p = MagicMock()
            p.extract_text.return_value = t
            mock_pages.append(p)
        mock_pdf = MagicMock()
        mock_pdf.pages = mock_pages
        mock_pdf.__enter__ = lambda s: s
        mock_pdf.__exit__ = MagicMock(return_value=False)
        return mock_pdf

    def test_happy_path(self, tmp_pdf: Path) -> None:
        with patch(
            "app.document_reader.extractor.pdfplumber.open", return_value=self._mock_pdf(["Hello PDF", "Page two"])
        ):
            result = extract_text_from_pdf(tmp_pdf)
        assert result.filename == tmp_pdf.name
        assert result.total_pages == 2
        assert "Hello PDF" in result.text

    def test_page_with_none_text_becomes_empty_string(self, tmp_pdf: Path) -> None:
        with patch("app.document_reader.extractor.pdfplumber.open", return_value=self._mock_pdf([None])):
            result = extract_text_from_pdf(tmp_pdf)
        assert result.pages == [""]

    def test_empty_pdf_zero_pages(self, tmp_pdf: Path) -> None:
        with patch("app.document_reader.extractor.pdfplumber.open", return_value=self._mock_pdf([])):
            result = extract_text_from_pdf(tmp_pdf)
        assert result.total_pages == 0
        assert result.text == ""


class TestUnitExtractTextFromDocx:
    def _make_para(self, text: str) -> MagicMock:
        p = MagicMock()
        p.text = text
        return p

    def _make_cell(self, text: str) -> MagicMock:
        c = MagicMock()
        c.text = text
        return c

    def test_happy_path_paragraphs(self, tmp_docx: Path) -> None:
        mock_doc = MagicMock()
        mock_doc.paragraphs = [self._make_para("Para one"), self._make_para("Para two")]
        mock_doc.tables = []
        with patch("app.document_reader.extractor.DocxDocument", return_value=mock_doc):
            result = extract_text_from_docx(tmp_docx)
        assert "Para one" in result.text
        assert "Para two" in result.text

    def test_empty_paragraphs_filtered(self, tmp_docx: Path) -> None:
        mock_doc = MagicMock()
        mock_doc.paragraphs = [self._make_para(""), self._make_para("   "), self._make_para("Real")]
        mock_doc.tables = []
        with patch("app.document_reader.extractor.DocxDocument", return_value=mock_doc):
            result = extract_text_from_docx(tmp_docx)
        assert "Real" in result.text

    def test_table_cells_included(self, tmp_docx: Path) -> None:
        mock_doc = MagicMock()
        mock_doc.paragraphs = [self._make_para("Para")]
        row = MagicMock()
        row.cells = [self._make_cell("Cell A"), self._make_cell("Cell B")]
        table = MagicMock()
        table.rows = [row]
        mock_doc.tables = [table]
        with patch("app.document_reader.extractor.DocxDocument", return_value=mock_doc):
            result = extract_text_from_docx(tmp_docx)
        assert "Cell A" in result.text

    def test_no_paragraphs_no_tables_returns_placeholder(self, tmp_docx: Path) -> None:
        mock_doc = MagicMock()
        mock_doc.paragraphs = []
        mock_doc.tables = []
        with patch("app.document_reader.extractor.DocxDocument", return_value=mock_doc):
            result = extract_text_from_docx(tmp_docx)
        assert result.pages == [""]
        assert result.total_pages == 1


class TestUnitExtractTextFromDocument:
    def test_file_not_found_raises(self) -> None:
        with pytest.raises(FileNotFoundError):
            extract_text_from_document("/nonexistent/path/file.pdf")

    def test_unsupported_extension_raises_value_error(self, tmp_path: Path) -> None:
        f = tmp_path / "file.xyz"
        f.write_bytes(b"data")
        with pytest.raises(ValueError, match="Unsupported"):
            extract_text_from_document(f)

    def test_pdf_dispatches_to_pdf_extractor(self, tmp_pdf: Path) -> None:
        expected = DocumentContent(filename="test.pdf", total_pages=1, text="text", pages=["text"])
        with patch("app.document_reader.extractor.extract_text_from_pdf", return_value=expected) as mock_fn:
            result = extract_text_from_document(tmp_pdf)
        mock_fn.assert_called_once_with(tmp_pdf)
        assert result is expected

    def test_docx_dispatches_to_docx_extractor(self, tmp_docx: Path) -> None:
        expected = DocumentContent(filename="test.docx", total_pages=1, text="text", pages=["text"])
        with patch("app.document_reader.extractor.extract_text_from_docx", return_value=expected) as mock_fn:
            result = extract_text_from_document(tmp_docx)
        mock_fn.assert_called_once_with(tmp_docx)
        assert result is expected

    def test_result_is_cached_after_first_call(self, tmp_pdf: Path) -> None:
        expected = DocumentContent(filename="test.pdf", total_pages=1, text="text", pages=["text"])
        with patch("app.document_reader.extractor.extract_text_from_pdf", return_value=expected) as mock_fn:
            extract_text_from_document(tmp_pdf)
            extract_text_from_document(tmp_pdf)
        mock_fn.assert_called_once()

    def test_doc_extension_triggers_conversion(self, tmp_path: Path) -> None:
        doc_file = tmp_path / "file.doc"
        doc_file.write_bytes(b"legacy doc")
        converted = tmp_path / "file.docx"
        converted.write_bytes(b"PK fake")
        expected = DocumentContent(filename="file.docx", total_pages=1, text="converted", pages=["converted"])
        with (
            patch("app.document_reader.extractor.convert_doc_to_docx", return_value=converted),
            patch("app.document_reader.extractor.extract_text_from_docx", return_value=expected),
            patch("shutil.rmtree"),
        ):
            result = extract_text_from_document(doc_file)
        assert result is expected


class TestUnitConvertDocToDocx:
    def test_raises_runtime_error_when_output_missing(self, tmp_path: Path) -> None:
        doc = tmp_path / "file.doc"
        doc.write_bytes(b"data")
        out_dir = tmp_path / "out"
        out_dir.mkdir()
        with (
            patch("subprocess.run"),
            patch("tempfile.mkdtemp", return_value=str(out_dir)),
            pytest.raises(RuntimeError, match="LibreOffice"),
        ):
            convert_doc_to_docx(doc)

    def test_returns_converted_path_on_success(self, tmp_path: Path) -> None:
        doc = tmp_path / "file.doc"
        doc.write_bytes(b"data")
        out_dir = tmp_path / "out"
        out_dir.mkdir()
        converted = out_dir / "file.docx"
        converted.write_bytes(b"PK converted")
        with patch("subprocess.run"), patch("tempfile.mkdtemp", return_value=str(out_dir)):
            result = convert_doc_to_docx(doc)
        assert result == converted


class TestIntegrationExtractor:
    def test_chunk_then_cache_roundtrip(self, sample_content: DocumentContent) -> None:
        chunks = chunk_text(sample_content.text)
        set_cached_content("integration_key", sample_content)
        assert get_cached_content("integration_key") is sample_content
        assert isinstance(chunks, list)

    def test_file_hash_consistent_across_reads(self, tmp_path: Path) -> None:
        f = tmp_path / "stable.pdf"
        f.write_bytes(b"stable content")
        assert file_hash(f) == file_hash(f)

    @pytest.mark.parametrize(
        ("text", "max_chars", "expected_min_chunks"),
        [
            ("a\n\nb\n\nc", 3, 2),
            ("short", 100, 1),
            ("\n\n".join(["x" * 100] * 20), 250, 5),
        ],
    )
    def test_chunk_text_parametrized(self, text: str, max_chars: int, expected_min_chunks: int) -> None:
        assert len(chunk_text(text, max_chars=max_chars)) >= expected_min_chunks

    def test_cache_eviction_on_stale_entry(self, sample_content: DocumentContent) -> None:
        with _cache_lock:
            _content_cache["ttl_test"] = (sample_content, 0.0)

        with patch("app.document_reader.extractor.time.monotonic", return_value=9999999999.0):
            assert get_cached_content("ttl_test") is None