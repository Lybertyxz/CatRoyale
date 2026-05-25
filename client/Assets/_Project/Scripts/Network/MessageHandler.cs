using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace CatRoyale.Network
{
    public class MessageHandler
    {
        private readonly Dictionary<string, Action<string>> _handlers = new();

        public void Register(string messageType, Action<string> handler)
        {
            _handlers[messageType] = handler;
            Debug.Log($"[MessageHandler] Registered handler for: {messageType}");
        }

        public void Unregister(string messageType)
        {
            _handlers.Remove(messageType);
        }

        public void Handle(string rawMessage)
        {
            try
            {
                var envelope = JsonConvert.DeserializeObject<Envelope>(rawMessage);
                if (envelope == null) return;

                if (_handlers.TryGetValue(envelope.Type, out var handler))
                {
                    handler.Invoke(envelope.Payload?.ToString() ?? "{}");
                }
                else
                {
                    Debug.LogWarning($"[MessageHandler] No handler for: {envelope.Type}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MessageHandler] Parse error: {e.Message}");
            }
        }
    }

    [Serializable]
    public class Envelope
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("payload")]
        public Newtonsoft.Json.Linq.JToken Payload { get; set; }
    }
}