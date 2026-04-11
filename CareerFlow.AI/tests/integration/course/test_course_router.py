"""Tests for app/courses/router.py"""

from __future__ import annotations

from unittest.mock import AsyncMock

import pytest
from httpx import AsyncClient

from tests.conftest import wire_course


class TestUnitCoursesRouter:
    """Unit tests: router delegates correctly to service and shapes the response."""

    @pytest.mark.anyio
    async def test_create_skeleton_happy_path(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """POST /courses/skeleton returns skeleton dict and correct estimated_days."""
        wire_course(mock_openai_client, num_days=2, num_subchapters=1)
        response = await api_client.post("/courses/skeleton", json={"topic": "Python"})

        assert response.status_code == 200
        body = response.json()
        assert "skeleton" in body
        assert "estimated_days" in body
        assert body["estimated_days"] == 2
        assert body["skeleton"]["topic"] == "Python"

    @pytest.mark.anyio
    async def test_create_skeleton_single_day(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """POST /courses/skeleton with a one-day plan returns estimated_days=1."""
        wire_course(mock_openai_client, num_days=1, num_subchapters=1)
        response = await api_client.post("/courses/skeleton", json={"topic": "Git"})

        assert response.status_code == 200
        assert response.json()["estimated_days"] == 1

    @pytest.mark.anyio
    async def test_create_skeleton_missing_topic(self, api_client: AsyncClient) -> None:
        """POST /courses/skeleton without topic returns 422 validation error."""
        response = await api_client.post("/courses/skeleton", json={})
        assert response.status_code == 422

    @pytest.mark.anyio
    async def test_create_skeleton_empty_topic(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """POST /courses/skeleton with an empty string topic still calls service."""
        wire_course(mock_openai_client, num_days=1, num_subchapters=1)
        response = await api_client.post("/courses/skeleton", json={"topic": ""})
        assert response.status_code == 200

    @pytest.mark.anyio
    async def test_expand_chapter_happy_path(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """POST /courses/chapters/expand returns chapter, expanded, subchapter_contents and quiz."""
        wire_course(mock_openai_client, num_days=1, num_subchapters=2)
        payload = {
            "topic": "Python",
            "chapter_title": "Ziua 1: Fundamente",
            "core_concept": "Sintaxa de baza",
        }
        response = await api_client.post("/courses/chapters/expand", json=payload)

        assert response.status_code == 200
        body = response.json()
        assert "chapter" in body
        assert "expanded" in body
        assert "subchapter_contents" in body
        assert "quiz" in body
        assert len(body["subchapter_contents"]) == 2

    @pytest.mark.anyio
    async def test_expand_chapter_missing_fields(self, api_client: AsyncClient) -> None:
        """POST /courses/chapters/expand without required fields returns 422."""
        response = await api_client.post("/courses/chapters/expand", json={"topic": "Python"})
        assert response.status_code == 422

    @pytest.mark.anyio
    async def test_expand_chapter_quiz_structure(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """Expanded chapter quiz has 10 questions each with 4 options."""
        wire_course(mock_openai_client, num_days=1, num_subchapters=1)
        payload = {
            "topic": "Python",
            "chapter_title": "Ziua 1",
            "core_concept": "Variabile",
        }
        response = await api_client.post("/courses/chapters/expand", json=payload)

        quiz = response.json()["quiz"]["questions"]
        assert len(quiz) == 10
        for q in quiz:
            assert len(q["options"]) == 4

    @pytest.mark.anyio
    async def test_create_skeleton_service_raises_propagates(
        self, api_client: AsyncClient, mock_openai_client: AsyncMock
    ) -> None:
        """If the AI service raises RuntimeError the router returns 500."""
        mock_openai_client.beta.chat.completions.parse.side_effect = RuntimeError("boom")
        response = await api_client.post("/courses/skeleton", json={"topic": "Python"})
        assert response.status_code == 500

    @pytest.mark.anyio
    async def test_expand_chapter_subchapter_content_fields(
        self, api_client: AsyncClient, mock_openai_client: AsyncMock
    ) -> None:
        """Each subchapter_content entry contains theory_html and quiz keys."""
        wire_course(mock_openai_client, num_days=1, num_subchapters=1)
        payload = {
            "topic": "Python",
            "chapter_title": "Ziua 1",
            "core_concept": "Bucle",
        }
        response = await api_client.post("/courses/chapters/expand", json=payload)
        sc = response.json()["subchapter_contents"][0]
        assert "theory_html" in sc
        assert "quiz" in sc


class TestIntegrationCoursesRouter:
    """Integration tests: full request/response cycle with wired mocks."""

    @pytest.mark.anyio
    async def test_skeleton_then_expand_workflow(self, api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
        """A skeleton response's chapter data is valid input for the expand endpoint."""
        wire_course(mock_openai_client, num_days=1, num_subchapters=1)
        skel_resp = await api_client.post("/courses/skeleton", json={"topic": "FastAPI"})
        assert skel_resp.status_code == 200
        chapter = skel_resp.json()["skeleton"]["chapters"][0]

        wire_course(mock_openai_client, num_days=1, num_subchapters=1)
        expand_resp = await api_client.post(
            "/courses/chapters/expand",
            json={
                "topic": "FastAPI",
                "chapter_title": chapter["title"],
                "core_concept": chapter["core_concept"],
            },
        )
        assert expand_resp.status_code == 200
        assert "subchapter_contents" in expand_resp.json()

    @pytest.mark.anyio
    @pytest.mark.parametrize(("num_days", "num_subs"), [(1, 1), (3, 2), (5, 3)])
    async def test_expand_various_sizes(
        self,
        api_client: AsyncClient,
        mock_openai_client: AsyncMock,
        num_days: int,
        num_subs: int,
    ) -> None:
        """expand_chapter returns the correct number of subchapter_contents."""
        wire_course(mock_openai_client, num_days=num_days, num_subchapters=num_subs)
        response = await api_client.post(
            "/courses/chapters/expand",
            json={
                "topic": "Python",
                "chapter_title": "Ziua 1",
                "core_concept": "Concept",
            },
        )
        assert response.status_code == 200
        assert len(response.json()["subchapter_contents"]) == num_subs