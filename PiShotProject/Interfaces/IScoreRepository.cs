using PiShotProject.Models;

namespace PiShotProject.Interfaces
{
    public interface IScoreRepository
    {
        Task AddAttemptAsync(int profileId);
        Task AddScoreAsync(int profileId);
        Task<CurrentGame?> GetCurrentGameEntityAsync();
        Task UpdateTiebreakStatusAsync(int p1Id, int p2Id);
    }
}