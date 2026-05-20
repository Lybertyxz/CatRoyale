-- name: GetPieceTemplate :one
SELECT * FROM piece_templates
WHERE id = $1;

-- name: ListPieceTemplates :many
SELECT * FROM piece_templates;

-- name: CreatePieceTemplate :one
INSERT INTO piece_templates (
    id, name, role, rarity, slot_cost,
    max_hp, attack, armor, attack_range,
    move_range, can_jump, movement_type,
    movement_custom, abilities
)
VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14)
RETURNING *;

-- name: GetUserPieces :many
SELECT up.*, pt.name, pt.role, pt.rarity
FROM user_pieces up
JOIN piece_templates pt ON pt.id = up.template_id
WHERE up.user_id = $1;

-- name: AddPieceToUser :one
INSERT INTO user_pieces (id, user_id, template_id)
VALUES ($1, $2, $3)
RETURNING *;