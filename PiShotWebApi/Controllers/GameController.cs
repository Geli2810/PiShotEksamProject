using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BasketballApi.Models;
using PiShotProject.Interfaces;
using PiShotProject.Models;

namespace BasketballApi.Controllers
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

        [HttpPost("start")]
        public IActionResult StartGame([FromBody] StartGameRequest req)
        {
            _gameService.StartNewGame(req);
            return Ok(new { msg = "Game Started" });
        }

        
        [HttpPost("declare_winner")]
        public IActionResult DeclareWinner([FromBody] EndGameRequest req)
        {
            _gameService.DeclareWinner(req.WinnerId);
            return Ok(new { msg = "Winner Declared" });
        }

        
        [HttpPost("finish")]
        public IActionResult FinishGame([FromBody] EndGameRequest req)
        {
            try
            {
                _gameService.RecordGameResult(req.WinnerId);
                return Ok(new { msg = "Game Result Recorded" });
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new { msg = ex.Message });
            }
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            var status = _gameService.GetCurrentStatus();
            return Ok(status);
        }
    }
}