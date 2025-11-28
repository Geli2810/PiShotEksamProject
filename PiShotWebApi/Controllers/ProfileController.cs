using Microsoft.AspNetCore.Mvc;
using PiShotProject;
using PiShotWebApi.DTO_Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PiShotWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private ProfileRepository _repo;

        public ProfileController(ProfileRepository repo)
        {
            _repo = repo;
        }

        // GET: api/<ProfileController>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpGet]
        public ActionResult <IEnumerable<Profile>> Get()
        {
            var GetAllProfiles = _repo.GetAllProfiles();
            return GetAllProfiles.Any() ? Ok(GetAllProfiles) : NoContent();
        }

        // GET api/<ProfileController>/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult <Profile> Get(int id)
        {
            var profile = _repo.GetProfileById(id);
            return profile != null ? Ok(profile) : NotFound();
        }

        // POST api/<ProfileController>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public ActionResult <Profile> Post([FromBody] ProfileDTO newProfile)
        {
            try
            {
                Profile profile = ConvertDTOToProfile(newProfile);
                _repo.AddProfile(profile);
                return Created("/" + profile.Id, profile);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<ProfileController>/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}")]
        public ActionResult<Profile> Put(int id, [FromBody] ProfileDTO updateProfile)
        {
            try
            {
                Profile profile = ConvertDTOToProfile(updateProfile);
                var reformedProfile = _repo.UpdateProfile(profile, id);
                return reformedProfile != null ? Ok(reformedProfile) : NotFound();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // DELETE api/<ProfileController>/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]
        public ActionResult<Profile> Delete(int id)
        {
            var deletedProfile = _repo.DeleteProfile(id);
            return deletedProfile != null ? Ok(deletedProfile) : NotFound();
        }

        private Profile ConvertDTOToProfile(ProfileDTO profileDTO)
        {
            Profile profile = new Profile()
            {
                Name = profileDTO.Name,
                ProfileImagePath = profileDTO.ProfileImagePath
            };
            return profile;
        }
    }
}
