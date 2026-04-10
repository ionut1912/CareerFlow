"""Tests for app/document_reader/schemas.py"""
from __future__ import annotations

import pytest
from pydantic import ValidationError

from app.courses.schemas import ChapterSkeleton, QuizOption, QuizQuestion
from app.document_reader.schemas import (
    AnalysisAndSkeleton,
    DocumentAnalysis,
    DocumentChapterRequest,
    FullChapterResponse,
    LearningPlanSkeleton,
    SubchapterContent,
)

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _quiz_option(label: str = "Option", is_correct: bool = False) -> QuizOption:
    return QuizOption(label=label, is_correct=is_correct)


def _quiz_question(question: str = "Q?") -> QuizQuestion:
    return QuizQuestion(
        question=question,
        options=[
            _quiz_option("A", True),
            _quiz_option("B"),
            _quiz_option("C"),
            _quiz_option("D"),
        ],
    )


def _make_analysis(**kwargs: object) -> DocumentAnalysis:
    defaults: dict[str, object] = {
        "title": "Title",
        "summary": "Summary",
        "key_topics": ["a", "b"],
    }
    return DocumentAnalysis(**{**defaults, **kwargs})  # type: ignore[arg-type]


def _make_skeleton(n: int = 2) -> LearningPlanSkeleton:
    return LearningPlanSkeleton(
        topic="Topic",
        chapters=[
            ChapterSkeleton(title=f"Ziua {i+1}", core_concept=f"C{i+1}", day=i+1)
            for i in range(n)
        ],
    )


def _make_subchapter_content(**kwargs: object) -> SubchapterContent:
    defaults: dict[str, object] = {
        "title": "Sub",
        "content_summary": "Summary",
        "theory_html": "<p>Theory</p>",
        "quiz": [],
    }
    return SubchapterContent(**{**defaults, **kwargs})  # type: ignore[arg-type]


# ---------------------------------------------------------------------------
# DocumentAnalysis
# ---------------------------------------------------------------------------

def test_document_analysis_valid() -> None:
    """DocumentAnalysis stores all fields correctly."""
    model = _make_analysis()
    assert model.title == "Title"
    assert model.summary == "Summary"
    assert model.key_topics == ["a", "b"]


def test_document_analysis_accepts_long_title() -> None:
    """DocumentAnalysis has no max_length on title; long strings are valid."""
    long_title = "a" * 10_000
    assert len(_make_analysis(title=long_title).title) == 10_000


def test_document_analysis_accepts_long_summary() -> None:
    """DocumentAnalysis has no max_length on summary; long strings are valid."""
    assert len(_make_analysis(summary="s" * 10_000).summary) == 10_000


def test_document_analysis_empty_key_topics_valid() -> None:
    """DocumentAnalysis accepts an empty key_topics list."""
    assert _make_analysis(key_topics=[]).key_topics == []


def test_document_analysis_multiple_key_topics() -> None:
    """DocumentAnalysis stores all provided key_topics."""
    assert _make_analysis(key_topics=["x", "y", "z"]).key_topics == ["x", "y", "z"]


def test_document_analysis_missing_title_raises() -> None:
    """DocumentAnalysis raises ValidationError when title is absent."""
    with pytest.raises(ValidationError):
        DocumentAnalysis(summary="S", key_topics=[])  # type: ignore[call-arg]


def test_document_analysis_missing_summary_raises() -> None:
    """DocumentAnalysis raises ValidationError when summary is absent."""
    with pytest.raises(ValidationError):
        DocumentAnalysis(title="T", key_topics=[])  # type: ignore[call-arg]


def test_document_analysis_missing_key_topics_raises() -> None:
    """DocumentAnalysis raises ValidationError when key_topics is absent."""
    with pytest.raises(ValidationError):
        DocumentAnalysis(title="T", summary="S")  # type: ignore[call-arg]


# ---------------------------------------------------------------------------
# LearningPlanSkeleton (document_reader version)
# ---------------------------------------------------------------------------

def test_learning_plan_skeleton_valid() -> None:
    """LearningPlanSkeleton stores topic and chapters correctly."""
    model = _make_skeleton(3)
    assert model.topic == "Topic"
    assert len(model.chapters) == 3


