using PiShotProject.Models;

namespace PiShotProject.Interfaces
{
    public interface IGameRepository
    {
        Game StartGame(int p1Id, int p2Id);
        Game? GetActiveGame();
        void FinishGame(int gameId, int winnerId);
        void ResetGame(int gameId);
    }
}