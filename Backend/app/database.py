"""SQLAlchemy database setup (SQLite for dev, PostgreSQL-ready via DATABASE_URL)."""
from sqlalchemy import create_engine
from sqlalchemy.orm import DeclarativeBase, sessionmaker

from .config import settings


class Base(DeclarativeBase):
    pass


engine = create_engine(
    settings.database_url,
    echo=settings.debug_echo_sql,
    connect_args=settings.sqlalchemy_connect_args,
    pool_pre_ping=True,
)

SessionLocal = sessionmaker(bind=engine, autoflush=False, autocommit=False)


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def init_db() -> None:
    """Create tables (dev convenience). Production should use Alembic migrations."""
    from . import models  # noqa: F401
    Base.metadata.create_all(bind=engine)
