package config

import (
	"github.com/joho/godotenv"
	"github.com/spf13/viper"
)

type Config struct {
	ServerPort  string
	DatabaseURL string
	RedisURL    string
	JWTSecret   string
}

func Load() (*Config, error) {
	godotenv.Load()

	viper.SetDefault("SERVER_PORT", "8080")
	viper.AutomaticEnv()

	return &Config{
		ServerPort:  viper.GetString("SERVER_PORT"),
		DatabaseURL: viper.GetString("DATABASE_URL"),
		RedisURL:    viper.GetString("REDIS_URL"),
		JWTSecret:   viper.GetString("JWT_SECRET"),
	}, nil
}