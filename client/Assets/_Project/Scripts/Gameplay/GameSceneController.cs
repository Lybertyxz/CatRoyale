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
        [SerializeField] private TextMeshProUGUI _timerP0Text;
        [SerializeField] private TextMeshProUGUI _timerP1Text;
        [SerializeField] private TextMeshProUGUI _opponentNameText;
        [SerializeField] private TextMeshProUGUI _paText;
        [SerializeField] private TextMeshProUGUI _pmText;
        [SerializeField] private Button _skipTurnButton;

        [Header("Result")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _backToMenuButton;

        // ─── State ────────────────────────────────────────────
        private string _matchID;
        private string _localPlayerID;
        private string _opponentName;
        private int _playerIndex;
        private bool _isMyTurn;
        private bool _gameOver;
        private float _timeBankP0;
        private float _timeBankP1;
        private int _remainingPA;
        private int _remainingPM;
        private bool _timeBankExpired = false;

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

            if (_network != null)
                _network.OnMessageReceived += OnMessageReceived;

            _boardView?.Initialize("", 0);

            if (_boardView != null)
                _boardView.OnActionRequested += OnBoardAction;

            // Puis rejoue game_start qui mettra à jour localPlayerID et playerIndex
            if (!string.IsNullOrEmpty(GameContext.PendingGameStartPayload))
            {
                HandleGameStart(GameContext.PendingGameStartPayload);
                GameContext.PendingGameStartPayload = null;
            }
        }

        private void OnDestroy()
        {
            if (_network != null)
                _network.OnMessageReceived -= OnMessageReceived;
        }

        private void Update()
        {
            if (_gameOver || _timeBankExpired) return;
            if (_isMyTurn)
            {
                _timeBankP0 = Mathf.Max(0, _timeBankP0 - Time.deltaTime);
                if (_timeBankP0 <= 0)
                {
                    _timeBankExpired = true;
                    OnSkipTurn();
                }
            }
            else
                _timeBankP1 = Mathf.Max(0, _timeBankP1 - Time.deltaTime);

            UpdateTimerUI();
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
                case "opponent_disconnected":
                    MainThreadDispatcher.Run(() => HandleOpponentDisconnected());
                    break;
                case "error":
                    var err = JsonConvert.DeserializeObject<ErrorPayload>(envelope.Payload?.ToString() ?? "{}");
                    Debug.LogWarning($"[GameSceneController] Server error: {err?.Message}");
                    break;
            }
        }

        // ─── Handlers ─────────────────────────────────────────

        private void HandleGameStart(string payload)
        {
            var data = JsonConvert.DeserializeObject<GameStartPayload>(payload);
            if (data == null) return;

            _matchID = data.MatchID;
            _localPlayerID = data.PlayerID;
            _opponentName = data.Opponent;
            _isMyTurn = data.YourTurn;
            _playerIndex = data.PlayerIndex;
            _timeBankP0 = data.TimeBankSeconds;
            _timeBankP1 = data.TimeBankSeconds;
            _remainingPA = 1;
            _remainingPM = 1;

            _boardView?.Initialize(_localPlayerID, _playerIndex);

            if (_opponentNameText) _opponentNameText.text = _opponentName;
            UpdateTurnUI();
            UpdateTimerUI();
            UpdateActionUI();
        }

        private void HandleGameReady(string payload)
        {
            var data = JsonConvert.DeserializeObject<GameReadyPayload>(payload);
            if (data?.State == null) return;
            Debug.Log($"[GameSceneController] HandleGameReady — localPlayerID: {_localPlayerID} | playerIndex: {_playerIndex} | boardInitialized: {_boardView?.IsInitialized}");
            if (_boardView != null && !_boardView.IsInitialized)
                _boardView.Initialize(_localPlayerID);

            ApplyState(data.State);
        }

        private void HandleTurnResult(string payload)
        {
            var state = JsonConvert.DeserializeObject<GameStatePayload>(payload);
            if (state == null) return;

            ApplyState(state);
        }

        private void ApplyState(GameStatePayload state)
        {
            _isMyTurn = state.CurrentPlayer == _localPlayerID;
            _timeBankP0 = state.TimeBankP0;
            _timeBankP1 = state.TimeBankP1;
            _remainingPA = state.RemainingPA;
            _remainingPM = state.RemainingPM;
            _timeBankExpired = false;

            _boardView?.UpdateBoard(state.Pieces);
            UpdateTurnUI();
            UpdateTimerUI();
            UpdateActionUI();
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

        private void HandleOpponentDisconnected()
        {
            _gameOver = true;
            if (_resultPanel) _resultPanel.SetActive(true);
            if (_resultText) _resultText.text = "VICTOIRE ! (adversaire déconnecté)";
        }

        // ─── Actions ──────────────────────────────────────────

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
            SendAction("move", fromX, fromY, toX, toY);
        }

        public void OnAbilityRequested(int pieceX, int pieceY, int targetX, int targetY, string abilityID)
        {
            SendAction("ability", pieceX, pieceY, targetX, targetY, abilityID);
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

        private void UpdateTimerUI()
        {
            if (_timerP0Text) _timerP0Text.text = FormatTime(_timeBankP0);
            if (_timerP1Text) _timerP1Text.text = FormatTime(_timeBankP1);
        }

        private void UpdateActionUI()
        {
            if (_paText) _paText.text = $"PA: {_remainingPA}/1";
            if (_pmText) _pmText.text = $"PM: {_remainingPM}/1";
        }

        private string FormatTime(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60);
            int s = Mathf.FloorToInt(seconds % 60);
            return $"{m:00}:{s:00}";
        }
    }

    // ─── Payloads ─────────────────────────────────────────────

    public class GameStartPayload
    {
        [JsonProperty("player_id")] public string PlayerID { get; set; }
        [JsonProperty("match_id")] public string MatchID { get; set; }
        [JsonProperty("opponent")] public string Opponent { get; set; }
        [JsonProperty("your_turn")] public bool YourTurn { get; set; }
        [JsonProperty("turn_duration")] public float TimeBankSeconds { get; set; }
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
        [JsonProperty("time_bank_p0")] public float TimeBankP0 { get; set; }
        [JsonProperty("time_bank_p1")] public float TimeBankP1 { get; set; }
        [JsonProperty("remaining_pa")] public int RemainingPA { get; set; }
        [JsonProperty("remaining_pm")] public int RemainingPM { get; set; }
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