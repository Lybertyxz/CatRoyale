using UnityEngine;

namespace CatRoyale.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = "Menu";

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

            // Initialise les services core
            SceneLoader.Initialize();
            GameManager.Instance.SetState(GameState.Boot);

            // TODO: AuthService.Initialize()
            // TODO: NetworkService.Initialize()

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