using System.Text.Json.Serialization;

namespace PiShotProject.Models
{
    public class StartGameRequest
    {
        [JsonPropertyName("player1Id")]
        public int Player1Id { get; set; }

        [JsonPropertyName("player2Id")]
        public int Player2Id { get; set; }
    }
}
