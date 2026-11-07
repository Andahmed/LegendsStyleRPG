"""Health & readiness endpoints (used by deployment checks)."""
from fastapi import APIRouter

from ..config import settings

router = APIRouter(tags=["health"])


@router.get("/health")
def health():
    return {"status": "ok", "env": settings.environment, "service": settings.app_name}
