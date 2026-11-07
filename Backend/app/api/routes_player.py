"""Player cloud-save endpoints (server-authoritative).

The server validates the payload schema and ownership. Conflict rule:
a save is accepted only if its player_level >= current stored level OR the
client explicitly forces overwrite (anti-rollback protection stub).
"""
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy import select
from sqlalchemy.orm import Session

from ..api.routes_auth import current_player
from ..database import get_db
from ..models import Player, PlayerSave
from ..schemas import PlayerProfile, SaveReadResponse, SaveWriteRequest

router = APIRouter(prefix="/player", tags=["player"])


@router.get("/profile", response_model=PlayerProfile)
def profile(player: Player = Depends(current_player)):
    return PlayerProfile(
        id=player.id, username=player.username, display_name=player.display_name,
        level=player.level, exp=player.exp, created_at=player.created_at,
    )


@router.put("/save", response_model=SaveReadResponse)
def write_save(body: SaveWriteRequest,
               player: Player = Depends(current_player),
               db: Session = Depends(get_db)):
    row = db.scalar(select(PlayerSave).where(
        PlayerSave.player_id == player.id, PlayerSave.slot == body.slot))

    if row is not None:
        old_level = int(row.payload.get("playerLevel", 1))
        new_level = body.payload.player_level
        if new_level < old_level:
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                f"Anti-rollback: stored level {old_level} > submitted {new_level}",
            )

    if row is None:
        row = PlayerSave(player_id=player.id, slot=body.slot,
                         payload=body.payload.model_dump(by_alias=True))
        db.add(row)
    else:
        row.payload = body.payload.model_dump(by_alias=True)

    player.level = body.payload.player_level
    player.exp = body.payload.exp
    db.commit()
    db.refresh(row)

    return SaveReadResponse(slot=row.slot, payload=row.payload, updated_at=row.updated_at)


@router.get("/save", response_model=SaveReadResponse)
def read_save(slot: int = 0,
              player: Player = Depends(current_player),
              db: Session = Depends(get_db)):
    row = db.scalar(select(PlayerSave).where(
        PlayerSave.player_id == player.id, PlayerSave.slot == slot))
    if row is None:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "No save in this slot")
    return SaveReadResponse(slot=row.slot, payload=row.payload, updated_at=row.updated_at)
