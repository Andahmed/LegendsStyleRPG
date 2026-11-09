"""Runtime configuration read from environment variables.

Development vs production are separated via env files (see Deployment/config).
No secrets are ever hard-coded here or shipped inside the game client.
"""
import os
from dataclasses import dataclass, field

_KNOWN_DB_SCHEMES = ("sqlite", "postgresql", "postgres", "mysql", "mssql", "oracle")


def _database_url() -> str:
    """Return DATABASE_URL, falling back to local SQLite when the inherited
    environment variable is not a recognizable SQLAlchemy URL (defensive)."""
    raw = os.getenv("DATABASE_URL", "").strip()
    if raw.split(":", 1)[0].lower() in _KNOWN_DB_SCHEMES and "+psycopg" not in raw.split(":", 1)[0]:
        # accept both "postgresql://" and "postgresql+psycopg://"
        return raw
    if raw.startswith("postgresql+psycopg://") or raw.startswith("postgresql://"):
        return raw
    if raw and not raw.lower().startswith(_KNOWN_DB_SCHEMES):
        import warnings
        warnings.warn(
            f"Ignoring unsupported DATABASE_URL ({raw.split(':', 1)[0]}...); using local SQLite."
        )
    return raw if raw.lower().startswith(_KNOWN_DB_SCHEMES) else "sqlite:///./legends_dev.db"


def _bool(name: str, default: str = "false") -> bool:
    return os.getenv(name, default).strip().lower() in ("1", "true", "yes", "on")


@dataclass
class Settings:
    app_name: str = "LegendsStyleRPG API"
    environment: str = os.getenv("APP_ENV", "dev")            # dev | prod
    database_url: str = field(default_factory=_database_url)
    host: str = os.getenv("HOST", "0.0.0.0")
    port: int = int(os.getenv("PORT", "8000"))
    token_ttl_minutes: int = int(os.getenv("TOKEN_TTL_MINUTES", "720"))
    cors_origins: list = field(default_factory=lambda: [
        o.strip() for o in os.getenv("CORS_ORIGINS", "*").split(",") if o.strip()
    ])
    debug_echo_sql: bool = _bool("DEBUG_ECHO_SQL")

    @property
    def sqlalchemy_connect_args(self) -> dict:
        # SQLite needs check_same_thread=False for FastAPI threadpool
        if self.database_url.startswith("sqlite"):
            return {"check_same_thread": False}
        return {}


settings = Settings()
