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

        public List<Profile> GetAllProfiles()
        {
            var profiles = _dbContext.Profiles
                .AsNoTracking()
                .ToList();

            return profiles;
        }

        public Profile? GetProfileById(int id)
        {
            return _dbContext.Profiles.FirstOrDefault(p => p.Id == id);
        }

        public Profile AddProfile(Profile profile)
        {
            profile.Id = 0;
            if (string.IsNullOrEmpty(profile.ProfileImage))
                profile.ProfileImage = Profile.DefaultProfileImagePath;

            _dbContext.Profiles.Add(profile);
            _dbContext.SaveChanges();
            return profile;
        }

        public Profile? UpdateProfile(Profile profile, int id)
        {
            var existing = GetProfileById(id);
            if (existing == null) return null;

            existing.Name = profile.Name;
            existing.ProfileImage = profile.ProfileImage;
            _dbContext.SaveChanges();
            return existing;
        }

        public Profile? DeleteProfile(int id)
        {
            var existing = GetProfileById(id);
            if (existing == null) return null;

            _dbContext.Profiles.Remove(existing);
            _dbContext.SaveChanges();
            return existing;
        }

        public List<ProfileDTO> GetAllProfilesWithStats()
        {
            return _dbContext.Profiles.AsNoTracking().Select(p => new ProfileDTO
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

                
            }).ToList();
        }
    }
}
