import pytest
from pydantic import ValidationError

from app.courses.schemas import (
    CourseSkeletonRequest,
    ChapterRequest,
    SubchapterSkeleton,
    ChapterSkeleton,
    LearningPlanSkeleton,
    ExpandedDay,
    QuizOption,
    QuizQuestion,
    SubchapterContentResponse,
    ChapterQuizResponse,
)


def get_string_of_length(length: int) -> str:
    return "A" * length


def test_course_skeleton_request_happy_path() -> None:
    model = CourseSkeletonRequest(topic="Python Programming")
    assert model.topic == "Python Programming"


def test_course_skeleton_request_max_length_boundary() -> None:
    model = CourseSkeletonRequest(topic=get_string_of_length(200))
    assert len(model.topic) == 200


def test_course_skeleton_request_exceeds_max_length() -> None:
    with pytest.raises(ValidationError) as exc_info:
        CourseSkeletonRequest(topic=get_string_of_length(201))
    
    errors = exc_info.value.errors()
    assert errors[0]["type"] == "string_too_long"
    assert errors[0]["loc"] == ("topic",)


def test_chapter_request_missing_fields() -> None:
    with pytest.raises(ValidationError) as exc_info:
        # Use model_validate to test validation errors safely without mypy complaining
        ChapterRequest.model_validate({"topic": "Topic"}) 
    
    errors = exc_info.value.errors()
    assert len(errors) == 2
    assert errors[0]["loc"] == ("chapter_title",)
    assert errors[1]["loc"] == ("core_concept",)


def test_learning_plan_skeleton_happy_path() -> None:
    chapter = ChapterSkeleton(title="Ch 1", core_concept="Intro")
    model = LearningPlanSkeleton(topic="FastAPI", chapters=[chapter])
    
    assert model.topic == "FastAPI"
    assert len(model.chapters) == 1
    assert model.chapters[0].title == "Ch 1"


def test_learning_plan_skeleton_empty_list() -> None:
    model = LearningPlanSkeleton(topic="Empty Course", chapters=[])
    assert model.chapters == []


def test_expanded_day_happy_path() -> None:
    subchapter = SubchapterSkeleton(title="Sub 1", content_summary="Summary")
    model = ExpandedDay(subchapters=[subchapter])
    assert len(model.subchapters) == 1


def test_quiz_option_happy_path() -> None:
    model = QuizOption(label="Yes", is_correct=True)
    assert model.label == "Yes"
    assert model.is_correct is True


def test_quiz_option_invalid_type() -> None:
    with pytest.raises(ValidationError) as exc_info:
        # Safely trigger validation failure for mypy compatibility
        QuizOption.model_validate({"label": "Maybe", "is_correct": "Not a boolean"}) 
        
    errors = exc_info.value.errors()
    assert errors[0]["type"] == "bool_parsing"


def test_quiz_question_and_response_happy_path() -> None:
    option1 = QuizOption(label="A", is_correct=True)
    option2 = QuizOption(label="B", is_correct=False)
    question = QuizQuestion(question="What is A?", options=[option1, option2])
    response = ChapterQuizResponse(questions=[question])
    
    assert len(response.questions) == 1
    assert response.questions[0].options[0].is_correct is True


def test_subchapter_content_response_html_length_warning() -> None:
    valid_html = "<p>" + get_string_of_length(190) + "</p>"
    invalid_html = "<p>" + get_string_of_length(200) + "</p>" 
    
    model = SubchapterContentResponse(theory_html=valid_html, quiz=[])
    assert model.theory_html == valid_html
    
    with pytest.raises(ValidationError) as exc_info:
        SubchapterContentResponse(theory_html=invalid_html, quiz=[])
        
    errors = exc_info.value.errors()
    assert errors[0]["type"] == "string_too_long"