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

            // BYPASS TEMPORAIRE pour test — à supprimer en production
            await System.Threading.Tasks.Task.Delay(500);
            SetStatus("Connecté !");
            SetLoading(false);
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Menu);
            GameManager.Instance.SetState(GameState.Menu);
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