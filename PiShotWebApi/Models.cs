using System.Text.Json.Serialization;

namespace BasketballApi.Models
{
    // DTOs (Data Transfer Objects)
    public class ProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProfileImage { get; set; }
        public int Goals { get; set; }
        public int Attempts { get; set; }
        public double Accuracy { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinLossRatio { get; set; }
        public int Rank { get; set; }
    }

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
        public int VisualScore { get; set; } // Score shown on screen
        public int TotalScore { get; set; }  // Actual goals made
    }

    // Requests
    public class CreateProfileRequest
    {
        public string Name { get; set; }
        public string ProfileImage { get; set; }
    }

    public class StartGameRequest
    {
        [JsonPropertyName("player1Id")] public int Player1Id { get; set; }
        [JsonPropertyName("player2Id")] public int Player2Id { get; set; }
    }

    public class EndGameRequest
    {
        public int WinnerId { get; set; }
    }

    public class ScoreRequest
    {
        [JsonPropertyName("profileId")] public int ProfileId { get; set; }
    }
}