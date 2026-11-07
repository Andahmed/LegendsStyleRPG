"""FastAPI application factory."""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from .api import routes_auth, routes_health, routes_player
from .config import settings
from .database import init_db


def create_app() -> FastAPI:
    app = FastAPI(
        title=settings.app_name,
        version="0.1.0",
        docs_url="/docs" if settings.environment == "dev" else None,
    )

    if settings.cors_origins:
        app.add_middleware(
            CORSMiddleware,
            allow_origins=settings.cors_origins,
            allow_credentials=True,
            allow_methods=["*"],
            allow_headers=["*"],
        )

    app.include_router(routes_health.router, prefix="/api/v1")
    app.include_router(routes_auth.router, prefix="/api/v1")
    app.include_router(routes_player.router, prefix="/api/v1")

    @app.on_event("startup")
    def _startup() -> None:
        init_db()

    return app


app = create_app()
