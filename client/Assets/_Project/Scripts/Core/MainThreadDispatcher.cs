using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatRoyale.Core
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private readonly Queue<Action> _actions = new();

        public static void Initialize()
        {
            if (_instance != null) return;
            var go = new GameObject("MainThreadDispatcher");
            _instance = go.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        public static void Run(Action action)
        {
            if (_instance == null) return;
            lock (_instance._actions)
                _instance._actions.Enqueue(action);
        }

        private void Update()
        {
            lock (_actions)
            {
                while (_actions.Count > 0)
                    _actions.Dequeue()?.Invoke();
            }
        }
    }
}