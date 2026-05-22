using System.Collections.Generic;
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

        private string _matchID;
        private string _localPlayerID;
        private string _opponentName;
        private bool _isMyTurn;
        private float _timeRemaining;
        private bool _gameOver;

        private NetworkService _network;

        private void Awake()
        {
            _skipTurnButton?.onClick.AddListener(OnSkipTurn);
            _backToMenuButton?.onClick.AddListener(OnBackToMenu);
            if (_resultPanel) _resultPanel.SetActive(false);
        }

        private void Start()
        {
            _network = ServiceLocator.Get<NetworkService>();
            _localPlayerID = ServiceLocator.Get<AuthService>()?.UserID;
            if (_network != null)
                _network.OnMessageReceived += OnMessageReceived;
            _boardView?.Initialize(_localPlayerID);
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

        public void StartMatch(string matchID, string opponentName, bool isMyTurn, int turnDuration)
        {
            _matchID = matchID;
            _opponentName = opponentName;
            _isMyTurn = isMyTurn;
            _timeRemaining = turnDuration;
            if (_opponentNameText) _opponentNameText.text = opponentName;
            UpdateTurnUI();
        }

        private void OnMessageReceived(string rawMessage)
        {
            var envelope = JsonConvert.DeserializeObject<Envelope>(rawMessage);
            if (envelope == null) return;

            switch (envelope.Type)
            {
                case "turn_result": HandleTurnResult(envelope.Payload); break;
                case "game_over": HandleGameOver(envelope.Payload); break;
            }
        }

        private void HandleTurnResult(string payload)
        {
            var state = JsonConvert.DeserializeObject<GameStatePayload>(payload);
            if (state == null) return;
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

        private async void OnSkipTurn()
        {
            if (!_isMyTurn || _gameOver) return;
            var action = new
            {
                type = "play_turn",
                payload = JsonConvert.SerializeObject(
                new GameAction { type = "skip", piece_pos = new Pos(), target_pos = new Pos() })
            };
            await _network?.SendAsync(JsonConvert.SerializeObject(action));
        }

        private void OnBackToMenu()
        {
            ServiceLocator.Get<SceneLoader>()?.LoadScene("Menu");
        }

        private void UpdateTurnUI()
        {
            if (_turnText) _turnText.text = _isMyTurn ? "Votre tour" : $"Tour de {_opponentName}";
            if (_skipTurnButton) _skipTurnButton.interactable = _isMyTurn;
        }
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

    public class GameAction
    {
        public string type;
        public Pos piece_pos;
        public Pos target_pos;
        public string ability_id;
    }

    public class Pos { public int x; public int y; }
}