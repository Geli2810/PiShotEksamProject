using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PiShotWebApi.Models;

namespace PiShotWebApi.Controllers
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
                // We set StartTime = GETDATE(). 
                // Any score before this time is "History". Any score after is "Live".
                string query = @"
                    UPDATE CurrentGame 
                    SET Player1Id = @P1, Player2Id = @P2, IsActive = 1, StartTime = GETDATE() 
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

        [HttpPost("reset")]
        public IActionResult Reset()
        {
            // Just mark game as inactive.
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = "UPDATE CurrentGame SET IsActive = 0 WHERE Id = 1";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { msg = "Game Ended" });
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = @"
                    SELECT 
                        cg.IsActive, 
                        cg.Player1Id, p1.Name as P1Name, 
                        cg.Player2Id, p2.Name as P2Name
                    FROM CurrentGame cg
                    LEFT JOIN Profiles p1 ON cg.Player1Id = p1.Id
                    LEFT JOIN Profiles p2 ON cg.Player2Id = p2.Id
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
                                p1_name = r["P1Name"] != DBNull.Value ? r["P1Name"] : "Unknown",
                                p2_id = r["Player2Id"] != DBNull.Value ? r["Player2Id"] : 0,
                                p2_name = r["P2Name"] != DBNull.Value ? r["P2Name"] : "Unknown"
                            });
                        }
                    }
                }
            }
            return Ok(new { isActive = false });
        }





        // POST: api/game/finish
        // Called by Frontend when a player reaches 5 points
        [HttpPost("finish")]
        public IActionResult FinishGame([FromBody] EndGameRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                // 1. Get current players to find out who lost
                int p1 = 0, p2 = 0;
                string getPlayers = "SELECT Player1Id, Player2Id FROM CurrentGame WHERE Id = 1";
                using (var cmd = new SqlCommand(getPlayers, conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            p1 = (int)r["Player1Id"];
                            p2 = (int)r["Player2Id"];
                        }
                    }
                }

                // Determine Loser
                int loserId = (req.WinnerId == p1) ? p2 : p1;

                // 2. Insert into GameHistory
                string insertHistory = "INSERT INTO GameResults (WinnerId, LoserId) VALUES (@Win, @Lose)";
                using (var cmd = new SqlCommand(insertHistory, conn))
                {
                    cmd.Parameters.AddWithValue("@Win", req.WinnerId);
                    cmd.Parameters.AddWithValue("@Lose", loserId);
                    cmd.ExecuteNonQuery();
                }

                // 3. Reset Active Game State (Set IsActive = 0)
                string resetGame = "UPDATE CurrentGame SET IsActive = 0 WHERE Id = 1";
                using (var cmd = new SqlCommand(resetGame, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { msg = "Game Recorded & Finished" });
        }



    }


}