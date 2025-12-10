namespace PiShotProject.DTO
{
    public class LiveScoreDTO
    {
        public bool IsTiebreak { get; set; }
        public PlayerScoreDTO P1 { get; set; } = new PlayerScoreDTO();
        public PlayerScoreDTO P2 { get; set; } = new PlayerScoreDTO();
    }
}