using Newtonsoft.Json;

namespace CatRoyale.Gameplay
{
    public class PieceStateData
    {
        [JsonProperty("template_id")] public string TemplateID { get; set; }
        [JsonProperty("owner_id")] public string OwnerID { get; set; }
        [JsonProperty("current_hp")] public int CurrentHP { get; set; }
        [JsonProperty("max_hp")] public int MaxHP { get; set; }
        [JsonProperty("position")] public PiecePosition Position { get; set; }
        [JsonProperty("is_alive")] public bool IsAlive { get; set; }

        public int X => Position?.X ?? 0;
        public int Y => Position?.Y ?? 0;
    }

    public class PiecePosition
    {
        [JsonProperty("x")] public int X { get; set; }
        [JsonProperty("y")] public int Y { get; set; }
    }
}