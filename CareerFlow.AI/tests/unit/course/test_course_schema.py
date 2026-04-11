"""Tests for app/courses/schemas.py"""

from __future__ import annotations

import pytest
from pydantic import ValidationError

from app.courses.schema import (
    ChapterQuizResponse,
    ChapterRequest,
    ChapterSkeleton,
    CourseSkeletonRequest,
    ExpandedDay,
    LearningPlanSkeleton,
    QuizOption,
    QuizQuestion,
    SubchapterContentResponse,
    SubchapterSkeleton,
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


# ---------------------------------------------------------------------------
# CourseSkeletonRequest
# ---------------------------------------------------------------------------


def test_course_skeleton_request_valid() -> None:
    """CourseSkeletonRequest stores the topic correctly."""
    assert CourseSkeletonRequest(topic="Python").topic == "Python"


def test_course_skeleton_request_accepts_long_topic() -> None:
    """CourseSkeletonRequest has no max_length on topic; any-length string is valid."""
    long_topic = "a" * 10_000
    assert CourseSkeletonRequest(topic=long_topic).topic == long_topic


def test_course_skeleton_request_requires_topic() -> None:
    """CourseSkeletonRequest raises ValidationError when topic is missing."""
    with pytest.raises(ValidationError):
        CourseSkeletonRequest()  # type: ignore[call-arg]


def test_course_skeleton_request_topic_none_raises() -> None:
    """CourseSkeletonRequest raises ValidationError when topic is None."""
    with pytest.raises(ValidationError):
        CourseSkeletonRequest(topic=None)  # type: ignore[arg-type]


# ---------------------------------------------------------------------------
# ChapterRequest
# ---------------------------------------------------------------------------


def test_chapter_request_valid() -> None:
    """ChapterRequest stores all three fields correctly."""
    model = ChapterRequest(topic="T", chapter_title="CT", core_concept="CC")
    assert model.topic == "T"
    assert model.chapter_title == "CT"
    assert model.core_concept == "CC"


def test_chapter_request_missing_core_concept_raises() -> None:
    """ChapterRequest raises ValidationError when core_concept is absent."""
    with pytest.raises(ValidationError):
        ChapterRequest(topic="T", chapter_title="CT")  # type: ignore[call-arg]


def test_chapter_request_missing_topic_raises() -> None:
    """ChapterRequest raises ValidationError when topic is absent."""
    with pytest.raises(ValidationError):
        ChapterRequest(chapter_title="CT", core_concept="CC")  # type: ignore[call-arg]


# ---------------------------------------------------------------------------
# ChapterSkeleton
# ---------------------------------------------------------------------------


def test_chapter_skeleton_default_day() -> None:
    """ChapterSkeleton.day defaults to 0."""
    assert ChapterSkeleton(title="T", core_concept="CC").day == 0


def test_chapter_skeleton_custom_day() -> None:
    """ChapterSkeleton.day stores the provided value."""
    assert ChapterSkeleton(title="T", core_concept="CC", day=5).day == 5


def test_chapter_skeleton_missing_title_raises() -> None:
    """ChapterSkeleton raises ValidationError when title is missing."""
    with pytest.raises(ValidationError):
        ChapterSkeleton(core_concept="CC")  # type: ignore[call-arg]


def test_chapter_skeleton_missing_core_concept_raises() -> None:
    """ChapterSkeleton raises ValidationError when core_concept is missing."""
    with pytest.raises(ValidationError):
        ChapterSkeleton(title="T")  # type: ignore[call-arg]


# ---------------------------------------------------------------------------
# LearningPlanSkeleton
# ---------------------------------------------------------------------------


def test_learning_plan_skeleton_stores_chapters() -> None:
    """LearningPlanSkeleton stores all supplied chapters."""
    chapters = [ChapterSkeleton(title=f"Ziua {i}", core_concept="C") for i in range(3)]
    model = LearningPlanSkeleton(topic="Python", chapters=chapters)
    assert len(model.chapters) == 3
    assert model.topic == "Python"


def test_learning_plan_skeleton_empty_chapters() -> None:
    """LearningPlanSkeleton accepts an empty chapters list."""
    assert LearningPlanSkeleton(topic="Python", chapters=[]).chapters == []


def test_learning_plan_skeleton_missing_topic_raises() -> None:
    """LearningPlanSkeleton raises ValidationError when topic is absent."""
    with pytest.raises(ValidationError):
        LearningPlanSkeleton(chapters=[])  # type: ignore[call-arg]


# ---------------------------------------------------------------------------
# SubchapterSkeleton
# ---------------------------------------------------------------------------


def test_subchapter_skeleton_valid() -> None:
    """SubchapterSkeleton stores title and content_summary correctly."""
    model = SubchapterSkeleton(title="Sub", content_summary="Summary")
    assert model.title == "Sub"
    assert model.content_summary == "Summary"


def test_subchapter_skeleton_missing_content_summary_raises() -> None:
    """SubchapterSkeleton raises ValidationError when content_summary is absent."""
    with pytest.raises(ValidationError):
        SubchapterSkeleton(title="Sub")  # type: ignore[call-arg]


def test_subchapter_skeleton_missing_title_raises() -> None:
    """SubchapterSkeleton raises ValidationError when title is absent."""
    with pytest.raises(ValidationError):
        SubchapterSkeleton(content_summary="Summary")  # type: ignore[call-arg]


# ---------------------------------------------------------------------------
# ExpandedDay
# ---------------------------------------------------------------------------


def test_expanded_day_stores_subchapters() -> None:
    """ExpandedDay stores all provided subchapters."""
    subs = [SubchapterSkeleton(title=f"S{i}", content_summary="x") for i in range(4)]
    assert len(ExpandedDay(subchapters=subs).subchapters) == 4


def test_expanded_day_empty_subchapters() -> None:
    """ExpandedDay accepts an empty subchapters list."""
    assert ExpandedDay(subchapters=[]).subchapters == []


def test_expanded_day_missing_subchapters_raises() -> None:
    """ExpandedDay raises ValidationError when subchapters is absent."""
    with pytest.raises(ValidationError):
        ExpandedDay()  # type: ignore[call-arg]


# ---------------------------------------------------------------------------
# QuizOption
# ---------------------------------------------------------------------------


def test_quiz_option_correct() -> None:
    """QuizOption stores label and is_correct=True correctly."""
    opt = QuizOption(label="Answer A", is_correct=True)
    assert opt.label == "Answer A"
    assert opt.is_correct is True


def test_quiz_option_incorrect() -> None:
    """QuizOption stores is_correct=False correctly."""
    assert QuizOption(label="Wrong", is_correct=False).is_correct is False


def test_quiz_option_missing_label_raises() -> None:
    """QuizOption raises ValidationError when label is missing."""
    with pytest.raises(ValidationError):
        QuizOption(is_correct=False)  # type: ignore[call-arg]


def test_quiz_option_missing_is_correct_raises() -> None:
    """QuizOption raises ValidationError when is_correct is missing."""
    with pytest.raises(ValidationError):
        QuizOption(label="A")  # type: ignore[call-arg]


# ---------------------------------------------------------------------------
# QuizQuestion
# ---------------------------------------------------------------------------


def test_quiz_question_valid() -> None:
    """QuizQuestion stores question text and all options correctly."""
    q = _quiz_question("What is X?")
    assert q.question == "What is X?"
    assert len(q.options) == 4


def test_quiz_question_missing_question_raises() -> None:
    """QuizQuestion raises ValidationError when question is missing."""
    with pytest.raises(ValidationError):
        QuizQuestion(options=[])  # type: ignore[call-arg]


def test_quiz_question_missing_options_raises() -> None:
    """QuizQuestion raises ValidationError when options is missing."""
    with pytest.raises(ValidationError):
        QuizQuestion(question="Q?")  # type: ignore[call-arg]


def test_quiz_question_empty_options_valid() -> None:
    """QuizQuestion accepts an empty options list (no min_length constraint)."""
    assert QuizQuestion(question="Q?", options=[]).options == []


# ---------------------------------------------------------------------------
# SubchapterContentResponse
# ---------------------------------------------------------------------------


def test_subchapter_content_response_valid() -> None:
    """SubchapterContentResponse stores theory_html and quiz correctly."""
    model = SubchapterContentResponse(theory_html="<p>Content</p>", quiz=[_quiz_question()])
    assert "<p>Content</p>" in model.theory_html
    assert len(model.quiz) == 1


def test_subchapter_content_response_accepts_large_html() -> None:
    """SubchapterContentResponse has no max_length on theory_html."""
    big_html = "<p>" + "x" * 50_000 + "</p>"
    assert SubchapterContentResponse(theory_html=big_html, quiz=[]).theory_html == big_html


def test_subchapter_content_response_requires_theory_html() -> None:
    """SubchapterContentResponse raises ValidationError when theory_html is absent."""
    with pytest.raises(ValidationError):
        SubchapterContentResponse(quiz=[])  # type: ignore[call-arg]


def test_subchapter_content_response_empty_quiz_valid() -> None:
    """SubchapterContentResponse accepts an empty quiz list."""
    assert SubchapterContentResponse(theory_html="<p>x</p>", quiz=[]).quiz == []


# ---------------------------------------------------------------------------
# ChapterQuizResponse
# ---------------------------------------------------------------------------


def test_chapter_quiz_response_stores_questions() -> None:
    """ChapterQuizResponse stores all supplied questions."""
    questions = [_quiz_question(f"Q{i}?") for i in range(10)]
    assert len(ChapterQuizResponse(questions=questions).questions) == 10


def test_chapter_quiz_response_empty_questions() -> None:
    """ChapterQuizResponse accepts an empty questions list."""
    assert ChapterQuizResponse(questions=[]).questions == []


def test_chapter_quiz_response_missing_questions_raises() -> None:
    """ChapterQuizResponse raises ValidationError when questions is absent."""
    with pytest.raises(ValidationError):
        ChapterQuizResponse()  # type: ignore[call-arg]


@pytest.mark.parametrize("count", [1, 5, 10])
def test_chapter_quiz_response_various_counts(count: int) -> None:
    """ChapterQuizResponse stores any number of questions correctly."""
    model = ChapterQuizResponse(questions=[_quiz_question(f"Q{i}?") for i in range(count)])
    assert len(model.questions) == count