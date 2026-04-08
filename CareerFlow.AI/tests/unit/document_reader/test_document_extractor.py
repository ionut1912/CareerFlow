import pytest
from pathlib import Path
from unittest.mock import MagicMock
from pytest_mock import MockerFixture

from app.document_reader.extractor import (
    DocumentContent,
    file_hash,
    get_cached_content,
    set_cached_content,
    extract_text_from_pdf,
    extract_text_from_docx,
    convert_doc_to_docx,
    extract_text_from_document,
    chunk_text,
    _content_cache,
)
import app.document_reader.extractor as extractor_module


@pytest.fixture(autouse=True)
def clear_cache() -> None:
    _content_cache.clear()
    yield
    _content_cache.clear()


@pytest.fixture
def mock_document_content() -> DocumentContent:
    return DocumentContent(filename="test.pdf", total_pages=1, text="Test", pages=["Test"])


def test_file_hash(tmp_path: Path) -> None:
    test_file = tmp_path / "test.txt"
    test_file.write_text("hello world")
    expected_hash = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9" 
    assert file_hash(test_file) == expected_hash


def test_cache_set_and_get(mock_document_content: DocumentContent) -> None:
    set_cached_content("hash123", mock_document_content)
    cached = get_cached_content("hash123")
    assert cached == mock_document_content


def test_cache_miss() -> None:
    assert get_cached_content("unknown_hash") is None


def test_cache_eviction(mocker: MockerFixture, mock_document_content: DocumentContent) -> None:
    mock_time = mocker.patch.object(extractor_module.time, "monotonic")
    
    mock_time.return_value = 0.0
    set_cached_content("stale_hash", mock_document_content)
    
    mock_time.return_value = 601.0
    assert get_cached_content("stale_hash") is None


def test_extract_text_from_pdf(mocker: MockerFixture) -> None:
    mock_page1 = MagicMock()
    mock_page1.extract_text.return_value = "Page 1 content"
    mock_page2 = MagicMock()
    mock_page2.extract_text.return_value = None 
    
    mock_pdf = MagicMock()
    mock_pdf.pages = [mock_page1, mock_page2]
    
    mocker.patch("app.document_reader.extractor.pdfplumber.open").return_value.__enter__.return_value = mock_pdf
    
    result = extract_text_from_pdf(Path("dummy.pdf"))
    
    assert result.total_pages == 2
    assert result.text == "Page 1 content\n\n"
    assert result.pages == ["Page 1 content", ""]


def test_extract_text_from_docx(mocker: MockerFixture) -> None:
    mock_para = MagicMock()
    mock_para.text = "Paragraph text"
    
    mock_empty_para = MagicMock()
    mock_empty_para.text = "   " 
    
    mock_cell1 = MagicMock()
    mock_cell1.text = " Cell 1 "
    mock_cell2 = MagicMock()
    mock_cell2.text = "Cell 2"
    
    mock_row = MagicMock()
    mock_row.cells = [mock_cell1, mock_cell2]
    mock_table = MagicMock()
    mock_table.rows = [mock_row]
    
    mock_doc_instance = MagicMock()
    mock_doc_instance.paragraphs = [mock_para, mock_empty_para]
    mock_doc_instance.tables = [mock_table]
    
    mocker.patch("app.document_reader.extractor.DocxDocument", return_value=mock_doc_instance)
    
    result = extract_text_from_docx(Path("dummy.docx"))
    
    assert "Paragraph text" in result.text
    assert "Cell 1 | Cell 2" in result.text


def test_convert_doc_to_docx_success(mocker: MockerFixture) -> None:
    mock_subprocess = mocker.patch("app.document_reader.extractor.subprocess.run")
    mocker.patch("app.document_reader.extractor.tempfile.mkdtemp", return_value="/tmp/mockdir")
    mocker.patch("app.document_reader.extractor.Path.exists", return_value=True)
    
    result = convert_doc_to_docx(Path("old_format.doc"))
    
    assert result.name == "old_format.docx"
    mock_subprocess.assert_called_once()


def test_convert_doc_to_docx_failure(mocker: MockerFixture) -> None:
    mocker.patch("app.document_reader.extractor.subprocess.run")
    mocker.patch("app.document_reader.extractor.Path.exists", return_value=False)
    
    with pytest.raises(RuntimeError, match="LibreOffice failed to convert"):
        convert_doc_to_docx(Path("bad.doc"))


def test_extract_text_file_not_found() -> None:
    with pytest.raises(FileNotFoundError):
        extract_text_from_document("does_not_exist.pdf")


def test_extract_text_unsupported_type(mocker: MockerFixture, tmp_path: Path) -> None:
    test_file = tmp_path / "test.txt"
    test_file.touch()
    with pytest.raises(ValueError, match="Unsupported file type"):
        extract_text_from_document(test_file)


def test_extract_text_cache_hit(mocker: MockerFixture, tmp_path: Path, mock_document_content: DocumentContent) -> None:
    test_file = tmp_path / "test.pdf"
    test_file.touch()
    
    mocker.patch("app.document_reader.extractor.file_hash", return_value="hash_hit")
    mocker.patch("app.document_reader.extractor.get_cached_content", return_value=mock_document_content)
    mock_extract_pdf = mocker.patch("app.document_reader.extractor.extract_text_from_pdf")
    
    result = extract_text_from_document(test_file)
    
    assert result == mock_document_content
    mock_extract_pdf.assert_not_called()


def test_extract_text_doc_flow(mocker: MockerFixture, tmp_path: Path, mock_document_content: DocumentContent) -> None:
    test_file = tmp_path / "legacy.doc"
    test_file.touch()
    
    mock_converted_path = MagicMock(spec=Path)
    mock_converted_path.parent = "/tmp/mockdir"
    
    mocker.patch("app.document_reader.extractor.file_hash", return_value="new_hash")
    mocker.patch("app.document_reader.extractor.get_cached_content", return_value=None)
    
    mock_convert = mocker.patch("app.document_reader.extractor.convert_doc_to_docx", return_value=mock_converted_path)
    mock_extract_docx = mocker.patch("app.document_reader.extractor.extract_text_from_docx", return_value=mock_document_content)
    mock_rmtree = mocker.patch("app.document_reader.extractor.shutil.rmtree")
    mock_set_cache = mocker.patch("app.document_reader.extractor.set_cached_content")
    
    result = extract_text_from_document(test_file)
    
    assert result == mock_document_content
    mock_convert.assert_called_once_with(test_file)
    mock_extract_docx.assert_called_once_with(mock_converted_path)
    mock_rmtree.assert_called_once_with(mock_converted_path.parent, ignore_errors=True)
    mock_set_cache.assert_called_once_with("new_hash", mock_document_content)


def test_chunk_text_under_max_chars() -> None:
    text = "Short text."
    chunks = chunk_text(text, max_chars=100)
    assert len(chunks) == 1
    assert chunks[0] == text


def test_chunk_text_over_max_chars() -> None:
    para1 = "A" * 50
    para2 = "B" * 60
    text = f"{para1}\n\n{para2}"
    chunks = chunk_text(text, max_chars=100)
    
    assert len(chunks) == 2
    assert chunks[0] == para1
    assert chunks[1] == para2