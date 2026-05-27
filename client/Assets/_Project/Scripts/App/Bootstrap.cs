using UnityEngine;
using CatRoyale.Core;
using CatRoyale.UI;
using CatRoyale.Network;
using CatRoyale.Gameplay;
using CatRoyale.Data;

namespace CatRoyale.App
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private PieceVisualRegistry _pieceVisualRegistry;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await InitializeServices();
            LoadMenu();
        }

        private async System.Threading.Tasks.Task InitializeServices()
        {
            Debug.Log("[Bootstrap] Initializing services...");

            SceneLoader.Initialize();
            UIManager.Initialize();
            ApiService.Initialize("http://localhost:8080");
            NetworkService.Initialize("ws://localhost:8080/api/v1/ws");
            MainThreadDispatcher.Initialize();

            await AuthService.Initialize();

            var pieceRepo = new PieceRepository(ServiceLocator.Get<ApiService>(), _pieceVisualRegistry);
            await pieceRepo.InitAsync();
            ServiceLocator.Register(pieceRepo);

            GameManager.Instance.SetState(GameState.Boot);

            await System.Threading.Tasks.Task.Delay(100);
            Debug.Log("[Bootstrap] Services ready.");
        }

        private void LoadMenu()
        {
            GameManager.Instance.SetState(GameState.Menu);
            SceneLoader.Instance.LoadScene(menuSceneName);
        }
    }
}