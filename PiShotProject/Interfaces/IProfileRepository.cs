using System.Collections.Generic;
using PiShotProject.Models;

namespace PiShotProject.Interfaces
{
    public interface IProfileRepository
    {
        List<ProfileDTO> GetAllProfilesWithStats();
        List<Profile> GetAllProfiles();
        Profile AddProfile(Profile profile);
        Profile? GetProfileById(int id);
        Profile? UpdateProfile(Profile profile, int id);
        Profile? DeleteProfile(int id);
    }
}
