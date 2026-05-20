-- name: CreateMatch :one
INSERT INTO matches (id, player1_id, player2_id, status, turn_duration)
VALUES ($1, $2, $3, $4, $5)
RETURNING *;

-- name: GetMatchByID :one
SELECT * FROM matches
WHERE id = $1;

-- name: UpdateMatchStatus :one
UPDATE matches
SET status = $2, updated_at = NOW()
WHERE id = $1
RETURNING *;

-- name: UpdateMatchState :one
UPDATE matches
SET board_state = $2, turn_number = $3, updated_at = NOW()
WHERE id = $1
RETURNING *;

-- name: FinishMatch :one
UPDATE matches
SET status = 'finished', winner_id = $2, finished_at = NOW(), updated_at = NOW()
WHERE id = $1
RETURNING *;

-- name: GetUserMatchHistory :many
SELECT * FROM matches
WHERE player1_id = $1 OR player2_id = $1
ORDER BY created_at DESC
LIMIT $2;

-- name: CreateMatchAction :one
INSERT INTO match_actions (id, match_id, player_id, turn_number, action_type, piece_x, piece_y, target_x, target_y, ability_id)
VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)
RETURNING *;