def test_learning_plan_skeleton_empty_chapters() -> None:
    """LearningPlanSkeleton accepts an empty chapters list."""
    assert LearningPlanSkeleton(topic="T", chapters=[]).chapters == []


def test_learning_plan_skeleton_missing_topic_raises() -> None:
    """LearningPlanSkeleton raises ValidationError when topic is absent."""
    with pytest.raises(ValidationError):
        LearningPlanSkeleton(chapters=[])  # type: ignore[call-arg]


def test_learning_plan_skeleton_missing_chapters_raises() -> None:
    """LearningPlanSkeleton raises ValidationError when chapters is absent."""
    with pytest.raises(ValidationError):
        LearningPlanSkeleton(topic="T")  # type: ignore[call-arg]


@pytest.mark.parametrize("n", [1, 5, 10])
def test_learning_plan_skeleton_various_chapter_counts(n: int) -> None:
    """LearningPlanSkeleton correctly stores any number of chapters."""
    assert len(_make_skeleton(n).chapters) == n


# ---------------------------------------------------------------------------
# AnalysisAndSkeleton
# ---------------------------------------------------------------------------

def test_analysis_and_skeleton_valid() -> None:
    """AnalysisAndSkeleton combines analysis and skeleton correctly."""
    model = AnalysisAndSkeleton(analysis=_make_analysis(), skeleton=_make_skeleton())
    assert model.analysis.title == "Title"
    assert model.skeleton.topic == "Topic"


def test_analysis_and_skeleton_missing_analysis_raises() -> None:
    """AnalysisAndSkeleton raises ValidationError when analysis is absent."""
    with pytest.raises(ValidationError):
        AnalysisAndSkeleton(skeleton=_make_skeleton())  # type: ignore[call-arg]


def test_analysis_and_skeleton_missing_skeleton_raises() -> None:
    """AnalysisAndSkeleton raises ValidationError when skeleton is absent."""
    with pytest.raises(ValidationError):
        AnalysisAndSkeleton(analysis=_make_analysis())  # type: ignore[call-arg]


def test_analysis_and_skeleton_chapter_count_preserved() -> None:
    """AnalysisAndSkeleton preserves the chapter count from the skeleton."""
    model = AnalysisAndSkeleton(analysis=_make_analysis(), skeleton=_make_skeleton(5))
    assert len(model.skeleton.chapters) == 5


# ---------------------------------------------------------------------------
# DocumentChapterRequest
# ---------------------------------------------------------------------------

def test_document_chapter_request_valid() -> None:
    """DocumentChapterRequest stores all three fields correctly."""
    model = DocumentChapterRequest(
        chapter_title="Title", core_concept="Concept", document_id="abc123"
    )
    assert model.chapter_title == "Title"
    assert model.core_concept == "Concept"
    assert model.document_id == "abc123"


def test_document_chapter_request_accepts_long_chapter_title() -> None:
    """DocumentChapterRequest has no max_length on chapter_title."""
    model = DocumentChapterRequest(
        chapter_title="a" * 5_000, core_concept="C", document_id="id"
    )
    assert len(model.chapter_title) == 5_000


def test_document_chapter_request_accepts_long_core_concept() -> None:
    """DocumentChapterRequest has no max_length on core_concept."""
    model = DocumentChapterRequest(
        chapter_title="T", core_concept="b" * 5_000, document_id="id"
    )
    assert len(model.core_concept) == 5_000


def test_document_chapter_request_missing_document_id_raises() -> None:
    """DocumentChapterRequest raises ValidationError when document_id is absent."""
    with pytest.raises(ValidationError):
        DocumentChapterRequest(  # type: ignore[call-arg]
            chapter_title="Title", core_concept="Concept"
        )


def test_document_chapter_request_missing_chapter_title_raises() -> None:
    """DocumentChapterRequest raises ValidationError when chapter_title is absent."""
    with pytest.raises(ValidationError):
        DocumentChapterRequest(  # type: ignore[call-arg]
            core_concept="Concept", document_id="id"
        )


