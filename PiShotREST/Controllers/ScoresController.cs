using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PiShotREST.Models;

namespace BasketballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ScoresController(IConfiguration config) { _config = config; }

        // POST: Record a Goal (Adds to lifetime history immediately)
        [HttpPost]
        public IActionResult AddScore([FromBody] ScoreRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = "INSERT INTO Scores (ProfileId) VALUES (@Pid)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Pid", req.ProfileId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        // POST: Record an Attempt (Adds to lifetime history immediately)
        [HttpPost("shot_attempt")]
        public IActionResult AddAttempt([FromBody] ScoreRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = "INSERT INTO ShotAttempts (ProfileId) VALUES (@Pid)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Pid", req.ProfileId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        // GET: Live Scoreboard (Filtered by Time)
        [HttpGet("live")]
        public IActionResult GetLive()
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                // LOGIC:
                // 1. Get the Time the game started (@Start)
                // 2. Count scores for P1 and P2 where Timestamp >= @Start

                string query = @"
                    DECLARE @Start DATETIME;
                    DECLARE @P1 INT; 
                    DECLARE @P2 INT;

                    SELECT @Start = StartTime, @P1 = Player1Id, @P2 = Player2Id 
                    FROM CurrentGame WHERE Id = 1;

                    SELECT 
                        p.Id, p.Name, 
                        (SELECT COUNT(*) FROM Scores s 
                         WHERE s.ProfileId = p.Id AND s.ScoredAt >= @Start) as Score
                    FROM Profiles p 
                    WHERE p.Id = @P1 OR p.Id = @P2";

                var players = new List<dynamic>();

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            players.Add(new
                            {
                                id = (int)r["Id"],
                                name = r["Name"].ToString(),
                                score = (int)r["Score"]
                            });
                        }
                        return Ok(players);
                    }
                }
            }
        }
    }
}