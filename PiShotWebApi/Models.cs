using System.Text.Json.Serialization;

namespace PiShotWebApi.Models
{
    public class ProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProfileImage { get; set; } // New
        public int Goals { get; set; }
        public int Attempts { get; set; }
        public double Accuracy { get; set; }

        // New Stats
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinLossRatio { get; set; }
    }

    public class CreateProfileRequest
    {
        public string Name { get; set; }
        public string ProfileImage { get; set; } // Base64 string or URL
    }

    public class EndGameRequest
    {
        public int WinnerId { get; set; }
    }

    // (Keep StartGameRequest and ScoreRequest the same as before)
    public class StartGameRequest { public int Player1Id { get; set; } public int Player2Id { get; set; } }
    public class ScoreRequest { [JsonPropertyName("profileId")] public int ProfileId { get; set; } }
}
