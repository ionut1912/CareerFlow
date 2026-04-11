import os

from fastapi import FastAPI

from app.courses.router import router as courses_router
from app.document_reader.router import router as document_router

tags_metadata = [
    {"name": "Generare AI", "description": "Endpoint-uri pentru generare lazy-loaded"},
    {"name": "Sistem", "description": "Health check"},
]

app = FastAPI(
    title="CareerFlow AI Service",
    version="1.0.0",
    root_path="/ai" if os.getenv("ENVIRONMENT") == "production" else "",
    docs_url="/swagger",
    redoc_url=None,
    openapi_tags=tags_metadata,
)

app.include_router(courses_router)
app.include_router(document_router)


@app.get("/", tags=["Sistem"])
def read_root() -> dict[str, str]:
    return {"service": "CareerFlow.AI", "status": "active"}
