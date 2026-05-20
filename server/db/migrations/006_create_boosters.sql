-- +goose Up
CREATE TABLE booster_types (
    id          TEXT PRIMARY KEY,
    name        TEXT NOT NULL,
    description TEXT,
    price_coins INTEGER NOT NULL DEFAULT 0,
    price_gems  INTEGER NOT NULL DEFAULT 0,
    pieces_count INTEGER NOT NULL DEFAULT 3,
    rarity_weights JSONB NOT NULL
);

CREATE TABLE booster_openings (
    id          TEXT PRIMARY KEY,
    user_id     TEXT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    booster_type_id TEXT NOT NULL REFERENCES booster_types(id),
    pieces_obtained JSONB NOT NULL,
    opened_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_booster_openings_user_id ON booster_openings(user_id);

-- +goose Down
DROP TABLE booster_openings;
DROP TABLE booster_types;