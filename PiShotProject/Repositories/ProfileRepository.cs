using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
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
            foreach (var p in profiles)
            {
                //_dbContext.Entry(p).Collection<Score>("Scores").Query().Where(s => s.ProfileId == p.Id).Load();
                //_dbContext.Entry(p).Collection<ShotAttempt>("ShotAttempts").Query().Where(a => a.ProfileId == p.Id).Load();
                //_dbContext.Entry(p).Collection<GameResult>("Wins").Query().Where(g => g.WinnerId == p.Id).Load();
                //_dbContext.Entry(p).Collection<GameResult>("Losses").Query().Where(g => g.LoserId == p.Id).Load();
            }

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
    }
}
