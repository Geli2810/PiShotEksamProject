using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BasketballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ScoresController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/scores
        // Called by Catapult Pi when a goal is confirmed
        [HttpPost]
        public IActionResult PostScore([FromBody] ScoreRequest request)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Scores (PlayerName, ScoredAt) VALUES (@PlayerName, GETDATE())";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerName", request.Player);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Score saved" });
        }


        [HttpPost("shot_attempt")]
        public IActionResult RegisterAttempt([FromBody] ScoreRequest request)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Insert into the NEW table
                string query = "INSERT INTO ShotAttempts (PlayerName, AttemptedAt) VALUES (@PlayerName, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PlayerName", request.Player);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Attempt recorded" });
        }


        // GET: api/scores/live
        // Called by Vue Frontend to get the scoreboard
        [HttpGet("live")]
        public IActionResult GetLiveScores()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            var result = new { Player1 = 0, Player2 = 0 };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Efficiently count scores in one query
                string query = "SELECT PlayerName, COUNT(*) as Count FROM Scores GROUP BY PlayerName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int p1 = 0;
                        int p2 = 0;
                        while (reader.Read())
                        {
                            string player = reader["PlayerName"].ToString();
                            int count = (int)reader["Count"];

                            if (player == "Player1") p1 = count;
                            if (player == "Player2") p2 = count;
                        }
                        return Ok(new { Player1 = p1, Player2 = p2 });
                    }
                }
            }
        }
    }

    public class ScoreRequest
    {
        public string Player { get; set; }
    }
}