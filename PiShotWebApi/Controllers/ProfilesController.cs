using Microsoft.AspNetCore.Mvc;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using PiShotWebApi.DTO;

namespace PiShotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileRepository _repository;

        public ProfilesController(IProfileRepository repository)
        {
            _repository = repository;
        }

        // GET
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var profiles = _repository.GetAllProfiles();
            var list = profiles.Select(p => new ProfileDTO
            {
                Id = p.Id,
                Name = p.Name,
                ProfileImage = p.ProfileImage,
                Goals = 0,
                Attempts = 0,
                Accuracy = 0,
                Wins = 0,
                Losses = 0,
                WinLossRatio = 0,
                Rank = 0
            }).ToList();

            return Ok(list);
        }

        // POST
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] CreateProfileRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { msg = "Name is required" });

            var profile = new Profile(req.Name, req.ProfileImage);
            _repository.AddProfile(profile);
            return StatusCode(StatusCodes.Status201Created, new { msg = "Created" });
        }

        // PUT
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Update(int id, [FromBody] CreateProfileRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { msg = "Name is required" });

            var profile = new Profile(req.Name, req.ProfileImage);
            var updated = _repository.UpdateProfile(profile, id);
            if (updated == null)
                return NotFound(new { msg = "Profile not found" });

            return Ok(new { msg = "Updated" });
        }

        // DELETE
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var deleted = _repository.DeleteProfile(id);
            if (deleted == null)
                return NotFound(new { msg = "Profile not found" });

            return Ok(new { msg = "Deleted" });
        }
    }
}
