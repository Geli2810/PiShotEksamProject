using System.Text.Json.Serialization;

namespace PiShotWebApi.DTO
{
    public class ScoreRequest
    {
        [JsonPropertyName("profileId")] public int ProfileId { get; set; }
    }
}
