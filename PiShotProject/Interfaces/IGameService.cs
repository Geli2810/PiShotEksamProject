using PiShotProject.Models;

namespace PiShotProject.Interfaces
{
    public interface IGameService
    {
        void StartNewGame(StartGameRequest request);
        void DeclareWinner(int winnerId);
        void RecordGameResult(int winnerId);
        void StopCurrentGame();
        GameStatusResponse GetCurrentStatus();
    }
}
