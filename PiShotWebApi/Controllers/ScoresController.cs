using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BasketballApi.Models;

namespace BasketballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ScoresController(IConfiguration config) { _config = config; }

        [HttpPost]
        public IActionResult AddScore([FromBody] ScoreRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = "INSERT INTO Scores (ProfileId) VALUES (@Pid)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Pid", req.ProfileId);
                    conn.Open(); cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        [HttpPost("shot_attempt")]
        public IActionResult AddAttempt([FromBody] ScoreRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = "INSERT INTO ShotAttempts (ProfileId) VALUES (@Pid)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Pid", req.ProfileId);
                    conn.Open(); cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        [HttpGet("live")]
        public IActionResult GetLive()
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                // 1. Get Game Config AND StartTime
                string qGame = @"
                    SELECT cg.Player1Id, cg.Player2Id, cg.StartTime, cg.IsTiebreak, 
                           cg.TiebreakOffsetP1, cg.TiebreakOffsetP2,
                           p1.Name as P1Name, p1.ProfileImage as P1Img,
                           p2.Name as P2Name, p2.ProfileImage as P2Img
                    FROM CurrentGame cg
                    LEFT JOIN Profiles p1 ON cg.Player1Id = p1.Id
                    LEFT JOIN Profiles p2 ON cg.Player2Id = p2.Id
                    WHERE cg.Id = 1";

                int p1Id = 0, p2Id = 0, off1 = 0, off2 = 0;
                string p1Name = "P1", p2Name = "P2", p1Img = "", p2Img = "";
                bool isTiebreak = false;
                DateTime startTime = DateTime.MinValue;

                using (var cmd = new SqlCommand(qGame, conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            p1Id = (int)r["Player1Id"]; p2Id = (int)r["Player2Id"];
                            p1Name = r["P1Name"]?.ToString() ?? "P1"; p2Name = r["P2Name"]?.ToString() ?? "P2";
                            p1Img = r["P1Img"]?.ToString() ?? ""; p2Img = r["P2Img"]?.ToString() ?? "";
                            isTiebreak = r["IsTiebreak"] != DBNull.Value && (bool)r["IsTiebreak"];
                            off1 = r["TiebreakOffsetP1"] != DBNull.Value ? (int)r["TiebreakOffsetP1"] : 0;
                            off2 = r["TiebreakOffsetP2"] != DBNull.Value ? (int)r["TiebreakOffsetP2"] : 0;

                            if (r["StartTime"] != DBNull.Value)
                            {
                                startTime = (DateTime)r["StartTime"];
                            }
                        }
                    }
                }

                // 2. Count Scores ONLY for this session
                string qScores = "SELECT ProfileId, COUNT(*) as Cnt FROM Scores WHERE ScoredAt >= @GameStart GROUP BY ProfileId";
                int total1 = 0, total2 = 0;
                using (var cmd = new SqlCommand(qScores, conn))
                {
                    cmd.Parameters.AddWithValue("@GameStart", startTime);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int pid = (int)r["ProfileId"];
                            if (pid == p1Id) total1 = (int)r["Cnt"];
                            if (pid == p2Id) total2 = (int)r["Cnt"];
                        }
                    }
                }

                // 3. CHECK FOR TIEBREAK TRIGGER (5-5)
                // If both reach 5, we activate Tiebreak and set offsets so visual score resets
                if (!isTiebreak && total1 == 5 && total2 == 5)
                {
                    string setTb = "UPDATE CurrentGame SET IsTiebreak = 1, TiebreakOffsetP1 = 5, TiebreakOffsetP2 = 5 WHERE Id = 1";
                    using (var cmd = new SqlCommand(setTb, conn)) cmd.ExecuteNonQuery();
                    isTiebreak = true; off1 = 5; off2 = 5;
                }

                // 4. Return Data
                return Ok(new LiveScoreDTO
                {
                    IsTiebreak = isTiebreak,
                    P1 = new PlayerScoreDTO { Id = p1Id, Name = p1Name, ProfileImage = p1Img, TotalScore = total1, VisualScore = total1 - off1 },
                    P2 = new PlayerScoreDTO { Id = p2Id, Name = p2Name, ProfileImage = p2Img, TotalScore = total2, VisualScore = total2 - off2 }
                });
            }
        }
    }
}