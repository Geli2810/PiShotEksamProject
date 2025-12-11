using PiShotProject.Models;

namespace PiShotProject.Interfaces
{
    public interface IGameRepository
    {
        Task<CurrentGame?> GetStateAsync();
        Task SaveStateAsync(CurrentGame game);
        Task AddResultAsync(GameResult result);
    }
}
