using UnityEngine;

namespace CatRoyale.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            Debug.Log($"[GameManager] State: {CurrentState} -> {newState}");
            CurrentState = newState;
        }
    }

    public enum GameState
    {
        Boot,
        Menu,
        DeckBuilder,
        Matchmaking,
        InGame,
        PostGame
    }
}