FROM python:3.11-slim

WORKDIR /app

# Install LibreOffice
RUN apt-get update && apt-get install -y \
    libreoffice \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

# Install Poetry
RUN pip install --no-cache-dir poetry
RUN poetry config virtualenvs.create false

# Install Dependencies
COPY pyproject.toml poetry.lock* ./
RUN poetry install --only main --no-interaction --no-ansi --no-root

# Copy Application
COPY ./app ./app
COPY main.py .

# Fix for Exit Status 77: Give the appuser a writable home area
RUN adduser --system --no-create-home appuser
ENV HOME=/tmp
USER appuser

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]