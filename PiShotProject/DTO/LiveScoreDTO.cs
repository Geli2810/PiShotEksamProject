using System.Text.Json.Serialization;

namespace PiShotProject.DTO
{
    public class LiveScoreDTO
    {
        [JsonPropertyName("isTiebreak")]
        public bool IsTiebreak { get; set; }
        [JsonPropertyName("p1")]
        public PlayerScoreDTO P1 { get; set; } = new PlayerScoreDTO();
        [JsonPropertyName("p2")]
        public PlayerScoreDTO P2 { get; set; } = new PlayerScoreDTO();
    }
}