from __future__ import annotations

from pydantic import BaseModel

from app.courses.schemas import ChapterSkeleton, QuizQuestion


class DocumentAnalysis(BaseModel):
    title: str
    summary: str
    key_topics: list[str]


class LearningPlanSkeleton(BaseModel):
    topic: str
    chapters: list[ChapterSkeleton]


class AnalysisAndSkeleton(BaseModel):
    analysis: DocumentAnalysis
    skeleton: LearningPlanSkeleton


class DocumentChapterRequest(BaseModel):
    chapter_title: str
    core_concept: str
    document_id: str


class SubchapterContent(BaseModel):
    title: str
    content_summary: str
    theory_html: str
    quiz: list[QuizQuestion]


class FullChapterResponse(BaseModel):
    subchapters: list[SubchapterContent]
    recap_quiz: list[QuizQuestion]