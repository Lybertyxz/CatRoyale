-- name: GetUserByFirebaseUID :one
SELECT * FROM users
WHERE firebase_uid = $1;

-- name: GetUserByID :one
SELECT * FROM users
WHERE id = $1;

-- name: CreateUser :one
INSERT INTO users (id, username, email, firebase_uid)
VALUES ($1, $2, $3, $4)
RETURNING *;

-- name: UpdateUserXP :one
UPDATE users
SET xp = xp + $2, updated_at = NOW()
WHERE id = $1
RETURNING *;

-- name: UpdateUserCoins :one
UPDATE users
SET coins = coins + $2, updated_at = NOW()
WHERE id = $1
RETURNING *;

-- name: UpdateUserRank :one
UPDATE users
SET rank = $2, updated_at = NOW()
WHERE id = $1
RETURNING *;