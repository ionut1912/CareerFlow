import pytest
from pydantic import ValidationError

from app.document_reader.schemas import (
    DocumentAnalysis,
    LearningPlanSkeleton,
    AnalysisAndSkeleton,
    DocumentChapterRequest,
    SubchapterContent,
    FullChapterResponse,
)
from app.courses.schemas import ChapterSkeleton, QuizQuestion, QuizOption
from tests.conftest import make_quiz_question, make_chapter_skeleton


def get_string_of_length(length: int) -> str:
    return "A" * length


def test_document_analysis_happy_path() -> None:
    model = DocumentAnalysis(title="Valid Title", summary="A short summary", key_topics=["Topic 1", "Topic 2"])
    assert model.title == "Valid Title"
    assert len(model.key_topics) == 2


def test_document_analysis_title_boundary() -> None:
    model = DocumentAnalysis(title=get_string_of_length(200), summary="Summary", key_topics=[])
    assert len(model.title) == 200

    with pytest.raises(ValidationError) as exc_info:
        DocumentAnalysis(title=get_string_of_length(201), summary="Summary", key_topics=[])
    assert exc_info.value.errors()[0]["type"] == "string_too_long"
    assert exc_info.value.errors()[0]["loc"] == ("title",)


def test_document_chapter_request_happy_path() -> None:
    model = DocumentChapterRequest(chapter_title="Intro", core_concept="Basics", document_id="hash_123")
    assert model.document_id == "hash_123"


def test_document_chapter_request_max_length_failure() -> None:
    with pytest.raises(ValidationError) as exc_info:
        DocumentChapterRequest(chapter_title="Intro", core_concept="Basics", document_id=get_string_of_length(201))
    assert exc_info.value.errors()[0]["loc"] == ("document_id",)


def test_learning_plan_skeleton() -> None:
    model = LearningPlanSkeleton(topic="Python", chapters=[make_chapter_skeleton()])
    assert model.topic == "Python"
    assert len(model.chapters) == 1


def test_analysis_and_skeleton() -> None:
    analysis = DocumentAnalysis(title="T", summary="S", key_topics=["K"])
    plan = LearningPlanSkeleton(topic="Python", chapters=[make_chapter_skeleton()])
    model = AnalysisAndSkeleton(analysis=analysis, skeleton=plan)
    
    assert model.analysis.title == "T"
    assert model.skeleton.topic == "Python"


def test_subchapter_content() -> None:
    model = SubchapterContent(
        title="Sub 1", content_summary="Sum", theory_html="<p>Test</p>", quiz=[make_quiz_question()]
    )
    assert model.title == "Sub 1"
    assert len(model.quiz) == 1
    
    with pytest.raises(ValidationError):
        SubchapterContent(title="Sub 1", content_summary="Sum", theory_html=get_string_of_length(201), quiz=[])


def test_full_chapter_response() -> None:
    sub = SubchapterContent(
        title="Sub", content_summary="Sum", theory_html="<p></p>", quiz=[make_quiz_question()]
    )
    model = FullChapterResponse(subchapters=[sub], recap_quiz=[make_quiz_question(), make_quiz_question()])
    assert len(model.subchapters) == 1
    assert len(model.recap_quiz) == 2