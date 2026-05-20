.PHONY: run dev build migrate-up migrate-down migrate-status docker-up docker-down

DB_URL=postgres://postgres:postgres@localhost:5432/catroyale?sslmode=disable

run:
	go run ./cmd/server

dev:
	air

build:
	go build -o bin/server.exe ./cmd/server

migrate-up:
	goose -dir db/migrations postgres "$(DB_URL)" up

migrate-down:
	goose -dir db/migrations postgres "$(DB_URL)" down

migrate-status:
	goose -dir db/migrations postgres "$(DB_URL)" status

docker-up:
	docker compose up -d

docker-down:
	docker compose down

test:
	go test ./...