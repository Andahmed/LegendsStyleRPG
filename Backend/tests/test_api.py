"""API tests: health, register/login, save write/read, anti-rollback.

Run from the Backend folder:
    python -m pytest tests/ -v
"""
import os
import sys

# Override any inherited DATABASE_URL *before* importing the app so the
# engine is created against a local test database.
os.environ["DATABASE_URL"] = "sqlite:///./legends_test.db"
os.environ["APP_ENV"] = "dev"

import pytest
from fastapi.testclient import TestClient

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from app.main import app  # noqa: E402


@pytest.fixture()
def client(tmp_path, monkeypatch):
    # isolated SQLite per test run
    monkeypatch.setenv("DATABASE_URL", f"sqlite:///{tmp_path}/test.db")
    from app.database import engine, Base
    from app import models  # noqa: F401
    import app.main as main_mod

    # rebuild engine for the temp DB
    from sqlalchemy import create_engine
    from sqlalchemy.orm import sessionmaker
    test_engine = create_engine(f"sqlite:///{tmp_path}/test.db", connect_args={"check_same_thread": False})
    Base.metadata.create_all(test_engine)
    TestSession = sessionmaker(bind=test_engine)

    # patch dependency
    from app.database import get_db

    def override_get_db():
        db = TestSession()
        try:
            yield db
        finally:
            db.close()

    app.dependency_overrides[get_db] = override_get_db
    with TestClient(app) as c:
        yield c
    app.dependency_overrides.clear()


def test_health(client):
    r = client.get("/api/v1/health")
    assert r.status_code == 200
    assert r.json()["status"] == "ok"


def test_register_and_me(client):
    r = client.post("/api/v1/auth/register", json={
        "username": "hero_one", "password": "secret123", "display_name": "Hero One"})
    assert r.status_code == 201
    token = r.json()["token"]

    r = client.get("/api/v1/auth/me", headers={"Authorization": f"Bearer {token}"})
    assert r.status_code == 200
    assert r.json()["username"] == "hero_one"


def test_duplicate_register_rejected(client):
    client.post("/api/v1/auth/register", json={"username": "dup", "password": "secret123"})
    r = client.post("/api/v1/auth/register", json={"username": "dup", "password": "other456"})
    assert r.status_code == 409


def test_login_and_bad_password(client):
    client.post("/api/v1/auth/register", json={"username": "login_test", "password": "abc12345"})
    ok = client.post("/api/v1/auth/login", json={"username": "login_test", "password": "abc12345"})
    assert ok.status_code == 200

    bad = client.post("/api/v1/auth/login", json={"username": "login_test", "password": "WRONG"})
    assert bad.status_code == 401


def test_save_roundtrip_and_anti_rollback(client):
    reg = client.post("/api/v1/auth/register", json={"username": "saver", "password": "pw123456"})
    token = reg.json()["token"]
    headers = {"Authorization": f"Bearer {token}"}

    payload = {"schemaVersion": 1, "playerLevel": 7, "exp": 350.0,
               "unlockedHeroIds": ["kael_flameblade", "sylra_frost"],
               "activeDifficulty": 1,
               "tower": [{"difficulty": 1, "bestFloor": 12}]}

    w = client.put("/api/v1/player/save", json={"slot": 0, "payload": payload}, headers=headers)
    assert w.status_code == 200

    r = client.get("/api/v1/player/save?slot=0", headers=headers)
    assert r.status_code == 200
    body = r.json()
    assert body["payload"]["playerLevel"] == 7
    assert body["payload"]["unlockedHeroIds"] == ["kael_flameblade", "sylra_frost"]

    # anti-rollback: lower level must be rejected
    payload["playerLevel"] = 3
    rb = client.put("/api/v1/player/save", json={"slot": 0, "payload": payload}, headers=headers)
    assert rb.status_code == 409


def test_unauthenticated_save_rejected(client):
    r = client.get("/api/v1/player/save")
    assert r.status_code == 401
