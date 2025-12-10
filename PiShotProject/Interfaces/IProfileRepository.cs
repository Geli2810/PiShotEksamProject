using PiShotProject.DTO;
using PiShotProject.Models;
using System.Collections.Generic;

namespace PiShotProject.Interfaces
{
    public interface IProfileRepository
    {
        List<Profile> GetAllProfiles();
        Profile? GetProfileById(int id);
        Profile AddProfile(Profile profile);
        Profile? UpdateProfile(Profile profile, int id);
        Profile? DeleteProfile(int id);
        List<ProfileDTO> GetAllProfilesWithStats();
    }
}
