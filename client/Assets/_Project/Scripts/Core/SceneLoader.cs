using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatRoyale.Core
{
    public class SceneLoader
    {
        public static SceneLoader Instance { get; private set; }

        public bool IsLoading { get; private set; }

        public static void Initialize()
        {
            Instance = new SceneLoader();
            ServiceLocator.Register(Instance);
        }

        public async Task LoadSceneAsync(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[SceneLoader] Already loading a scene.");
                return;
            }

            IsLoading = true;
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");

            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                await Task.Yield();
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            IsLoading = false;
            Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}