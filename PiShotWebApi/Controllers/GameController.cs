using BasketballApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using PiShotWebApi.DTO;

namespace BasketballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IGameRepostitory _repository;

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

        [HttpPost("stop")]
        public IActionResult StopGame()
        {
            _gameService.StopCurrentGame();
            return Ok(new { msg = "Game Stopped" });
        }

        [HttpPost("declare_winner")]
        public IActionResult DeclareWinner([FromBody] EndGameRequestDTO req)
        {
            _gameService.DeclareWinner(req.WinnerId);
            return Ok(new { msg = "Winner Declared" });
        }

        
        [HttpPost("finish")]
        public IActionResult FinishGame([FromBody] EndGameRequestDTO req)
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
        public ActionResult<GameStatusResponseDTO> GetCurrent()
        {
            return Ok(_gameService.GetCurrentStatus());
        }

        public GameStatusResponseDTO GetCurrentStatus()
        {
            var game = _repository.GetState();

            return new GameStatusResponseDTO
            {
                IsActive = game?.IsActive ?? false,
                P1_Id = game?.Player1Id ?? 0,
                P2_Id = game?.Player2Id ?? 0,
                CurrentWinnerId = game?.CurrentWinnerId,
                WinnerName = game?.WinnerName,
                WinnerImage = game?.WinnerImage
            };
        }
    }
}