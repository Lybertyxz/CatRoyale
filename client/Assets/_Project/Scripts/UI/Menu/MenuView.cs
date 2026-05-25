using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.Network;

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

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _collectionButton.onClick.AddListener(OnCollectionClicked);
            _deckBuilderButton.onClick.AddListener(OnDeckBuilderClicked);
            _shopButton.onClick.AddListener(OnShopClicked);
        }

        private async void OnPlayClicked()
        {
            Debug.Log("[MenuView] Play clicked");
            GameManager.Instance.SetState(GameState.Matchmaking);

            var network = ServiceLocator.Get<NetworkService>();
            if (network == null)
            {
                Debug.LogError("[MenuView] NetworkService not found.");
                return;
            }

            // Connecte au WebSocket avec le token Firebase
            var auth = ServiceLocator.Get<AuthService>();
            if (auth == null || !auth.IsLoggedIn)
            {
                Debug.LogWarning("[MenuView] Not logged in — using test connection.");
                await network.ConnectAsync("test_token");
            }
            else
            {
                var token = await auth.GetFirebaseToken();
                await network.ConnectAsync(token);
            }

            // Envoie join_queue
            var message = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                type = "join_queue",
                payload = "{}"
            });
            await network.SendAsync(message);
            Debug.Log("[MenuView] Joined matchmaking queue.");
        }

        private void OnCollectionClicked()
        {
            Debug.Log("[MenuView] Collection clicked");
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Collection);
        }

        private void OnDeckBuilderClicked()
        {
            Debug.Log("[MenuView] DeckBuilder clicked");
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.DeckBuilder);
        }

        private void OnShopClicked()
        {
            Debug.Log("[MenuView] Shop clicked");
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