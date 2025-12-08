namespace PiShotProject.Models
{
    public class StartGameRequest
    {
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
    }
    public class EndGameRequest
    {
        public int WinnerId { get; set; }
    }
    public class ScoreRequest
    {
        public int ProfileId { get; set; }
    }
    public class CreateProfileRequest
    {
        public string Name { get; set; }
        public string? ProfileImage { get; set; }
    }
}