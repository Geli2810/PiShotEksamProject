using Microsoft.AspNetCore.Mvc;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using PiShotProject.ClassDB;
using System.Net;
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

        // POST: api/scores (Called when GOAL is detected)
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddScore([FromBody] ScoreRequest req)
        {
            if (req == null || req.ProfileId <= 0)
            {
                return BadRequest(new { msg = "Invalid ProfileId." });
            }

            try
            {
                // Logic C: Only adds a score, looks for existing attempt
                await _repo.AddScoreAsync(req.ProfileId);
                return Ok(new { msg = "Goal Registered" });
            }
            catch (InvalidOperationException ex)
            {
                // Returns 400 if no attempt was found or game inactive
                return BadRequest(new { msg = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { msg = "Internal Error", detail = ex.Message });
            }
        }

        // POST: api/scores/shot_attempt (Called when Servo fires)
        [HttpPost("shot_attempt")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddAttempt([FromBody] ScoreRequest req)
        {
            if (req == null || req.ProfileId <= 0)
            {
                return BadRequest(new { msg = "Invalid ProfileId." });
            }

            try
            {
                // Logic C: Creates attempt and checks "Whose turn is it?"
                await _repo.AddAttemptAsync(req.ProfileId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // Returns 400 if it is not this player's turn
                return BadRequest(new { msg = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { msg = "Internal Error", detail = ex.Message });
            }
        }

        [HttpGet("live")]
        [ProducesResponseType(typeof(LiveScoreDTO), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLive()
        {
            var gameEntity = await _repo.GetCurrentGameEntityAsync();

            // Default empty state if no game is running
            if (gameEntity == null || !gameEntity.StartTime.HasValue || !gameEntity.Player1Id.HasValue || !gameEntity.Player2Id.HasValue)
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

            // Count Scores (Goals)
            int total1 = await _db.Scores.CountAsync(s => s.ProfileId == p1Id && s.ScoredAt >= start);
            int total2 = await _db.Scores.CountAsync(s => s.ProfileId == p2Id && s.ScoredAt >= start);

            // Count Attempts
            int attempts1 = await _db.ShotAttempts.CountAsync(a => a.ProfileId == p1Id && a.AttemptedAt >= start);
            int attempts2 = await _db.ShotAttempts.CountAsync(a => a.ProfileId == p2Id && a.AttemptedAt >= start);

            // Check Tiebreak Condition (5-5)
            if (!gameEntity.IsTiebreak && total1 == 5 && total2 == 5)
            {
                await _repo.UpdateTiebreakStatusAsync(p1Id, p2Id);
                // Update local variables for visual correctness immediately
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