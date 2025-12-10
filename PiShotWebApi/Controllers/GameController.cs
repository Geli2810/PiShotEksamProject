using Microsoft.AspNetCore.Mvc;
using PiShotProject.DTO;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using System.Net;

namespace PiShotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        // POST: /api/game/start
        [HttpPost("start")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult StartGame([FromBody] StartGameRequest req)
        {
            if (req == null || req.Player1Id <= 0 || req.Player2Id <= 0 || req.Player1Id == req.Player2Id)
            {
                return BadRequest(new { msg = "Player1Id og Player2Id skal være gyldige og forskellige." });
            }

            _gameService.StartNewGame(req);
            return Ok(new { msg = "Game Started" });
        }

        // POST: /api/game/stop
        [HttpPost("stop")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public IActionResult StopGame()
        {
            _gameService.StopCurrentGame();
            return Ok(new { msg = "Game Stopped" });
        }

        // POST: /api/game/declare_winner
        [HttpPost("declare_winner")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult DeclareWinner([FromBody] EndGameRequestDTO req)
        {
            if (req == null || req.WinnerId <= 0)
            {
                return BadRequest(new { msg = "WinnerId er påkrævet." });
            }

            _gameService.DeclareWinner(req.WinnerId);
            return Ok(new { msg = "Winner Declared" });
        }

        // POST: /api/game/finish
        [HttpPost("finish")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult FinishGame([FromBody] EndGameRequestDTO req)
        {
            if (req == null || req.WinnerId <= 0)
            {
                return BadRequest(new { msg = "WinnerId er påkrævet." });
            }

            try
            {
                _gameService.RecordGameResult(req.WinnerId);
                return Ok(new { msg = "Game Result Recorded" });
            }
            catch (Exception ex)
            {
                // fx "No active game found"
                return BadRequest(new { msg = ex.Message });
            }
        }

        // GET: /api/game/current
        [HttpGet("current")]
        [ProducesResponseType(typeof(GameStatusResponse), (int)HttpStatusCode.OK)]
        public ActionResult<GameStatusResponse> GetCurrent()
        {
            var status = _gameService.GetCurrentStatus();
            return Ok(status);
        }
    }
}
