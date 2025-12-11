using PiShotProject.DTO;
using PiShotProject.Models;

namespace PiShotProject.Interfaces
{
    public interface IGameService
    {
        Task StartNewGameAsync(StartGameRequestDTO request);
        Task DeclareWinnerAsync(int winnerId);
        Task RecordGameResultAsync(int winnerId);
        Task StopCurrentGameAsync();
        Task<GameStatusResponse> GetCurrentStatusAsync();
    }
}
