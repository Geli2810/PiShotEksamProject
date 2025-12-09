using PiShotProject.Models;

namespace PiShotProject.Interfaces
{
    public interface IGameRepository
    {
        CurrentGame? GetState();
        void SaveState(CurrentGame game);
        void AddResult(GameResult result);
    }
}
