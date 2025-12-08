using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PiShotProject.Models;

namespace BasketballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IConfiguration _config;
        public GameController(IConfiguration config) { _config = config; }

        [HttpPost("start")]
        public IActionResult StartGame([FromBody] StartGameRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                // UPDATED: No longer truncating tables. We just reset the CurrentGame state.
                string query = @"
                    UPDATE CurrentGame 
                    SET Player1Id = @P1, Player2Id = @P2, IsActive = 1, StartTime = GETDATE(),
                        IsTiebreak = 0, TiebreakOffsetP1 = 0, TiebreakOffsetP2 = 0, CurrentWinnerId = NULL
                    WHERE Id = 1";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@P1", req.Player1Id);
                    cmd.Parameters.AddWithValue("@P2", req.Player2Id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { msg = "Game Started" });
        }

        // Called by Raspberry Pi when it detects 10 points (or win condition)
        [HttpPost("declare_winner")]
        public IActionResult DeclareWinner([FromBody] EndGameRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                // Just update the winner ID, keep IsActive = 1 so frontend sees the trophy
                string query = "UPDATE CurrentGame SET CurrentWinnerId = @Win WHERE Id = 1";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Win", req.WinnerId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { msg = "Winner Declared" });
        }

        // Called by Frontend "Record Result" button
        [HttpPost("finish")]
        public IActionResult FinishGame([FromBody] EndGameRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                // 1. Identify Loser
                int p1 = 0, p2 = 0;
                string getPlayers = "SELECT Player1Id, Player2Id FROM CurrentGame WHERE Id = 1";
                using (var cmd = new SqlCommand(getPlayers, conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) { p1 = (int)r["Player1Id"]; p2 = (int)r["Player2Id"]; }
                    }
                }
                int loserId = (req.WinnerId == p1) ? p2 : p1;

                // 2. Record Result History
                string insertHist = "INSERT INTO GameResults (WinnerId, LoserId) VALUES (@Win, @Lose)";
                using (var cmd = new SqlCommand(insertHist, conn))
                {
                    cmd.Parameters.AddWithValue("@Win", req.WinnerId);
                    cmd.Parameters.AddWithValue("@Lose", loserId);
                    cmd.ExecuteNonQuery();
                }

                // 3. Stop Game (IsActive = 0)
                string stop = "UPDATE CurrentGame SET IsActive = 0, StartTime = NULL, IsTiebreak = 0, CurrentWinnerId = NULL WHERE Id = 1";
                using (var cmd = new SqlCommand(stop, conn)) cmd.ExecuteNonQuery();
            }
            return Ok(new { msg = "Game Recorded" });
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                // Join to get Winner Profile info too
                string query = @"
                    SELECT cg.IsActive, cg.Player1Id, cg.Player2Id, cg.CurrentWinnerId,
                           w.Name as WinnerName, w.ProfileImage as WinnerImage
                    FROM CurrentGame cg 
                    LEFT JOIN Profiles w ON cg.CurrentWinnerId = w.Id
                    WHERE cg.Id = 1";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return Ok(new
                            {
                                isActive = r["IsActive"],
                                p1_id = r["Player1Id"] != DBNull.Value ? r["Player1Id"] : 0,
                                p2_id = r["Player2Id"] != DBNull.Value ? r["Player2Id"] : 0,
                                // Send Winner Info if Pi declared one
                                currentWinnerId = r["CurrentWinnerId"] != DBNull.Value ? r["CurrentWinnerId"] : null,
                                winnerName = r["WinnerName"]?.ToString(),
                                winnerImage = r["WinnerImage"]?.ToString()
                            });
                        }
                    }
                }
            }
            return Ok(new { isActive = false });
        }
    }
}