def test_document_chapter_request_missing_core_concept_raises() -> None:
    """DocumentChapterRequest raises ValidationError when core_concept is absent."""
    with pytest.raises(ValidationError):
        DocumentChapterRequest(  # type: ignore[call-arg]
            chapter_title="Title", document_id="id"
        )


# ---------------------------------------------------------------------------
# SubchapterContent
# ---------------------------------------------------------------------------

def test_subchapter_content_valid_creation() -> None:
    """SubchapterContent is created correctly with all required fields."""
    model = _make_subchapter_content(quiz=[_quiz_question()])
    assert model.title == "Sub"
    assert len(model.quiz) == 1


def test_subchapter_content_empty_quiz_valid() -> None:
    """SubchapterContent accepts an empty quiz list (no min_length constraint)."""
    assert _make_subchapter_content(quiz=[]).quiz == []


def test_subchapter_content_accepts_large_theory_html() -> None:
    """SubchapterContent has no max_length on theory_html."""
    big = "<p>" + "x" * 100_000 + "</p>"
    assert len(_make_subchapter_content(theory_html=big).theory_html) > 100_000


def test_subchapter_content_missing_title_raises() -> None:
    """SubchapterContent raises ValidationError when title is absent."""
    with pytest.raises(ValidationError):
        SubchapterContent(  # type: ignore[call-arg]
            content_summary="Sum", theory_html="<p>x</p>", quiz=[]
        )


def test_subchapter_content_missing_theory_html_raises() -> None:
    """SubchapterContent raises ValidationError when theory_html is absent."""
    with pytest.raises(ValidationError):
        SubchapterContent(  # type: ignore[call-arg]
            title="Sub", content_summary="Sum", quiz=[]
        )


def test_subchapter_content_missing_content_summary_raises() -> None:
    """SubchapterContent raises ValidationError when content_summary is absent."""
    with pytest.raises(ValidationError):
        SubchapterContent(  # type: ignore[call-arg]
            title="Sub", theory_html="<p>x</p>", quiz=[]
        )


@pytest.mark.parametrize("quiz_len", [0, 1, 3, 10])
def test_subchapter_content_various_quiz_lengths(quiz_len: int) -> None:
    """SubchapterContent stores any number of quiz questions correctly."""
    model = _make_subchapter_content(
        quiz=[_quiz_question() for _ in range(quiz_len)]
    )
    assert len(model.quiz) == quiz_len


# ---------------------------------------------------------------------------
# FullChapterResponse
# ---------------------------------------------------------------------------

def test_full_chapter_response_valid() -> None:
    """FullChapterResponse stores subchapters and recap_quiz correctly."""
    sub = _make_subchapter_content()
    model = FullChapterResponse(
        subchapters=[sub], recap_quiz=[_quiz_question()] * 10
    )
    assert len(model.subchapters) == 1
    assert len(model.recap_quiz) == 10


def test_full_chapter_response_empty_lists_valid() -> None:
    """FullChapterResponse accepts empty subchapters and recap_quiz lists."""
    model = FullChapterResponse(subchapters=[], recap_quiz=[])
    assert model.subchapters == []
    assert model.recap_quiz == []


def test_full_chapter_response_missing_subchapters_raises() -> None:
    """FullChapterResponse raises ValidationError when subchapters is absent."""
    with pytest.raises(ValidationError):
        FullChapterResponse(recap_quiz=[])  # type: ignore[call-arg]


def test_full_chapter_response_missing_recap_quiz_raises() -> None:
    """FullChapterResponse raises ValidationError when recap_quiz is absent."""
    with pytest.raises(ValidationError):
        FullChapterResponse(subchapters=[])  # type: ignore[call-arg]


@pytest.mark.parametrize(("n_subs", "n_quiz"), [(1, 10), (3, 5), (0, 0)])
def test_full_chapter_response_various_sizes(n_subs: int, n_quiz: int) -> None:
    """FullChapterResponse correctly stores different counts of subchapters and recap questions."""
    model = FullChapterResponse(
        subchapters=[_make_subchapter_content() for _ in range(n_subs)],
        recap_quiz=[_quiz_question() for _ in range(n_quiz)],
    )
    assert len(model.subchapters) == n_subs
    assert len(model.recap_quiz) == n_quiz