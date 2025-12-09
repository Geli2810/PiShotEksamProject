namespace PiShotProject.Models
{
    public class GameStatusResponse
    {
        public bool IsActive { get; set; }

        public int Player1Id { get; set; }
        public int Player2Id { get; set; }

        public int? CurrentWinnerId { get; set; }
        public string? WinnerName { get; set; }
        public string? WinnerImage { get; set; }
    }
}
