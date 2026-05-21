using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;

namespace CatRoyale.UI.Login
{
    public class LoginView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _googleLoginButton;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private GameObject _loadingIndicator;

        private void Awake()
        {
            _googleLoginButton?.onClick.AddListener(OnGoogleLoginClicked);
        }

        private void Start()
        {
            SetStatus("Bienvenue sur Cat Royale !");
        }

        private async void OnGoogleLoginClicked()
        {
            SetLoading(true);
            SetStatus("Connexion en cours...");

            var auth = ServiceLocator.Get<AuthService>();
            if (auth == null)
            {
                SetStatus("Erreur : AuthService non initialisé.");
                SetLoading(false);
                return;
            }

            // Sur mobile on utilise Google Sign-In plugin
            // Pour l'instant on simule avec un token de test
            bool success = await auth.RefreshAndRegister();

            if (success)
            {
                SetStatus("Connecté !");
                SetLoading(false);
                ServiceLocator.Get<UIManager>().ShowView(ViewNames.Menu);
                GameManager.Instance.SetState(GameState.Menu);
            }
            else
            {
                SetStatus("Échec de la connexion. Réessayez.");
                SetLoading(false);
            }
        }

        private void SetStatus(string message)
        {
            if (_statusText) _statusText.text = message;
        }

        private void SetLoading(bool loading)
        {
            if (_loadingIndicator) _loadingIndicator.SetActive(loading);
            if (_googleLoginButton) _googleLoginButton.interactable = !loading;
        }
    }
}