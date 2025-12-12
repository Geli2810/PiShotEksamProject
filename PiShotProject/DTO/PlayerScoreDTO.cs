using System.Text.Json.Serialization;

public class PlayerScoreDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("profileImage")]
    public string ProfileImage { get; set; }
    [JsonPropertyName("visualScore")]
    public int VisualScore { get; set; }
    [JsonPropertyName("totalScore")]
    public int TotalScore { get; set; }
    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }
}