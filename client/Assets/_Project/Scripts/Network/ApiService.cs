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
        private const int MaxRetries = 3;
        private const float RetryDelay = 1f;

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
            _client.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrEmpty(token))
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        // ─── Auth ────────────────────────────────────────────
        public async Task<ApiResult<UserResponse>> Login(string firebaseToken)
            => await Post<UserResponse>("/api/v1/auth/login",
                JsonConvert.SerializeObject(new { token = firebaseToken }));

        // ─── Boosters ─────────────────────────────────────────
        public async Task<ApiResult<List<BoosterResponse>>> GetBoosters()
            => await Get<List<BoosterResponse>>("/api/v1/boosters");

        public async Task<ApiResult<OpenBoosterResponse>> OpenBooster(string boosterID)
            => await Post<OpenBoosterResponse>($"/api/v1/boosters/{boosterID}/open", "{}");

        // ─── Pieces ───────────────────────────────────────────
        public async Task<ApiResult<List<PieceResponse>>> GetPieces()
            => await Get<List<PieceResponse>>("/api/v1/pieces");

        public async Task<ApiResult<List<UserPieceResponse>>> GetUserPieces()
            => await Get<List<UserPieceResponse>>("/api/v1/user/pieces");

        // ─── Decks ────────────────────────────────────────────
        public async Task<ApiResult<List<DeckResponse>>> GetDecks()
            => await Get<List<DeckResponse>>("/api/v1/decks");

        public async Task<ApiResult<DeckResponse>> CreateDeck(string name)
            => await Post<DeckResponse>("/api/v1/decks",
                JsonConvert.SerializeObject(new { name }));

        public async Task<ApiResult<bool>> SaveDeck(string deckID, List<DeckEntryRequest> entries)
        {
            var result = await Put($"/api/v1/decks/{deckID}",
                JsonConvert.SerializeObject(new { entries }));
            return result.Success
                ? ApiResult<bool>.Ok(true)
                : ApiResult<bool>.Fail(result.Error, result.StatusCode);
        }

        public async Task<ApiResult<bool>> DeleteDeck(string deckID)
        {
            return await ExecuteWithRetry(async () =>
            {
                var response = await _client.DeleteAsync(_baseUrl + $"/api/v1/decks/{deckID}");
                if (response.IsSuccessStatusCode)
                    return ApiResult<bool>.Ok(true);
                var error = await response.Content.ReadAsStringAsync();
                return ApiResult<bool>.Fail(ParseError(error), (int)response.StatusCode);
            }, $"/api/v1/decks/{deckID}");
        }

        public async Task<ApiResult<DeckDetailResponse>> GetDeckDetail(string deckID)
            => await Get<DeckDetailResponse>($"/api/v1/decks/{deckID}");

        public async Task<ApiResult<bool>> SetActiveDeck(string deckID)
        {
            var result = await Put($"/api/v1/decks/{deckID}/active", "{}");
            return result.Success
                ? ApiResult<bool>.Ok(true)
                : ApiResult<bool>.Fail(result.Error, result.StatusCode);
        }

        // ─── HTTP Helpers ─────────────────────────────────────
        private async Task<ApiResult<T>> Get<T>(string endpoint)
        {
            return await ExecuteWithRetry(async () =>
            {
                var response = await _client.GetAsync(_baseUrl + endpoint);
                return await ParseResponse<T>(response);
            }, endpoint);
        }

        private async Task<ApiResult<T>> Post<T>(string endpoint, string body)
        {
            return await ExecuteWithRetry(async () =>
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_baseUrl + endpoint, content);
                return await ParseResponse<T>(response);
            }, endpoint);
        }

        private async Task<ApiResult<bool>> Put(string endpoint, string body)
        {
            return await ExecuteWithRetry(async () =>
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync(_baseUrl + endpoint, content);
                if (response.IsSuccessStatusCode)
                    return ApiResult<bool>.Ok(true, (int)response.StatusCode);
                var error = await response.Content.ReadAsStringAsync();
                return ApiResult<bool>.Fail(ParseError(error), (int)response.StatusCode);
            }, endpoint);
        }

        private async Task<ApiResult<T>> ParseResponse<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = ParseError(json);
                Debug.LogWarning($"[ApiService] {(int)response.StatusCode} — {error}");
                return ApiResult<T>.Fail(error, (int)response.StatusCode);
            }

            try
            {
                var data = JsonConvert.DeserializeObject<T>(json);
                return ApiResult<T>.Ok(data, (int)response.StatusCode);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiService] Deserialize error: {e.Message}\nJSON: {json}");
                return ApiResult<T>.Fail($"Parse error: {e.Message}");
            }
        }

        private async Task<ApiResult<T>> ExecuteWithRetry<T>(
            Func<Task<ApiResult<T>>> action, string endpoint)
        {
            int attempts = 0;
            while (attempts < MaxRetries)
            {
                try
                {
                    return await action();
                }
                catch (Exception e)
                {
                    attempts++;
                    if (attempts >= MaxRetries)
                    {
                        Debug.LogError($"[ApiService] {endpoint} failed after {MaxRetries} attempts: {e.Message}");
                        return ApiResult<T>.Fail($"Network error: {e.Message}");
                    }
                    Debug.LogWarning($"[ApiService] Retry {attempts}/{MaxRetries} for {endpoint}");
                    await Task.Delay(TimeSpan.FromSeconds(RetryDelay));
                }
            }
            return ApiResult<T>.Fail("Max retries exceeded");
        }

        private string ParseError(string json)
        {
            try
            {
                var err = JsonConvert.DeserializeObject<ErrorResponse>(json);
                return err?.Error ?? json;
            }
            catch { return json; }
        }
    }

    public class ErrorResponse
    {
        [JsonProperty("error")] public string Error { get; set; }
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
        [JsonProperty("move_range")] public int MoveRange { get; set; }
        [JsonProperty("can_jump")] public bool CanJump { get; set; }
        [JsonProperty("movement_type")] public string MovementType { get; set; }
        [JsonProperty("movement_custom")] public List<CustomPosition> MovementCustom { get; set; }
    }

    public class CustomPosition
    {
        [JsonProperty("x")] public int X { get; set; }
        [JsonProperty("y")] public int Y { get; set; }
    }

    // Pièces possédées par l'utilisateur (join user_pieces + piece_templates)
    public class UserPieceResponse
    {
        [JsonProperty("id")] public string ID { get; set; }           // user_piece id
        [JsonProperty("user_id")] public string UserID { get; set; }
        [JsonProperty("template_id")] public string TemplateID { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("role")] public string Role { get; set; }
        [JsonProperty("rarity")] public string Rarity { get; set; }
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

    public class DeckDetailResponse
    {
        [JsonProperty("deck")] public DeckResponse Deck { get; set; }
        [JsonProperty("entries")] public List<DeckEntryResponse> Entries { get; set; }
    }

    public class DeckEntryResponse
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("deck_id")] public string DeckID { get; set; }
        [JsonProperty("template_id")] public string TemplateID { get; set; }
        [JsonProperty("start_x")] public int StartX { get; set; }
        [JsonProperty("start_y")] public int StartY { get; set; }
    }
}