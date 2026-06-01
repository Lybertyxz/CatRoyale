using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.Network;
using Newtonsoft.Json;

namespace CatRoyale.UI.Menu
{
    public class MenuView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _collectionButton;
        [SerializeField] private Button _deckBuilderButton;
        [SerializeField] private Button _shopButton;

        [Header("User Info")]
        [SerializeField] private TextMeshProUGUI _usernameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _gemsText;

        [Header("Battle")]
        [SerializeField] private TMP_Dropdown _deckDropdown;

        private List<DeckResponse> _decks = new();
        private NetworkService _network;
        private ApiService _api;

        // ─── Unity Lifecycle ──────────────────────────────────

        private void Awake()
        {
            _playButton?.onClick.AddListener(OnPlayClicked);
            _collectionButton?.onClick.AddListener(OnCollectionClicked);
            _deckBuilderButton?.onClick.AddListener(OnDeckBuilderClicked);
            _shopButton?.onClick.AddListener(OnShopClicked);
        }

        private void Start()
        {
            _network = ServiceLocator.Get<NetworkService>();
            _api = ServiceLocator.Get<ApiService>();

            if (_network != null)
                _network.OnMessageReceived += OnNetworkMessage;

            if (_deckDropdown != null)
                _deckDropdown.onValueChanged.AddListener(OnDeckSelected);

            LoadDecks();
        }

        private void OnDestroy()
        {
            if (_network != null)
                _network.OnMessageReceived -= OnNetworkMessage;
        }

        // ─── Network ──────────────────────────────────────────

        private void OnNetworkMessage(string rawMessage)
        {
            var envelope = JsonConvert.DeserializeObject<Envelope>(rawMessage);
            if (envelope == null) return;

            if (envelope.Type == "game_start")
            {
                GameContext.PendingGameStartPayload = envelope.Payload?.ToString();
                MainThreadDispatcher.Run(() =>
                {
                    GameManager.Instance.SetState(GameState.InGame);
                    ServiceLocator.Get<SceneLoader>()?.LoadScene("Game");
                });
            }
        }

        // ─── Button Handlers ──────────────────────────────────

        private async void OnPlayClicked()
        {
            if (_decks.Count == 0 || _deckDropdown == null)
            {
                Debug.LogWarning("[MenuView] No deck available.");
                return;
            }

            var selected = _decks[_deckDropdown.value];
            GameContext.SelectedDeckID = selected.ID;
            GameContext.SelectedDeckName = selected.Name;

            if (_network == null)
            {
                Debug.LogError("[MenuView] NetworkService not found.");
                return;
            }

            var auth = ServiceLocator.Get<AuthService>();
            if (auth == null || !auth.IsLoggedIn)
                await _network.ConnectAsync("test_token");
            else
                await _network.ConnectAsync(await auth.GetFirebaseToken());

            var message = JsonConvert.SerializeObject(new
            {
                type = "join_queue",
                payload = new { deck_id = GameContext.SelectedDeckID }
            });

            await _network.SendAsync(message);

            GameManager.Instance.SetState(GameState.Matchmaking);
            Debug.Log($"[MenuView] Joined queue with deck: {selected.Name} ({selected.ID})");
        }

        private void OnCollectionClicked() => ServiceLocator.Get<UIManager>().ShowView(ViewNames.Collection);
        private void OnDeckBuilderClicked() => ServiceLocator.Get<UIManager>().ShowView(ViewNames.DeckBuilder);
        private void OnShopClicked() => ServiceLocator.Get<UIManager>().ShowView(ViewNames.Booster);

        // ─── Deck Selection ───────────────────────────────────

        private async void OnDeckSelected(int index)
        {
            if (_decks.Count == 0 || index >= _decks.Count) return;

            var selected = _decks[index];
            var result = await _api.SetActiveDeck(selected.ID);

            if (result.Success)
                Debug.Log($"[MenuView] Active deck set: {selected.Name}");
            else
                Debug.LogWarning($"[MenuView] SetActiveDeck failed: {result.Error}");
        }

        // ─── UI ───────────────────────────────────────────────

        public void SetUserInfo(string username, int level, int coins, int gems)
        {
            if (_usernameText) _usernameText.text = username;
            if (_levelText) _levelText.text = $"Lvl {level}";
            if (_coinsText) _coinsText.text = coins.ToString();
            if (_gemsText) _gemsText.text = gems.ToString();
        }

        private async void LoadDecks()
        {
            var result = await _api.GetDecks();
            if (!result.Success || result.Data == null) return;

            _decks = result.Data;

            _deckDropdown?.ClearOptions();
            _deckDropdown?.AddOptions(_decks.ConvertAll(d => new TMP_Dropdown.OptionData(d.Name)));

            // Sélectionne le deck actif par défaut sans déclencher OnDeckSelected
            var activeIndex = _decks.FindIndex(d => d.IsActive);
            if (activeIndex >= 0 && _deckDropdown != null)
                _deckDropdown.SetValueWithoutNotify(activeIndex);
        }
    }
}