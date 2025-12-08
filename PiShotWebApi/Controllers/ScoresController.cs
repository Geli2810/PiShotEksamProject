using Microsoft.AspNetCore.Mvc;
using PiShotProject.Models;
using PiShotProject.Interfaces;

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
        public IActionResult AddScore([FromBody] ScoreRequest req)
        {
            _repo.AddScore(req.ProfileId);
            return Ok();
        }

        [HttpPost("shot_attempt")]
        public IActionResult AddAttempt([FromBody] ScoreRequest req)
        {
            _repo.AddAttempt(req.ProfileId);
            return Ok();
        }

        [HttpGet("live")]
        public IActionResult GetLive()
        {
            LiveScoreDTO liveScore = _repo.GetLiveScore();

            if (!liveScore.IsTiebreak && liveScore.P1.TotalScore == 5 && liveScore.P2.TotalScore == 5)
            {
                _repo.UpdateTiebreakStatus(liveScore.P1.Id, liveScore.P2.Id);

                liveScore.IsTiebreak = true;
                liveScore.P1.VisualScore = liveScore.P1.TotalScore - 5;
                liveScore.P2.VisualScore = liveScore.P2.TotalScore - 5;
            }

            return Ok(liveScore);
        }
    }
}