using Microsoft.AspNetCore.Mvc;
using PiShotProject.ClassDB;
using PiShotWebApi.DTO_Models;

namespace PiShotProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly GameRepository _gameRepository;

        public GamesController(GameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAllGames()
        {
            var games = _gameRepository.GetAllGames();

            if (games == null || !games.Any())
                return NotFound();

            return Ok(games);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetGameById(int id)
        {
            var game = _gameRepository.GetGameById(id);
            return game != null ? Ok(game) : NotFound();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddGame([FromBody] Game game)
        {
            if (game == null) return BadRequest();

            var addedGame = _gameRepository.AddNewGame(game);
            return CreatedAtAction(nameof(GetGameById), new { id = addedGame.Id }, addedGame);
        }

        [HttpPut("{id}/winner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetWinner(int id, [FromBody] GameDTO scores)
        {
            var game = _gameRepository.GetGameById(id);
            if (game == null) return NotFound();

            try
            {
                var updatedGame = _gameRepository.SetWinner(game, scores.Score1, scores.Score2);
                return Ok(updatedGame);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("profile/{profileId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetGamesForProfile(int profileId)
        {
            var games = _gameRepository.GetGamesForProfile(profileId);
            return Ok(games);
        }

        [HttpGet("winner/{profileId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetGamesByWinner(int profileId)
        {
            var games = _gameRepository.GetGamesByWinner(profileId);
            return Ok(games);
        }

        
    }
}
