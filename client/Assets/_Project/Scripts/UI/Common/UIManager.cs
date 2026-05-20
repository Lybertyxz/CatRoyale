using System.Collections.Generic;
using UnityEngine;
using CatRoyale.Core;

namespace CatRoyale.UI
{
    public class UIManager
    {
        private readonly Dictionary<string, GameObject> _views = new();
        private GameObject _currentView;

        public static void Initialize()
        {
            var instance = new UIManager();
            ServiceLocator.Register(instance);
        }

        public void RegisterView(string viewName, GameObject viewObject)
        {
            _views[viewName] = viewObject;
            viewObject.SetActive(false);
        }

        public void ShowView(string viewName)
        {
            if (_currentView != null)
                _currentView.SetActive(false);

            if (_views.TryGetValue(viewName, out var view))
            {
                view.SetActive(true);
                _currentView = view;
                Debug.Log($"[UIManager] Showing view: {viewName}");
            }
            else
            {
                Debug.LogError($"[UIManager] View not found: {viewName}");
            }
        }

        public void HideAll()
        {
            foreach (var view in _views.Values)
                view.SetActive(false);
            _currentView = null;
        }
    }

    public static class ViewNames
    {
        public const string Menu = "Menu";
        public const string Collection = "Collection";
        public const string DeckBuilder = "DeckBuilder";
        public const string Booster = "Booster";
        public const string Matchmaking = "Matchmaking";
    }
}