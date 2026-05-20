-- +goose Up
CREATE TABLE users (
    id          TEXT PRIMARY KEY,
    username    TEXT NOT NULL,
    email       TEXT NOT NULL UNIQUE,
    firebase_uid TEXT NOT NULL UNIQUE,
    xp          INTEGER NOT NULL DEFAULT 0,
    level       INTEGER NOT NULL DEFAULT 1,
    rank        TEXT NOT NULL DEFAULT 'bronze',
    coins       INTEGER NOT NULL DEFAULT 0,
    gems        INTEGER NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_users_firebase_uid ON users(firebase_uid);

-- +goose Down
DROP TABLE users;