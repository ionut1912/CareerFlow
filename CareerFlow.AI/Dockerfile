FROM python:3.11-slim

WORKDIR /app

# 1. Install LibreOffice (Required for .doc support as per your pyproject.toml)
# We combine these commands to keep the image layer count and size down.
RUN apt-get update && apt-get install -y \
    libreoffice \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

# 2. Install Poetry
RUN pip install --no-cache-dir poetry
RUN poetry config virtualenvs.create false

# 3. Install Python dependencies
COPY pyproject.toml poetry.lock* ./
RUN poetry install --only main --no-interaction --no-ansi --no-root

# 4. Copy application code
COPY ./app ./app
COPY main.py .

# 5. Switch to non-root user for security
# Note: We do this LAST because system installs (apt-get) need root access.
RUN adduser --system --no-create-home appuser
USER appuser

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]