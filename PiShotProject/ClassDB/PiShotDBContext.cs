using Microsoft.EntityFrameworkCore;
using PiShotProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.ClassDB
{
    public class PiShotDBContext : DbContext
    {
        public PiShotDBContext(DbContextOptions<PiShotDBContext> options) : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }

        public DbSet<Game>Games { get; set; }
        public DbSet<GameResult> GameResults { get; set; }
        public DbSet<CurrentGame> CurrentGames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrentGame>().HasData(
                new CurrentGame { Id = 1, Player1Id = 0, Player2Id = 0, IsActive = false, StartTime = null, CurrentWinnerId = null }
                );
        }
    }
}
