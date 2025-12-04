using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PiShotWebApi.Models;

namespace PiShotWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ProfilesController(IConfiguration config) { _config = config; }
        [HttpGet]
        public IActionResult GetAll()
        {
            var rawList = new List<ProfileDTO>();
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                // 1. Fetch Data (Same query as before)
                string query = @"
            SELECT p.Id, p.Name, p.ProfileImage,
            (SELECT COUNT(*) FROM Scores s WHERE s.ProfileId = p.Id) as Goals,
            (SELECT COUNT(*) FROM ShotAttempts a WHERE a.ProfileId = p.Id) as Attempts,
            (SELECT COUNT(*) FROM GameResults w WHERE w.WinnerId = p.Id) as Wins,
            (SELECT COUNT(*) FROM GameResults l WHERE l.LoserId = p.Id) as Losses
            FROM Profiles p";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int g = r["Goals"] != DBNull.Value ? (int)r["Goals"] : 0;
                            int a = r["Attempts"] != DBNull.Value ? (int)r["Attempts"] : 0;
                            int w = r["Wins"] != DBNull.Value ? (int)r["Wins"] : 0;
                            int l = r["Losses"] != DBNull.Value ? (int)r["Losses"] : 0;

                            // Avoid division by zero
                            double accuracy = a > 0 ? (double)g / a * 100 : 0;

                            // Win Ratio (Percentage of games won)
                            int totalGames = w + l;
                            double winRate = totalGames > 0 ? (double)w / totalGames : 0;

                            rawList.Add(new ProfileDTO
                            {
                                Id = (int)r["Id"],
                                Name = r["Name"].ToString(),
                                ProfileImage = r["ProfileImage"] != DBNull.Value ? r["ProfileImage"].ToString() : "",
                                Goals = g,
                                Attempts = a,
                                Accuracy = Math.Round(accuracy, 1),
                                Wins = w,
                                Losses = l,
                                WinLossRatio = Math.Round(winRate * 100, 1) // Store as percentage (e.g., 55.5)
                            });
                        }
                    }
                }
            }

            // 2. LOGIC: Sort by Win Ratio Descending, then Accuracy Descending
            var sortedList = rawList
                .OrderByDescending(p => p.WinLossRatio)
                .ThenByDescending(p => p.Accuracy)
                .ToList();

            // 3. LOGIC: Assign Ranks
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].Rank = i + 1;
            }

            return Ok(sortedList);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateProfileRequest req)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = "INSERT INTO Profiles (Name, ProfileImage) VALUES (@Name, @Img)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", req.Name);
                    // Handle null image
                    cmd.Parameters.AddWithValue("@Img", req.ProfileImage ?? (object)DBNull.Value);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { msg = "Profile Created" });
        }
    }
}