using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.DTO;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace PiShotProject.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly PiShotDBContext _dbContext;

        public ProfileRepository(PiShotDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            var profiles = await _dbContext.Profiles
                .AsNoTracking()
                .ToListAsync();

            return profiles;
        }

        public async Task<Profile?> GetProfileByIdAsync(int id)
        {
            return await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Profile> AddProfileAsync(Profile profile)
        {
            profile.Id = 0;
            if (string.IsNullOrEmpty(profile.ProfileImage))
                profile.ProfileImage = Profile.DefaultProfileImagePath;

            await _dbContext.Profiles.AddAsync(profile);
            await _dbContext.SaveChangesAsync();
            return profile;
        }

        public async Task<Profile?> UpdateProfileAsync(Profile profile, int id)
        {
            var existing = await GetProfileByIdAsync(id);
            if (existing == null) return null;

            existing.Name = profile.Name;
            existing.ProfileImage = profile.ProfileImage;
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<Profile?> DeleteProfileAsync(int id)
        {
            var existing = await GetProfileByIdAsync(id);
            if (existing == null) return null;

            _dbContext.Profiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<ProfileDTO>> GetAllProfilesWithStatsAsync()
        {
            return await _dbContext.Profiles.AsNoTracking().Select(p => new ProfileDTO
            {
                Id = p.Id,
                Name = p.Name,
                ProfileImage = p.ProfileImage,

                Goals = _dbContext.Scores.Count(s => s.ProfileId == p.Id),
                Attempts = _dbContext.ShotAttempts.Count(a => a.ProfileId == p.Id),
                Wins = _dbContext.GameResults.Count(g => g.WinnerId == p.Id),
                Losses = _dbContext.GameResults.Count(g => g.LoserId == p.Id),

                Accuracy = 0,
                WinLossRatio = 0,
                Rank = 0

                
            }).ToListAsync();
        }
    }
}
