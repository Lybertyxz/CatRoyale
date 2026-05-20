-- +goose Up
CREATE TABLE piece_templates (
    id              TEXT PRIMARY KEY,
    name            TEXT NOT NULL,
    role            TEXT NOT NULL,
    rarity          TEXT NOT NULL,
    slot_cost       INTEGER NOT NULL DEFAULT 1,
    max_hp          INTEGER NOT NULL,
    attack          INTEGER NOT NULL,
    armor           INTEGER NOT NULL DEFAULT 0,
    attack_range    INTEGER NOT NULL DEFAULT 1,
    move_range      INTEGER NOT NULL DEFAULT 1,
    can_jump        BOOLEAN NOT NULL DEFAULT FALSE,
    movement_type   TEXT NOT NULL DEFAULT 'linear',
    movement_custom JSONB,
    abilities       JSONB NOT NULL DEFAULT '[]',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE user_pieces (
    id          TEXT PRIMARY KEY,
    user_id     TEXT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    template_id TEXT NOT NULL REFERENCES piece_templates(id),
    level       INTEGER NOT NULL DEFAULT 1,
    obtained_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_user_pieces_user_id ON user_pieces(user_id);

-- +goose Down
DROP TABLE user_pieces;
DROP TABLE piece_templates;