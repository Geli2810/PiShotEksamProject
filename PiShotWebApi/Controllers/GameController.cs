using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BasketballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GameController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/game/reset
        // Called by Pi when script starts
        [HttpPost("reset")]
        public IActionResult ResetGame()
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Clear the table for the new game
                string query = "TRUNCATE TABLE Scores";
                // OR if you want history: "UPDATE GameState SET IsActive=0 WHERE IsActive=1"

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Game Reset" });
        }

        // POST: api/game/end
        [HttpPost("end")]
        public IActionResult EndGame([FromBody] EndGameRequest request)
        {
            // You could store the winner in a separate table or log it
            return Ok(new { message = $"Game Over. Winner: {request.Winner}" });
        }
    }

    public class EndGameRequest
    {
        public string Winner { get; set; }
    }
}