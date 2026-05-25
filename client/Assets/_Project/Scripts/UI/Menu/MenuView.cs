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

        private NetworkService _network;

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _collectionButton.onClick.AddListener(OnCollectionClicked);
            _deckBuilderButton.onClick.AddListener(OnDeckBuilderClicked);
            _shopButton.onClick.AddListener(OnShopClicked);
        }

        private void Start()
        {
            _network = ServiceLocator.Get<NetworkService>();
            if (_network != null)
                _network.OnMessageReceived += OnNetworkMessage;
        }

        private void OnDestroy()
        {
            if (_network != null)
                _network.OnMessageReceived -= OnNetworkMessage;
        }

        private void OnNetworkMessage(string rawMessage)
        {
            var envelope = JsonConvert.DeserializeObject<Envelope>(rawMessage);
            if (envelope == null) return;

            if (envelope.Type == "game_start")
            {
                Debug.Log("[MenuView] Game start received — loading Game scene.");
                MainThreadDispatcher.Run(() =>
                {
                    GameManager.Instance.SetState(GameState.InGame);
                    ServiceLocator.Get<SceneLoader>()?.LoadScene("Game");
                });
            }
        }

        private async void OnPlayClicked()
        {
            Debug.Log("[MenuView] Play clicked");
            GameManager.Instance.SetState(GameState.Matchmaking);

            if (_network == null)
            {
                Debug.LogError("[MenuView] NetworkService not found.");
                return;
            }

            var auth = ServiceLocator.Get<AuthService>();
            if (auth == null || !auth.IsLoggedIn)
            {
                Debug.LogWarning("[MenuView] Not logged in — using test connection.");
                await _network.ConnectAsync("test_token");
            }
            else
            {
                var token = await auth.GetFirebaseToken();
                await _network.ConnectAsync(token);
            }

            var message = JsonConvert.SerializeObject(new
            {
                type = "join_queue",
                payload = "{}"
            });
            await _network.SendAsync(message);
            Debug.Log("[MenuView] Joined matchmaking queue.");
        }

        private void OnCollectionClicked()
        {
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Collection);
        }

        private void OnDeckBuilderClicked()
        {
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.DeckBuilder);
        }

        private void OnShopClicked()
        {
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Booster);
        }

        public void SetUserInfo(string username, int level, int coins, int gems)
        {
            if (_usernameText) _usernameText.text = username;
            if (_levelText) _levelText.text = $"Lvl {level}";
            if (_coinsText) _coinsText.text = coins.ToString();
            if (_gemsText) _gemsText.text = gems.ToString();
        }
    }
}