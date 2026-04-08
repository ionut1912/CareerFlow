from openai import AsyncOpenAI

from app.config import settings


def get_openai_client() -> AsyncOpenAI:
    return AsyncOpenAI(api_key=settings.openai_api_key)