using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using CatRoyale.Core;

namespace CatRoyale.Network
{
    public class NetworkService
    {
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        private string _serverUrl;

        public bool IsConnected => _socket?.State == WebSocketState.Open;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;

        public static void Initialize(string serverUrl)
        {
            var instance = new NetworkService(serverUrl);
            ServiceLocator.Register(instance);
        }

        public NetworkService(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        public async Task ConnectAsync(string firebaseToken)
        {
            try
            {
                _socket = new ClientWebSocket();
                _cts = new CancellationTokenSource();

                var uri = new Uri($"{_serverUrl}?token={firebaseToken}");
                await _socket.ConnectAsync(uri, _cts.Token);

                Debug.Log("[NetworkService] Connected to server.");
                OnConnected?.Invoke();

                _ = ReceiveLoopAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Connection failed: {e.Message}");
            }
        }

        public async Task SendAsync(string message)
        {
            if (!IsConnected) return;

            var bytes = Encoding.UTF8.GetBytes(message);
            await _socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                _cts.Token
            );
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];

            try
            {
                while (IsConnected)
                {
                    var result = await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cts.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[NetworkService] Receive error: {e.Message}");
                OnDisconnected?.Invoke();
            }
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;

            _cts.Cancel();
            await _socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Client disconnecting",
                CancellationToken.None
            );

            Debug.Log("[NetworkService] Disconnected.");
            OnDisconnected?.Invoke();
        }
    }
}