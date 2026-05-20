-- name: GetDecksByUser :many
SELECT * FROM decks
WHERE user_id = $1
ORDER BY created_at DESC;

-- name: GetActiveDeck :one
SELECT * FROM decks
WHERE user_id = $1 AND is_active = TRUE
LIMIT 1;

-- name: GetDeckByID :one
SELECT * FROM decks
WHERE id = $1;

-- name: CreateDeck :one
INSERT INTO decks (id, user_id, name)
VALUES ($1, $2, $3)
RETURNING *;

-- name: UpdateDeckActive :one
UPDATE decks
SET is_active = $2, updated_at = NOW()
WHERE id = $1
RETURNING *;

-- name: DeleteDeck :exec
DELETE FROM decks
WHERE id = $1;

-- name: GetDeckEntries :many
SELECT * FROM deck_entries
WHERE deck_id = $1;

-- name: AddDeckEntry :one
INSERT INTO deck_entries (id, deck_id, template_id, start_x, start_y)
VALUES ($1, $2, $3, $4, $5)
RETURNING *;

-- name: DeleteDeckEntries :exec
DELETE FROM deck_entries
WHERE deck_id = $1;