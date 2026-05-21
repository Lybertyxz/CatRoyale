using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using CatRoyale.Core;

namespace CatRoyale.Network
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private string _baseUrl;
        private string _authToken;

        public static void Initialize(string baseUrl)
        {
            var instance = new ApiService(baseUrl);
            ServiceLocator.Register(instance);
        }

        public ApiService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(10);
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        // ─── Auth ────────────────────────────────────────────
        public async Task<UserResponse> Login(string firebaseToken)
        {
            var body = JsonConvert.SerializeObject(new { token = firebaseToken });
            return await Post<UserResponse>("/api/v1/auth/login", body);
        }

        // ─── Boosters ─────────────────────────────────────────
        public async Task<List<BoosterResponse>> GetBoosters()
        {
            return await Get<List<BoosterResponse>>("/api/v1/boosters");
        }

        public async Task<OpenBoosterResponse> OpenBooster(string boosterID)
        {
            return await Post<OpenBoosterResponse>($"/api/v1/boosters/{boosterID}/open", "{}");
        }

        // ─── Pieces ───────────────────────────────────────────
        public async Task<List<PieceResponse>> GetPieces()
        {
            return await Get<List<PieceResponse>>("/api/v1/pieces");
        }

        // ─── Decks ────────────────────────────────────────────
        public async Task<List<DeckResponse>> GetDecks()
        {
            return await Get<List<DeckResponse>>("/api/v1/decks");
        }

        public async Task<DeckResponse> CreateDeck(string name)
        {
            var body = JsonConvert.SerializeObject(new { name });
            return await Post<DeckResponse>("/api/v1/decks", body);
        }

        public async Task SaveDeck(string deckID, List<DeckEntryRequest> entries)
        {
            var body = JsonConvert.SerializeObject(new { entries });
            await Put($"/api/v1/decks/{deckID}", body);
        }

        // ─── HTTP Helpers ─────────────────────────────────────
        private async Task<T> Get<T>(string endpoint)
        {
            try
            {
                var response = await _client.GetAsync(_baseUrl + endpoint);
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiService] GET {endpoint} failed: {e.Message}");
                return default;
            }
        }

        private async Task<T> Post<T>(string endpoint, string body)
        {
            try
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_baseUrl + endpoint, content);
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiService] POST {endpoint} failed: {e.Message}");
                return default;
            }
        }

        private async Task Put(string endpoint, string body)
        {
            try
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                await _client.PutAsync(_baseUrl + endpoint, content);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiService] PUT {endpoint} failed: {e.Message}");
            }
        }
    }

    // ─── Response Models ──────────────────────────────────────
    public class UserResponse
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("xp")] public int XP { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("rank")] public string Rank { get; set; }
        [JsonProperty("coins")] public int Coins { get; set; }
        [JsonProperty("gems")] public int Gems { get; set; }
    }

    public class BoosterResponse
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("price_coins")] public int PriceCoins { get; set; }
        [JsonProperty("price_gems")] public int PriceGems { get; set; }
        [JsonProperty("pieces_count")] public int PiecesCount { get; set; }
    }

    public class OpenBoosterResponse
    {
        [JsonProperty("pieces")] public List<PieceResponse> Pieces { get; set; }
    }

    public class PieceResponse
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("role")] public string Role { get; set; }
        [JsonProperty("rarity")] public string Rarity { get; set; }
        [JsonProperty("slot_cost")] public int SlotCost { get; set; }
        [JsonProperty("max_hp")] public int MaxHP { get; set; }
        [JsonProperty("attack")] public int Attack { get; set; }
        [JsonProperty("armor")] public int Armor { get; set; }
        [JsonProperty("attack_range")] public int AttackRange { get; set; }
    }

    public class DeckResponse
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("is_active")] public bool IsActive { get; set; }
        [JsonProperty("total_slots")] public int TotalSlots { get; set; }
    }

    public class DeckEntryRequest
    {
        [JsonProperty("template_id")] public string TemplateID { get; set; }
        [JsonProperty("start_x")] public int StartX { get; set; }
        [JsonProperty("start_y")] public int StartY { get; set; }
    }
}