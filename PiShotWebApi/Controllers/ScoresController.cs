using Microsoft.AspNetCore.Mvc;
using PiShotWebApi.DTO;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using System.Net;

namespace PiShotWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly IScoreRepository _repo;

        public ScoresController(IScoreRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult AddScore([FromBody] ScoreRequest req)
        {
            if (req == null || req.ProfileId <= 0)
            {
                return BadRequest("Ugyldigt ProfileId. ScoreRequest er påkrævet.");
            }

            _repo.AddScore(req.ProfileId);

            return CreatedAtAction(nameof(GetLive), null);
        }

        [HttpPost("shot_attempt")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult AddAttempt([FromBody] ScoreRequest req)
        {
            if (req == null || req.ProfileId <= 0)
            {
                return BadRequest("Ugyldigt ProfileId. ScoreRequest er påkrævet.");
            }

            _repo.AddAttempt(req.ProfileId);

            return NoContent();
        }

        [HttpGet("live")]
        [ProducesResponseType(typeof(LiveScoreDTO), (int)HttpStatusCode.OK)]
        public IActionResult GetLive()
        {
            var gameEntity = _repo.GetCurrentGameEntity();

            if (gameEntity == null || !gameEntity.StartTime.HasValue)
            {
                return Ok(new LiveScoreDTO { P1 = new PlayerScoreDTO(), P2 = new PlayerScoreDTO() });
            }

            (int total1, int total2) = _repo.GetScoresSinceGameStart(
                gameEntity.Player1Id,
                gameEntity.Player2Id,
                gameEntity.StartTime.Value);

            if (!gameEntity.IsTiebreak && total1 == 5 && total2 == 5)
            {
                _repo.UpdateTiebreakStatus(gameEntity.Player1Id, gameEntity.Player2Id);

                gameEntity.IsTiebreak = true;
                gameEntity.TiebreakOffsetP1 = 5;
                gameEntity.TiebreakOffsetP2 = 5;
            }

            var liveScore = new LiveScoreDTO
            {
                IsTiebreak = gameEntity.IsTiebreak,
                P1 = new PlayerScoreDTO
                {
                    Id = gameEntity.Player1Id,
                    Name = gameEntity.Player1?.Name ?? "P1",
                    ProfileImage = gameEntity.Player1?.ProfileImage ?? "",
                    TotalScore = total1,
                    VisualScore = total1 - gameEntity.TiebreakOffsetP1
                },
                P2 = new PlayerScoreDTO
                {
                    Id = gameEntity.Player2Id,
                    Name = gameEntity.Player2?.Name ?? "P2",
                    ProfileImage = gameEntity.Player2?.ProfileImage ?? "",
                    TotalScore = total2,
                    VisualScore = total2 - gameEntity.TiebreakOffsetP2
                }
            };

            return Ok(liveScore);
        }
    }
}