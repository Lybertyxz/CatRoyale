-- +goose Up
CREATE TABLE decks (
    id          TEXT PRIMARY KEY,
    user_id     TEXT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name        TEXT NOT NULL DEFAULT 'My Deck',
    is_active   BOOLEAN NOT NULL DEFAULT FALSE,
    total_slots INTEGER NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE deck_entries (
    id          TEXT PRIMARY KEY,
    deck_id     TEXT NOT NULL REFERENCES decks(id) ON DELETE CASCADE,
    template_id TEXT NOT NULL REFERENCES piece_templates(id),
    start_x     INTEGER NOT NULL,
    start_y     INTEGER NOT NULL,
    UNIQUE(deck_id, start_x, start_y)
);

CREATE INDEX idx_decks_user_id ON decks(user_id);
CREATE INDEX idx_deck_entries_deck_id ON deck_entries(deck_id);

-- +goose Down
DROP TABLE deck_entries;
DROP TABLE decks;