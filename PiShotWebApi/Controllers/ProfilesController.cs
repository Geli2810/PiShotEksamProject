using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using PiShotWebApi.DTO;

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

        // GET: api/profiles
        // Returnerer kun rå data (goals, attempts, wins, losses).
        // Alt med Accuracy, WinLossRatio og Rank beregnes på frontend.
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var profiles = _repository.GetAllProfiles().ToList();

            // Hent alle rækker én gang
            var allScores = _db.Scores.AsNoTracking().ToList();
            var allAttempts = _db.ShotAttempts.AsNoTracking().ToList();
            var allGameResults = _db.GameResults.AsNoTracking().ToList();

            var list = new List<ProfileDTO>();

            foreach (var p in profiles)
            {
                var goals = allScores.Count(s => s.ProfileId == p.Id);
                var attempts = allAttempts.Count(a => a.ProfileId == p.Id);
                var wins = allGameResults.Count(g => g.WinnerId == p.Id);
                var losses = allGameResults.Count(g => g.LoserId == p.Id);

                list.Add(new ProfileDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    ProfileImage = p.ProfileImage,

                    // kun rå tal – frontend regner videre
                    Goals = goals,
                    Attempts = attempts,
                    Wins = wins,
                    Losses = losses,

                    // disse er placeholders (frontend sætter rigtige værdier)
                    Accuracy = 0,
                    WinLossRatio = 0,
                    Rank = 0
                });
            }

            // Ingen sortering / rank her – det håndteres i frontend
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
