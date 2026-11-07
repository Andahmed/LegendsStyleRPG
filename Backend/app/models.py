"""SQL models: players, auth tokens and cloud-save blobs."""
import datetime

from sqlalchemy import Boolean, DateTime, ForeignKey, Integer, JSON, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from .database import Base


def utcnow() -> datetime.datetime:
    # naive UTC — SQLite does not preserve tzinfo; keep every timestamp
    # in the same (naive-UTC) convention to avoid comparison TypeErrors.
    return datetime.datetime.now(datetime.timezone.utc).replace(tzinfo=None)


class Player(Base):
    __tablename__ = "players"

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    username: Mapped[str] = mapped_column(String(64), unique=True, index=True)
    password_hash: Mapped[str] = mapped_column(String(256))
    display_name: Mapped[str] = mapped_column(String(64), default="")
    level: Mapped[int] = mapped_column(Integer, default=1)
    exp: Mapped[float] = mapped_column(default=0.0)
    is_banned: Mapped[bool] = mapped_column(Boolean, default=False)
    created_at: Mapped[datetime.datetime] = mapped_column(DateTime(), default=utcnow)

    saves: Mapped[list["PlayerSave"]] = relationship(
        back_populates="player", cascade="all, delete-orphan"
    )
    tokens: Mapped[list["AuthToken"]] = relationship(
        back_populates="player", cascade="all, delete-orphan"
    )


class AuthToken(Base):
    __tablename__ = "auth_tokens"

    token: Mapped[str] = mapped_column(String(64), primary_key=True)
    player_id: Mapped[int] = mapped_column(ForeignKey("players.id"), index=True)
    created_at: Mapped[datetime.datetime] = mapped_column(DateTime(), default=utcnow)
    expires_at: Mapped[datetime.datetime] = mapped_column(DateTime())
    revoked: Mapped[bool] = mapped_column(Boolean, default=False)

    player: Mapped[Player] = relationship(back_populates="tokens")


class PlayerSave(Base):
    """Server-authoritative save slot (schema-compatible with the client JSON)."""
    __tablename__ = "player_saves"
    __table_args__ = (UniqueConstraint("player_id", "slot", name="uq_player_slot"),)

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    player_id: Mapped[int] = mapped_column(ForeignKey("players.id"), index=True)
    slot: Mapped[int] = mapped_column(Integer, default=0)
    payload: Mapped[dict] = mapped_column(JSON)
    updated_at: Mapped[datetime.datetime] = mapped_column(
        DateTime(), default=utcnow, onupdate=utcnow
    )

    player: Mapped[Player] = relationship(back_populates="saves")
