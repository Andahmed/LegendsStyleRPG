"""Pydantic request/response schemas."""
import datetime

from pydantic import BaseModel, Field, field_validator


class RegisterRequest(BaseModel):
    username: str = Field(min_length=3, max_length=32, pattern=r"^[A-Za-z0-9_]+$")
    password: str = Field(min_length=6, max_length=128)
    display_name: str = Field(default="", max_length=64)


class LoginRequest(BaseModel):
    username: str
    password: str


class TokenResponse(BaseModel):
    token: str
    expires_at: datetime.datetime
    player_id: int
    username: str


class SavePayload(BaseModel):
    """Mirrors the client SaveData JSON (schemaVersion 1)."""
    schema_version: int = Field(default=1, alias="schemaVersion")
    player_level: int = Field(default=1, ge=1, le=999, alias="playerLevel")
    exp: float = Field(default=0.0, ge=0)
    unlocked_hero_ids: list[str] = Field(default_factory=list, alias="unlockedHeroIds")
    active_difficulty: int = Field(default=0, ge=0, le=3, alias="activeDifficulty")
    tower: list[dict] = Field(default_factory=list)
    last_save_utc: str = Field(default="", alias="lastSaveUtc")

    model_config = {"populate_by_name": True}


class SaveWriteRequest(BaseModel):
    slot: int = Field(default=0, ge=0, le=5)
    payload: SavePayload


class SaveReadResponse(BaseModel):
    slot: int
    payload: dict
    updated_at: datetime.datetime


class PlayerProfile(BaseModel):
    id: int
    username: str
    display_name: str
    level: int
    exp: float
    created_at: datetime.datetime
