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

            // Her VED vi, at de har værdier → derfor .Value
            (int total1, int total2) = _repo.GetScoresSinceGameStart(
                gameEntity.Player1Id.Value,
                gameEntity.Player2Id.Value,
                gameEntity.StartTime.Value);

            // Tiebreak-logik
            if (!gameEntity.IsTiebreak && total1 == 5 && total2 == 5)
            {
                _repo.UpdateTiebreakStatus(gameEntity.Player1Id.Value, gameEntity.Player2Id.Value);

                gameEntity.IsTiebreak = true;
                gameEntity.TiebreakOffsetP1 = 5;
                gameEntity.TiebreakOffsetP2 = 5;
            }

            var liveScore = new LiveScoreDTO
            {
                IsTiebreak = gameEntity.IsTiebreak,
                P1 = new PlayerScoreDTO
                {
                    Id = gameEntity.Player1Id.Value,
                    Name = gameEntity.Player1?.Name ?? "P1",
                    ProfileImage = gameEntity.Player1?.ProfileImage ?? "",
                    TotalScore = total1,
                    VisualScore = total1 - gameEntity.TiebreakOffsetP1
                },
                P2 = new PlayerScoreDTO
                {
                    Id = gameEntity.Player2Id.Value,
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
