using PiShotProject.ClassDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject
{
    public class ProfileRepository
    {
        private readonly PiShotDBContext _dbContext;

        public ProfileRepository(PiShotDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Profile> GetAllProfiles()
        {
            return _dbContext.Profiles.ToList();
        }

        public Profile AddProfile(Profile profile)
        {
            profile.Id = 0;
            if (profile.ProfileImagePath == null)
            {
                profile.ProfileImagePath = Profile.DefaultProfileImagePath;
            }
            _dbContext.Profiles.Add(profile);
            _dbContext.SaveChanges();
            return profile;
        }
        
        public Profile? GetProfileById(int id)
        {
            return _dbContext.Profiles.FirstOrDefault(p => p.Id == id);
        }

        public Profile? UpdateProfile(Profile profile, int id)
        {
            Profile? existingProfile = GetProfileById(id);
            if (existingProfile == null)
            {
                return null;
            }
            existingProfile.Name = profile.Name;
            existingProfile.ProfileImagePath = profile.ProfileImagePath;
            _dbContext.SaveChanges();
            return existingProfile;
        }

        public Profile? DeleteProfile(int id)
        {
            Profile? profileToDelete = GetProfileById(id);
            if (profileToDelete == null)
            {
                return null;
            }
            _dbContext.Profiles.Remove(profileToDelete);
            _dbContext.SaveChanges();
            return profileToDelete;
        }
    }
}
