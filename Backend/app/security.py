"""Password hashing (PBKDF2, stdlib) and opaque token helpers.

Deliberately dependency-free: PBKDF2-HMAC-SHA256 with per-user salt.
Tokens are random 256-bit values stored server-side with expiry.
"""
import datetime
import hashlib
import hmac
import secrets

from .config import settings

_PBKDF2_ITERATIONS = 240_000


def hash_password(password: str) -> str:
    salt = secrets.token_bytes(16)
    digest = hashlib.pbkdf2_hmac("sha256", password.encode("utf-8"), salt, _PBKDF2_ITERATIONS)
    return f"pbkdf2_sha256${_PBKDF2_ITERATIONS}${salt.hex()}${digest.hex()}"


def verify_password(password: str, stored: str) -> bool:
    try:
        algo, iterations, salt_hex, digest_hex = stored.split("$")
        if algo != "pbkdf2_sha256":
            return False
        digest = hashlib.pbkdf2_hmac(
            "sha256", password.encode("utf-8"), bytes.fromhex(salt_hex), int(iterations)
        )
        return hmac.compare_digest(digest.hex(), digest_hex)
    except (ValueError, TypeError):
        return False


def issue_token(ttl_minutes: int | None = None) -> tuple[str, datetime.datetime]:
    ttl = ttl_minutes or settings.token_ttl_minutes
    token = secrets.token_hex(32)
    # naive UTC to match the SQLite timestamp convention (see models.utcnow)
    expires = datetime.datetime.now(datetime.timezone.utc).replace(tzinfo=None) + datetime.timedelta(minutes=ttl)
    return token, expires
