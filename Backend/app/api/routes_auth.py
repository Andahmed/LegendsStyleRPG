"""Authentication: register / login / logout / me.

Security invariants:
- Passwords are PBKDF2-hashed, never stored or logged in plaintext.
- Tokens are opaque random values with server-side expiry.
- The client is NEVER trusted for account state; everything is validated here.
"""
import datetime

from fastapi import APIRouter, Depends, Header, HTTPException, status
from sqlalchemy import select
from sqlalchemy.orm import Session

from ..database import get_db
from ..models import AuthToken, Player, utcnow
from ..schemas import LoginRequest, PlayerProfile, RegisterRequest, TokenResponse
from ..security import hash_password, issue_token, verify_password

router = APIRouter(prefix="/auth", tags=["auth"])


def _require_token(db: Session, authorization: str | None) -> AuthToken:
    if not authorization or not authorization.lower().startswith("bearer "):
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Missing bearer token")
    token_value = authorization.split(" ", 1)[1].strip()

    row = db.get(AuthToken, token_value)
    if row is None or row.revoked:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid token")
    if row.expires_at < utcnow():
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Token expired")
    return row


def current_player(authorization: str | None = Header(default=None),
                   db: Session = Depends(get_db)) -> Player:
    token_row = _require_token(db, authorization)
    player = db.get(Player, token_row.player_id)
    if player is None or player.is_banned:
        raise HTTPException(status.HTTP_403_FORBIDDEN, "Account unavailable")
    return player


@router.post("/register", response_model=TokenResponse, status_code=201)
def register(body: RegisterRequest, db: Session = Depends(get_db)):
    exists = db.scalar(select(Player).where(Player.username == body.username.lower()))
    if exists:
        raise HTTPException(status.HTTP_409_CONFLICT, "Username already taken")

    player = Player(
        username=body.username.lower(),
        password_hash=hash_password(body.password),
        display_name=body.display_name or body.username,
    )
    db.add(player)
    db.commit()
    db.refresh(player)

    token, expires = issue_token()
    db.add(AuthToken(token=token, player_id=player.id, expires_at=expires))
    db.commit()
    return TokenResponse(token=token, expires_at=expires, player_id=player.id,
                         username=player.username)


@router.post("/login", response_model=TokenResponse)
def login(body: LoginRequest, db: Session = Depends(get_db)):
    player = db.scalar(select(Player).where(Player.username == body.username.lower()))
    if player is None or not verify_password(body.password, player.password_hash):
        # uniform error — never reveal which part was wrong
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid credentials")
    if player.is_banned:
        raise HTTPException(status.HTTP_403_FORBIDDEN, "Account banned")

    token, expires = issue_token()
    db.add(AuthToken(token=token, player_id=player.id, expires_at=expires))
    db.commit()
    return TokenResponse(token=token, expires_at=expires, player_id=player.id,
                         username=player.username)


@router.post("/logout", status_code=204)
def logout(authorization: str | None = Header(default=None),
           db: Session = Depends(get_db)):
    token_row = _require_token(db, authorization)
    token_row.revoked = True
    db.commit()
    return None


@router.get("/me", response_model=PlayerProfile)
def me(player: Player = Depends(current_player)):
    return PlayerProfile(
        id=player.id, username=player.username, display_name=player.display_name,
        level=player.level, exp=player.exp, created_at=player.created_at,
    )


def cleanup_expired_tokens(db: Session) -> int:
    """Housekeeping helper (callable from a scheduler job)."""
    now_naive = datetime.datetime.now(datetime.timezone.utc).replace(tzinfo=None)
    result = db.execute(
        select(AuthToken).where(AuthToken.expires_at < now_naive)
    ).scalars().all()
    for row in result:
        db.delete(row)
    db.commit()
    return len(result)
