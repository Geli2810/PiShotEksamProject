using PiShotProject.DTO;
using PiShotProject.Models;
using System.Collections.Generic;

namespace PiShotProject.Interfaces
{
    public interface IProfileRepository
    {
        Task<List<Profile>> GetAllProfilesAsync();
        Task<Profile?> GetProfileByIdAsync(int id);
        Task<Profile> AddProfileAsync(Profile profile);
        Task<Profile?> UpdateProfileAsync(Profile profile, int id);
        Task<Profile?> DeleteProfileAsync(int id);
        Task<List<ProfileDTO>> GetAllProfilesWithStatsAsync();
    }
}
