using System.Text.Json.Serialization;

namespace PiShotProject.DTO
{
    public class ScoreRequest
    {
        [JsonPropertyName("profileId")] public int ProfileId { get; set; }
    }
}
