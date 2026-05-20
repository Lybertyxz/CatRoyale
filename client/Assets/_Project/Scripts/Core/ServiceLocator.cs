using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatRoyale.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting service: {type.Name}");
            }
            _services[type] = service;
            Debug.Log($"[ServiceLocator] Registered: {type.Name}");
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            Debug.LogError($"[ServiceLocator] Service not found: {type.Name}");
            return null;
        }

        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}