import pytest
from httpx import AsyncClient
from fastapi import HTTPException
from unittest.mock import AsyncMock, MagicMock
from pytest_mock import MockerFixture

# Use the centralized app and dependencies from conftest
from main import app 
from app.dependencies import get_openai_client


@pytest.fixture
def mock_service(mocker: MockerFixture) -> MockerFixture:
    mocker.patch("app.courses.service.generate_skeleton", new_callable=AsyncMock)
    mocker.patch("app.courses.service.build_full_chapter", new_callable=AsyncMock)
    return mocker


@pytest.mark.asyncio
async def test_create_skeleton_happy_path(api_client: AsyncClient, mock_service: MockerFixture) -> None:
    mock_skeleton = MagicMock()
    mock_skeleton.model_dump.return_value = {"title": "Python 101", "chapters": ["Intro", "Setup"]}
    mock_skeleton.chapters = ["Intro", "Setup"]
    
    mock_service.patch("app.courses.service.generate_skeleton").return_value = mock_skeleton
    
    response = await api_client.post("/courses/skeleton", json={"topic": "Python Programming for Beginners"})
    
    assert response.status_code == 200
    data = response.json()
    assert data["estimated_days"] == 2
    assert data["skeleton"]["title"] == "Python 101"


@pytest.mark.asyncio
async def test_create_skeleton_invalid_data_422(api_client: AsyncClient) -> None:
    response = await api_client.post("/courses/skeleton", json={})
    assert response.status_code == 422
    assert response.json()["detail"][0]["loc"] == ["body", "topic"]


@pytest.mark.asyncio
async def test_create_skeleton_boundary_values(api_client: AsyncClient, mock_service: MockerFixture) -> None:
    mock_skeleton = MagicMock()
    mock_skeleton.model_dump.return_value = {"title": "Long Topic"}
    mock_skeleton.chapters = ["One"]
    mock_service.patch("app.courses.service.generate_skeleton").return_value = mock_skeleton
    
    long_topic = "A" * 10000 
    response = await api_client.post("/courses/skeleton", json={"topic": long_topic})
    
    assert response.status_code == 422
    error_detail = response.json()["detail"][0]
    assert error_detail["type"] == "string_too_long"


@pytest.mark.asyncio
async def test_expand_chapter_happy_path(api_client: AsyncClient, mock_service: MockerFixture) -> None:
    expected_response = {"content": "Welcome to Python...", "exercises": []}
    mock_service.patch("app.courses.service.build_full_chapter").return_value = expected_response
    
    payload = {
        "topic": "Python Basics",
        "chapter_title": "Variables and Types",
        "core_concept": "Understanding memory allocation"
    }
    
    response = await api_client.post("/courses/chapters/expand", json=payload)
    
    assert response.status_code == 200
    assert response.json() == expected_response


@pytest.mark.asyncio
async def test_unauthorized_access_401(api_client: AsyncClient) -> None:
    async def mock_auth_failure() -> None:
        raise HTTPException(status_code=401, detail="Not authenticated")
    
    app.dependency_overrides[get_openai_client] = mock_auth_failure
    response = await api_client.post("/courses/skeleton", json={"topic": "Test"})
    
    assert response.status_code == 401
    assert response.json()["detail"] == "Not authenticated"
    app.dependency_overrides.clear()


@pytest.mark.asyncio
async def test_courses_api_integration(api_client: AsyncClient, mock_openai_client: AsyncMock) -> None:
    mock_openai_client.beta.chat.completions.parse.return_value = MagicMock(
        choices=[MagicMock(message=MagicMock(parsed=MagicMock(
            model_dump=lambda: {"title": "Int Test", "chapters": [{"title": "Ch1"}]}
        )))]
    )
    
    payload = {"topic": "Integration Testing FastApi"}
    response = await api_client.post("/courses/skeleton", json=payload)
    
    assert response.status_code == 200
    assert mock_openai_client.beta.chat.completions.parse.called