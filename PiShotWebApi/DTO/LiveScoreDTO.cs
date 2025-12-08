namespace PiShotWebApi.DTO
{
    public class LiveScoreDTO
    {
        public bool IsTiebreak { get; set; }
        public PlayerScoreDTO P1 { get; set; }
        public PlayerScoreDTO P2 { get; set; }
    }
}