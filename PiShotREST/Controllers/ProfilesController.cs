using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PiShotREST.Models;

namespace PiShotREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ProfilesController(IConfiguration config) { _config = config; }

        // ... imports ...
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<ProfileDTO>();
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                // Advanced Query: Get Stats + Win/Loss counts
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

                            double wlRatio = l > 0 ? (double)w / l : w; // If 0 losses, ratio is equal to wins

                            list.Add(new ProfileDTO
                            {
                                Id = (int)r["Id"],
                                Name = r["Name"].ToString(),
                                ProfileImage = r["ProfileImage"] != DBNull.Value ? r["ProfileImage"].ToString() : "",
                                Goals = g,
                                Attempts = a,
                                Accuracy = a > 0 ? Math.Round((double)g / a * 100, 1) : 0,
                                Wins = w,
                                Losses = l,
                                WinLossRatio = Math.Round(wlRatio, 2)
                            });
                        }
                    }
                }
            }
            return Ok(list);
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