using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CatRoyale.Network;

namespace CatRoyale.Data
{
    public class PieceRepository
    {
        private const string CacheFileName = "pieces_cache.json";
        private const float TTLHours = 1f;

        private readonly ApiService _api;
        private readonly PieceVisualRegistry _visuals;
        private List<PieceModel> _pieces;

        private string CachePath => Path.Combine(Application.persistentDataPath, CacheFileName);

        public PieceRepository(ApiService api, PieceVisualRegistry visuals)
        {
            _api = api;
            _visuals = visuals;
        }

        // ─── Public API ───────────────────────────────────────

        public List<PieceModel> GetAll() => _pieces ?? new();
        public List<PieceModel> GetOwned() => _pieces?.FindAll(p => p.IsOwned) ?? new();
        public PieceModel Get(string id) => _pieces?.Find(p => p.ID == id);

        public async Awaitable InitAsync()
        {
            var cache = LoadCache();

            if (cache != null && !IsCacheExpired(cache.FetchedAt))
            {
                Debug.Log("[PieceRepository] Using disk cache.");
                _pieces = Merge(cache.Pieces);
                await ApplyOwnership();
                return;
            }

            Debug.Log("[PieceRepository] Fetching from server...");
            var result = await _api.GetPieces();

            if (!result.Success)
            {
                Debug.LogWarning($"[PieceRepository] Fetch failed: {result.Error}");
                if (cache != null) _pieces = Merge(cache.Pieces);
                return;
            }

            SaveCache(result.Data);
            _pieces = Merge(result.Data);
            await ApplyOwnership();
            Debug.Log($"[PieceRepository] {_pieces.Count} pieces loaded.");
        }

        // ─── Ownership ────────────────────────────────────────

        private async Awaitable ApplyOwnership()
        {
            var result = await _api.GetUserPieces();

            if (!result.Success)
            {
                Debug.LogWarning($"[PieceRepository] GetUserPieces failed: {result.Error}");
                return;
            }

            // Indexe les template_ids possédés
            var ownedIDs = new HashSet<string>();
            foreach (var up in result.Data)
                ownedIDs.Add(up.TemplateID);

            // Met à jour IsOwned sur les modèles
            foreach (var piece in _pieces)
                piece.IsOwned = ownedIDs.Contains(piece.ID);

            var ownedCount = ownedIDs.Count;
            Debug.Log($"[PieceRepository] {ownedCount}/{_pieces.Count} pieces owned.");
        }

        // ─── Merge stats + visuels ────────────────────────────

        private List<PieceModel> Merge(List<PieceResponse> dtos)
        {
            var models = new List<PieceModel>();
            foreach (var dto in dtos)
            {
                models.Add(new PieceModel
                {
                    ID = dto.ID,
                    Name = dto.Name,
                    Role = dto.Role,
                    Rarity = dto.Rarity,
                    SlotCost = dto.SlotCost,
                    MaxHP = dto.MaxHP,
                    Attack = dto.Attack,
                    Armor = dto.Armor,
                    AttackRange = dto.AttackRange,
                    MoveRange = dto.MoveRange,
                    CanJump = dto.CanJump,
                    MovementType = dto.MovementType,
                    Icon = _visuals.GetIcon(dto.ID),
                    IsOwned = false
                });
            }
            return models;
        }

        // ─── Cache disque ─────────────────────────────────────

        private void SaveCache(List<PieceResponse> pieces)
        {
            try
            {
                var cache = new PieceCache { FetchedAt = DateTime.UtcNow, Pieces = pieces };
                File.WriteAllText(CachePath, JsonConvert.SerializeObject(cache));
            }
            catch (Exception e) { Debug.LogWarning($"[PieceRepository] Cache save failed: {e.Message}"); }
        }

        private PieceCache LoadCache()
        {
            try
            {
                if (!File.Exists(CachePath)) return null;
                return JsonConvert.DeserializeObject<PieceCache>(File.ReadAllText(CachePath));
            }
            catch { return null; }
        }

        private bool IsCacheExpired(DateTime fetchedAt) =>
            (DateTime.UtcNow - fetchedAt).TotalHours > TTLHours;

        [Serializable]
        private class PieceCache
        {
            public DateTime FetchedAt;
            public List<PieceResponse> Pieces;
        }
    }
}