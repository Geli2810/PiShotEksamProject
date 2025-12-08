using System.Text.Json.Serialization;

namespace PiShotProject.Models
{
    public class LiveScoreDTO
    {
        public bool IsTiebreak { get; set; }
        public PlayerScoreDTO P1 { get; set; }
        public PlayerScoreDTO P2 { get; set; }
    }

    public class PlayerScoreDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProfileImage { get; set; }
        public int VisualScore { get; set; }
        public int TotalScore { get; set; }
    }

    public class ScoreRequest
    {
        [JsonPropertyName("profileId")] public int ProfileId { get; set; }
    }
}