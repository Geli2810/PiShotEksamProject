using System.Text.Json.Serialization;

namespace PiShotProject.DTO
{
    public class EndGameRequestDTO
    {
        [JsonPropertyName("winnerId")]
        public int WinnerId { get; set; }
    }
}
