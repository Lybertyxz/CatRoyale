using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.Network;
using Newtonsoft.Json;

namespace CatRoyale.Gameplay
{
    public class GameSceneController : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private BoardView _boardView;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _opponentNameText;
        [SerializeField] private Button _skipTurnButton;

        [Header("Result")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _backToMenuButton;

        // ─── State ────────────────────────────────────────────
        private string _matchID;
        private string _localPlayerID;
        private string _opponentName;
        private int _playerIndex;   // 0 = haut du board, 1 = bas du board
        private bool _isMyTurn;
        private float _timeRemaining;
        private bool _gameOver;
        private string _pendingDeckID;

        private NetworkService _network;
        private ApiService _api;

        // ─── Unity Lifecycle ──────────────────────────────────

        private void Awake()
        {
            _skipTurnButton?.onClick.AddListener(OnSkipTurn);
            _backToMenuButton?.onClick.AddListener(OnBackToMenu);
            if (_resultPanel) _resultPanel.SetActive(false);
        }

        private void Start()
        {
            _network = ServiceLocator.Get<NetworkService>();
            _api = ServiceLocator.Get<ApiService>();
            _pendingDeckID = GameContext.SelectedDeckID;

            if (_network != null)
                _network.OnMessageReceived += OnMessageReceived;

            // Rejoue le game_start si reçu avant le chargement de la scène
            if (!string.IsNullOrEmpty(GameContext.PendingGameStartPayload))
            {
                HandleGameStart(GameContext.PendingGameStartPayload);
                GameContext.PendingGameStartPayload = null;
            }

            _boardView?.Initialize(_localPlayerID);

            if (_boardView != null)
                _boardView.OnActionRequested += OnBoardAction;

        }

        private void OnDestroy()
        {
            if (_network != null)
                _network.OnMessageReceived -= OnMessageReceived;
        }

        private void Update()
        {
            if (_gameOver || !_isMyTurn) return;
            _timeRemaining -= Time.deltaTime;
            if (_timerText) _timerText.text = Mathf.CeilToInt(_timeRemaining).ToString();
            if (_timeRemaining <= 0) OnSkipTurn();
        }

        // ─── WebSocket Messages ───────────────────────────────

        private void OnMessageReceived(string rawMessage)
        {
            var envelope = JsonConvert.DeserializeObject<Envelope>(rawMessage);
            if (envelope == null) return;

            switch (envelope.Type)
            {
                case "game_start":
                    MainThreadDispatcher.Run(() => HandleGameStart(envelope.Payload?.ToString() ?? "{}"));
                    break;
                case "deck_ready":
                    // Accusé de réception du deck — rien à faire
                    break;
                case "game_ready":
                    MainThreadDispatcher.Run(() => HandleGameReady(envelope.Payload?.ToString() ?? "{}"));
                    break;
                case "turn_result":
                    MainThreadDispatcher.Run(() => HandleTurnResult(envelope.Payload?.ToString() ?? "{}"));
                    break;
                case "game_over":
                    MainThreadDispatcher.Run(() => HandleGameOver(envelope.Payload?.ToString() ?? "{}"));
                    break;
                case "error":
                    var err = JsonConvert.DeserializeObject<ErrorPayload>(envelope.Payload?.ToString() ?? "{}");
                    Debug.LogWarning($"[GameSceneController] Server error: {err?.Message}");
                    break;
            }
        }

        // ─── Handlers ─────────────────────────────────────────

        private async void HandleGameStart(string payload)
        {
            var data = JsonConvert.DeserializeObject<GameStartPayload>(payload);
            if (data == null) return;

            _matchID = data.MatchID;
            _localPlayerID = data.PlayerID;
            _opponentName = data.Opponent;
            _isMyTurn = data.YourTurn;
            _timeRemaining = data.TurnDuration;
            _playerIndex = data.PlayerIndex;

            if (_opponentNameText) _opponentNameText.text = _opponentName;
            UpdateTurnUI();

            Debug.Log($"[GameSceneController] Game start — match: {_matchID} | playerIndex: {_playerIndex} | myTurn: {_isMyTurn}");

        }

        private void HandleGameReady(string payload)
        {
            var data = JsonConvert.DeserializeObject<GameReadyPayload>(payload);
            if (data?.State == null) return;

            Debug.Log("[GameSceneController] Game ready — updating board.");
            _boardView?.UpdateBoard(data.State.Pieces);
            UpdateTurnUI();
        }

        private void HandleTurnResult(string payload)
        {
            var state = JsonConvert.DeserializeObject<GameStatePayload>(payload);
            if (state == null) return;

            Debug.Log($"[GameSceneController] TurnResult — currentPlayer: {state.CurrentPlayer} | localID: {_localPlayerID} | isMyTurn: {_isMyTurn}");
            _isMyTurn = state.CurrentPlayer == _localPlayerID;
            _timeRemaining = state.TimeRemaining;
            _boardView?.UpdateBoard(state.Pieces);
            UpdateTurnUI();
        }

        private void HandleGameOver(string payload)
        {
            var result = JsonConvert.DeserializeObject<GameOverPayload>(payload);
            if (result == null) return;

            _gameOver = true;
            if (_resultPanel) _resultPanel.SetActive(true);
            if (_resultText)
                _resultText.text = result.WinnerID == _localPlayerID ? "VICTOIRE !" : "DÉFAITE";
        }

        // ─── Actions ──────────────────────────────────────────

        private async Task SubmitDeck()
        {
            if (string.IsNullOrEmpty(_pendingDeckID))
            {
                Debug.LogWarning("[GameSceneController] No deck selected.");
                return;
            }

            var result = await _api.GetDeckDetail(_pendingDeckID);
            if (!result.Success || result.Data?.Entries == null)
            {
                Debug.LogError($"[GameSceneController] Failed to load deck: {result.Error}");
                return;
            }

            var entries = result.Data.Entries.ConvertAll(e => new DeckEntryPayload
            {
                template_id = e.TemplateID,
                start_x = e.StartX,
                start_y = e.StartY
            });

            var message = JsonConvert.SerializeObject(new
            {
                type = "submit_deck",
                payload = new { match_id = _matchID, entries }
            });

            await _network.SendAsync(message);
            Debug.Log($"[GameSceneController] Deck submitted — {entries.Count} pieces, playerIndex: {_playerIndex}");
        }

        public void SendAction(string type, int pieceX, int pieceY, int targetX, int targetY, string abilityID = "")
        {
            if (!_isMyTurn || _gameOver) return;

            var message = JsonConvert.SerializeObject(new
            {
                type = "play_turn",
                payload = new
                {
                    type = type,
                    piece_pos = new { x = pieceX, y = pieceY },
                    target_pos = new { x = targetX, y = targetY },
                    ability_id = abilityID
                }
            });

            _ = _network.SendAsync(message);
        }

        private void OnBoardAction(int fromX, int fromY, int toX, int toY)
        {
            // Le serveur détermine si c'est un move ou attack selon la case cible
            SendAction("move", fromX, fromY, toX, toY);
        }

        private void OnSkipTurn()
        {
            if (!_isMyTurn || _gameOver) return;
            SendAction("skip", 0, 0, 0, 0);
        }

        private void OnBackToMenu()
        {
            ServiceLocator.Get<SceneLoader>()?.LoadScene("Menu");
        }

        // ─── UI ───────────────────────────────────────────────

        private void UpdateTurnUI()
        {
            if (_turnText) _turnText.text = _isMyTurn ? "Votre tour" : $"Tour de {_opponentName}";
            if (_skipTurnButton) _skipTurnButton.interactable = _isMyTurn && !_gameOver;
        }
    }

