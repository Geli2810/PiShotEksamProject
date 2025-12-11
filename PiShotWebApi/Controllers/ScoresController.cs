using Microsoft.AspNetCore.Mvc;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using PiShotProject.ClassDB;
using System.Net;
using System.Linq;
using PiShotProject.DTO;
using Microsoft.EntityFrameworkCore;

namespace PiShotWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly IScoreRepository _repo;
        private readonly PiShotDBContext _db;

        public ScoresController(IScoreRepository repo, PiShotDBContext db)
        {
            _repo = repo;
            _db = db;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddScore([FromBody] ScoreRequest req)
        {
            if (req == null || req.ProfileId <= 0)
            {
                return BadRequest("Ugyldigt ProfileId. ScoreRequest er påkrævet.");
            }

            try
            {
                await _repo.AddScoreAsync(req.ProfileId);
                return CreatedAtAction(nameof(GetLive), null);
            }
            catch (InvalidOperationException ex)
            {
                // F.eks. "Det er den anden spillers tur" eller "Der er ingen aktiv kamp"
                return BadRequest(new { msg = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { msg = "Uventet fejl ved AddScore", detail = ex.Message });
            }
        }

        [HttpPost("shot_attempt")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddAttempt([FromBody] ScoreRequest req)
        {
            if (req == null || req.ProfileId <= 0)
            {
                return BadRequest("Ugyldigt ProfileId. ScoreRequest er påkrævet.");
            }

            try
            {
                await _repo.AddAttemptAsync(req.ProfileId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { msg = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { msg = "Uventet fejl ved AddAttempt", detail = ex.Message });
            }
        }

        [HttpGet("live")]
        [ProducesResponseType(typeof(LiveScoreDTO), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLive()
        {
            var gameEntity = await _repo.GetCurrentGameEntityAsync();

            // Hvis der ikke er noget game eller ingen starttid → tom score
            if (gameEntity == null || !gameEntity.StartTime.HasValue)
            {
                return Ok(new LiveScoreDTO
                {
                    IsTiebreak = false,
                    P1 = new PlayerScoreDTO(),
                    P2 = new PlayerScoreDTO()
                });
            }

            // Hvis spillerne ikke er sat (null), så giver det heller ikke mening at hente scores
            if (!gameEntity.Player1Id.HasValue || !gameEntity.Player2Id.HasValue)
            {
                return Ok(new LiveScoreDTO
                {
                    IsTiebreak = false,
                    P1 = new PlayerScoreDTO(),
                    P2 = new PlayerScoreDTO()
                });
            }

            var p1Id = gameEntity.Player1Id.Value;
            var p2Id = gameEntity.Player2Id.Value;
            var start = gameEntity.StartTime.Value;

            // Totale mål siden kampstart
            int total1 = await _db.Scores
                .CountAsync(s => s.ProfileId == p1Id && s.ScoredAt >= start);

            int total2 = await _db.Scores
                .CountAsync(s => s.ProfileId == p2Id && s.ScoredAt >= start);

            // Totale forsøg siden kampstart (inkl. dem hvor der scores)
            int attempts1 = await _db.ShotAttempts
                .CountAsync(a => a.ProfileId == p1Id && a.AttemptedAt >= start);

            int attempts2 = await _db.ShotAttempts
                .CountAsync(a => a.ProfileId == p2Id && a.AttemptedAt >= start);

            // Tiebreak-logik: hvis begge står 5–5 og vi IKKE allerede er i tiebreak
            if (!gameEntity.IsTiebreak && total1 == 5 && total2 == 5)
            {
                await _repo.UpdateTiebreakStatusAsync(p1Id, p2Id);

                gameEntity.IsTiebreak = true;
                gameEntity.TiebreakOffsetP1 = 5;
                gameEntity.TiebreakOffsetP2 = 5;
            }

            var liveScore = new LiveScoreDTO
            {
                IsTiebreak = gameEntity.IsTiebreak,
                P1 = new PlayerScoreDTO
                {
                    Id = p1Id,
                    Name = gameEntity.Player1?.Name ?? "P1",
                    ProfileImage = gameEntity.Player1?.ProfileImage ?? "",
                    TotalScore = total1,
                    VisualScore = total1 - gameEntity.TiebreakOffsetP1,
                    Attempts = attempts1
                },
                P2 = new PlayerScoreDTO
                {
                    Id = p2Id,
                    Name = gameEntity.Player2?.Name ?? "P2",
                    ProfileImage = gameEntity.Player2?.ProfileImage ?? "",
                    TotalScore = total2,
                    VisualScore = total2 - gameEntity.TiebreakOffsetP2,
                    Attempts = attempts2
                }
            };

            return Ok(liveScore);
        }
    }
}
