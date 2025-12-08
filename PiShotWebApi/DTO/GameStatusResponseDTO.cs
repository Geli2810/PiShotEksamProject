namespace PiShotWebApi.DTO
{
    public class GameStatusResponseDTO
    {
        public bool IsActive { get; set; }
        public int P1_Id { get; set; }
        public int P2_Id { get; set; }

        public int? CurrentWinnerId { get; set; }
        public string? WinnerName { get; set; }
        public string? WinnerImage { get; set; }
    }
}
