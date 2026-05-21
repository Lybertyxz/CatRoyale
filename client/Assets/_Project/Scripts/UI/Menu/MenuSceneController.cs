using UnityEngine;
using CatRoyale.Core;

namespace CatRoyale.UI.Menu
{
    public class MenuSceneController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private GameObject _loginView;
        [SerializeField] private GameObject _menuView;
        [SerializeField] private GameObject _collectionView;
        [SerializeField] private GameObject _deckBuilderView;
        [SerializeField] private GameObject _boosterView;

        private void Start()
        {
            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager == null) return;

            uiManager.RegisterView(ViewNames.Login, _loginView);
            uiManager.RegisterView(ViewNames.Menu, _menuView);
            uiManager.RegisterView(ViewNames.Collection, _collectionView);
            uiManager.RegisterView(ViewNames.DeckBuilder, _deckBuilderView);
            uiManager.RegisterView(ViewNames.Booster, _boosterView);

            // Affiche le login au démarrage
            uiManager.ShowView(ViewNames.Login);
        }
    }
}