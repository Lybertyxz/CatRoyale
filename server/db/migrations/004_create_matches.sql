-- +goose Up
CREATE TABLE matches (
    id              TEXT PRIMARY KEY,
    player1_id      TEXT NOT NULL REFERENCES users(id),
    player2_id      TEXT NOT NULL REFERENCES users(id),
    winner_id       TEXT REFERENCES users(id),
    status          TEXT NOT NULL DEFAULT 'waiting',
    turn_number     INTEGER NOT NULL DEFAULT 1,
    turn_duration   INTEGER NOT NULL DEFAULT 60,
    board_state     JSONB,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    finished_at     TIMESTAMPTZ
);

CREATE TABLE match_actions (
    id          TEXT PRIMARY KEY,
    match_id    TEXT NOT NULL REFERENCES matches(id) ON DELETE CASCADE,
    player_id   TEXT NOT NULL REFERENCES users(id),
    turn_number INTEGER NOT NULL,
    action_type TEXT NOT NULL,
    piece_x     INTEGER NOT NULL,
    piece_y     INTEGER NOT NULL,
    target_x    INTEGER NOT NULL,
    target_y    INTEGER NOT NULL,
    ability_id  TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_matches_player1 ON matches(player1_id);
CREATE INDEX idx_matches_player2 ON matches(player2_id);
CREATE INDEX idx_match_actions_match_id ON match_actions(match_id);

-- +goose Down
DROP TABLE match_actions;
DROP TABLE matches;