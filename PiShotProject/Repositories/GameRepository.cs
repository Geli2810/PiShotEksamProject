using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;
using PiShotProject.ClassDB;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Repositories
{
    public class GameRepository : IGameRepostitory
    {
        private readonly string _connectionString;

        public GameRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public void AddGameResult(int winnerId, int loserId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO GameResults (WinnerId, LoserId) VALUES (@Win, @Lose)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Win", winnerId);
                    cmd.Parameters.AddWithValue("@Lose", loserId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public CurrentGame GetCurrentGame()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = @"
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
                            return new CurrentGame
                            {
                                IsActive = (bool)r["IsActive"],
                                Player1Id = r["Player1Id"] != DBNull.Value ? (int)r["Player1Id"] : 0,
                                Player2Id = r["Player2Id"] != DBNull.Value ? (int)r["Player2Id"] : 0,
                                CurrentWinnerId = r["CurrentWinnerId"] != DBNull.Value ? (int?)r["CurrentWinnerId"] : null,
                                WinnerName = r["WinnerName"]?.ToString(),
                                WinnerImage = r["WinnerImage"]?.ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void ResetGame()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = "UPDATE CurrentGame SET IsActive = 0, StartTime = NULL, IsTiebreak = 0, CurrentWinnerId = NULL WHERE Id = 1";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void StartGame(int player1Id, int player2Id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = @"UPDATE CurrentGame SET Player1Id = @P1, PLayer2Id = @P2, IsActive = 1, StartTime = GETDATE(), IsTiebreak = 0, TiebreakOffsetP1 = 0, TiebreakOffsetP2 = 0, CurrentWinnerId = NULL WHERE Id = 1";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@P1", player1Id);
                    cmd.Parameters.AddWithValue("@P2", player2Id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCurrentWinner(int winnerId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = "UPDATE CurrentGame SET CurrentWinnerId = @Win WHERE Id = 1";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Win", winnerId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
