package config

import (
	"github.com/joho/godotenv"
	"github.com/spf13/viper"
)

type Config struct {
	ServerPort             string
	DatabaseURL            string
	RedisURL               string
	JWTSecret              string
	FirebaseServiceAccount string
	DevMode                bool
}

func Load() (*Config, error) {
	godotenv.Load()

	viper.SetDefault("SERVER_PORT", "8080")
	viper.SetDefault("FIREBASE_SERVICE_ACCOUNT", "firebase-service-account.json")
	viper.AutomaticEnv()

	return &Config{
		ServerPort:             viper.GetString("SERVER_PORT"),
		DatabaseURL:            viper.GetString("DATABASE_URL"),
		RedisURL:               viper.GetString("REDIS_URL"),
		JWTSecret:              viper.GetString("JWT_SECRET"),
		FirebaseServiceAccount: viper.GetString("FIREBASE_SERVICE_ACCOUNT"),
		DevMode: viper.GetBool("DEV_MODE"),
	}, nil
}
