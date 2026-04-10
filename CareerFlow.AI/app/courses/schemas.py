from __future__ import annotations

from pydantic import BaseModel

# --- Request Models ---


class CourseSkeletonRequest(BaseModel):
    topic: str


class ChapterRequest(BaseModel):
    topic: str
    chapter_title: str
    core_concept: str


# --- AI Structured Output Models ---


class SubchapterSkeleton(BaseModel):
    title: str
    content_summary: str


class ChapterSkeleton(BaseModel):
    title: str
    core_concept: str
    day: int = 0


class LearningPlanSkeleton(BaseModel):
    topic: str
    chapters: list[ChapterSkeleton]


class ExpandedDay(BaseModel):
    subchapters: list[SubchapterSkeleton]


class QuizOption(BaseModel):
    label: str
    is_correct: bool


class QuizQuestion(BaseModel):
    question: str
    options: list[QuizOption]


class SubchapterContentResponse(BaseModel):
    theory_html: str
    quiz: list[QuizQuestion]


class ChapterQuizResponse(BaseModel):
    questions: list[QuizQuestion]
