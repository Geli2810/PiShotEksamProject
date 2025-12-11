using PiShotProject.Models; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Interfaces
{
    public interface IScoreRepository
    {
        Task AddScoreAsync(int profileId);
        Task AddAttemptAsync(int profileId);

        Task<CurrentGame?> GetCurrentGameEntityAsync();

        Task UpdateTiebreakStatusAsync(int p1Id, int p2Id);
    }
}