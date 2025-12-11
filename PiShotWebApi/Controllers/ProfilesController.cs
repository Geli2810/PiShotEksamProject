using Microsoft.AspNetCore.Mvc;
using PiShotProject.ClassDB;
using PiShotProject.DTO;
using PiShotProject.Interfaces;
using PiShotProject.Models;

namespace PiShotWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileRepository _repository;
        private readonly PiShotDBContext _db;

        public ProfilesController(IProfileRepository repository, PiShotDBContext db)
        {
            _repository = repository;
            _db = db;
        }
        //
        // GET: api/profiles
        // Returnerer kun rå data (goals, attempts, wins, losses).
        // Alt med Accuracy, WinLossRatio og Rank beregnes på frontend.
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var profilesWithStats = await _repository.GetAllProfilesWithStatsAsync();

            return Ok(profilesWithStats);
        }

        // POST
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProfileRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { msg = "Name is required" });

            var profile = new Profile(req.Name, req.ProfileImage);
            await _repository.AddProfileAsync(profile);
            return StatusCode(StatusCodes.Status201Created, new { msg = "Created" });
        }

        // PUT
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProfileRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { msg = "Name is required" });

            var profile = new Profile(req.Name, req.ProfileImage);
            var updated = await _repository.UpdateProfileAsync(profile, id);
            if (updated == null)
                return NotFound(new { msg = "Profile not found" });

            return Ok(new { msg = "Updated" });
        }

        // DELETE
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteProfileAsync(id);
            if (deleted == null)
                return NotFound(new { msg = "Profile not found" });

            return Ok(new { msg = "Deleted" });
        }
    }
}
