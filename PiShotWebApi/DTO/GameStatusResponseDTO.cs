namespace PiShotWebApi.DTO
{
    public class GameStatusResponseDTO
    {
        private readonly PiShotProject.Interfaces.IGameRepository _repository;

        public bool IsActive { get; set; }
        public int P1_Id { get; set; }
        public int P2_Id { get; set; }

        public int? CurrentWinnerId { get; set; }
        public string? WinnerName { get; set; }
        public string? WinnerImage { get; set; }

        public GameStatusResponseDTO GetCurrentStatus()
        {
            var game = _repository.GetState();

            return new GameStatusResponseDTO
            {
                IsActive = game?.IsActive ?? false,
                P1_Id = game?.Player1Id ?? 0,
                P2_Id = game?.Player2Id ?? 0,
                CurrentWinnerId = game?.CurrentWinnerId,
                WinnerName = game?.WinnerName,
            };
        }
    }
}
