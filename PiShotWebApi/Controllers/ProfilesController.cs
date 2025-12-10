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

        // GET
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var profiles = _repository.GetAllProfiles();

            // Hent alle rækker én gang
            var allScores = _db.Scores.AsNoTracking().ToList();
            var allAttempts = _db.ShotAttempts.AsNoTracking().ToList();
            var allGameResults = _db.GameResults.AsNoTracking().ToList();

            var list = new List<ProfileDTO>();

            foreach (var p in profiles)
            {
                // Mål og forsøg
                var goals = allScores.Count(s => s.ProfileId == p.Id);
                var attempts = allAttempts.Count(a => a.ProfileId == p.Id);

                double accuracy = 0;
                if (attempts > 0)
                {
                    accuracy = Math.Round((double)goals * 100.0 / attempts, 0);
                }

                // GameResult har kun WinnerId og LoserId
                var wins = allGameResults.Count(g => g.WinnerId == p.Id);
                var losses = allGameResults.Count(g => g.LoserId == p.Id);

                double winLossRatio = 0;
                if (wins + losses > 0)
                {
                    winLossRatio = Math.Round((double)wins * 100.0 / (wins + losses), 0);
                }

                list.Add(new ProfileDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    ProfileImage = p.ProfileImage,
                    Goals = goals,
                    Attempts = attempts,
                    Accuracy = (int)accuracy,
                    Wins = wins,
                    Losses = losses,
                    WinLossRatio = (int)winLossRatio,
                    Rank = 0 // sættes nedenfor
                });
            }

            // Ranking: flest wins, så accuracy, så navn
            var ordered = list
                .OrderByDescending(x => x.Wins)
                .ThenByDescending(x => x.Accuracy)
                .ThenBy(x => x.Name)
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Rank = i + 1;
            }

            return Ok(ordered);
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