    // ─── Payloads ─────────────────────────────────────────────

    public class GameStartPayload
    {
        [JsonProperty("player_id")] public string PlayerID { get; set; }
        [JsonProperty("match_id")] public string MatchID { get; set; }
        [JsonProperty("opponent")] public string Opponent { get; set; }
        [JsonProperty("your_turn")] public bool YourTurn { get; set; }
        [JsonProperty("turn_duration")] public int TurnDuration { get; set; }
        [JsonProperty("player_index")] public int PlayerIndex { get; set; }
    }

    public class GameReadyPayload
    {
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("state")] public GameStatePayload State { get; set; }
        [JsonProperty("player_index")] public int PlayerIndex { get; set; }
    }

    public class GameStatePayload
    {
        [JsonProperty("current_player")] public string CurrentPlayer { get; set; }
        [JsonProperty("time_remaining")] public float TimeRemaining { get; set; }
        [JsonProperty("turn_number")] public int TurnNumber { get; set; }
        [JsonProperty("pieces")] public List<PieceStateData> Pieces { get; set; }
    }

    public class GameOverPayload
    {
        [JsonProperty("winner_id")] public string WinnerID { get; set; }
    }

    public class ErrorPayload
    {
        [JsonProperty("message")] public string Message { get; set; }
    }

    public class DeckEntryPayload
    {
        [JsonProperty("template_id")] public string template_id;
        [JsonProperty("start_x")] public int start_x;
        [JsonProperty("start_y")] public int start_y;
    }
}