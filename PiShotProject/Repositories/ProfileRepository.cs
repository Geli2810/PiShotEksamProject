using PiShotProject.ClassDB;
using PiShotProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System;
using PiShotProject.Interfaces;

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
            return _dbContext.Profiles.AsNoTracking().ToList();
        }

        public List<ProfileDTO> GetAllProfilesWithStats()
        {
            var query = _dbContext.Profiles
                .Select(p => new
                {
                    Profile = p,
                    Goals = _dbContext.Scores.Count(s => s.ProfileId == p.Id),
                    Attempts = _dbContext.ShotAttempts.Count(a => a.ProfileId == p.Id),
                    Wins = _dbContext.Games.Count(g => g.WinnerId == p.Id),
                    Losses = _dbContext.Games.Count(g =>
                        !g.IsActive &&
                        g.WinnerId != null &&
                        g.WinnerId != p.Id &&
                        (g.Profile1Id == p.Id || g.Profile2Id == p.Id))
                });

            var results = query.ToList();

            return results.Select(x => new ProfileDTO
            {
                Id = x.Profile.Id,
                Name = x.Profile.Name,
                ProfileImage = x.Profile.ProfileImagePath,
                Goals = x.Goals,
                Attempts = x.Attempts,
                Wins = x.Wins,
                Losses = x.Losses,
                Accuracy = x.Attempts > 0
                    ? Math.Round((double)x.Goals / x.Attempts * 100, 1)
                    : 0,
                WinLossRatio = x.Losses > 0
                    ? Math.Round((double)x.Wins / x.Losses, 2)
                    : x.Wins
            }).ToList();
        }

        public Profile AddProfile(Profile profile)
        {
            profile.Id = 0;
            if (string.IsNullOrEmpty(profile.ProfileImagePath))
                profile.ProfileImagePath = Profile.DefaultProfileImagePath;

            _dbContext.Profiles.Add(profile);
            _dbContext.SaveChanges();
            return profile;
        }

        public Profile? GetProfileById(int id)
        {
            return _dbContext.Profiles.Find(id);
        }

        public Profile? UpdateProfile(Profile profile, int id)
        {
            var existing = _dbContext.Profiles.Find(id);
            if (existing == null) return null;

            existing.Name = profile.Name;
            existing.ProfileImagePath = profile.ProfileImagePath;
            _dbContext.SaveChanges();
            return existing;
        }

        public Profile? DeleteProfile(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var existing = _dbContext.Profiles.Find(id);
                if (existing == null) return null;

                var games = _dbContext.Games
                    .Where(g => g.Profile1Id == id || g.Profile2Id == id)
                    .ToList();

                foreach (var game in games)
                {
                    if (game.Profile1Id == id) game.Profile1Id = null;
                    if (game.Profile2Id == id) game.Profile2Id = null;
                    if (game.WinnerId == id) game.WinnerId = null;
                }

                _dbContext.Profiles.Remove(existing);

                _dbContext.SaveChanges();
                transaction.Commit();
                return existing;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
