// Fil: PiShotProject.ClassDB/PiShotDBContext.cs (OPDATERET)

using Microsoft.EntityFrameworkCore;
using PiShotProject.Models; // VIGTIGT: Brug Models i stedet for Entities
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
        public DbSet<Score> Scores { get; set; }
        public DbSet<ShotAttempt> ShotAttempts { get; set; }
        public DbSet<CurrentGame> CurrentGame { get; set; }
        public DbSet<GameResult> GameResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrentGame>()
                .Property(cg => cg.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<CurrentGame>()
                .HasOne(cg => cg.Player1)
                .WithMany()
                .HasForeignKey(cg => cg.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurrentGame>()
                .HasOne(cg => cg.Player2)
                .WithMany()
                .HasForeignKey(cg => cg.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Score>()
                .HasOne(s => s.Profile)
                .WithMany()
                .HasForeignKey(s => s.ProfileId);

            modelBuilder.Entity<ShotAttempt>()
                .HasOne(s => s.Profile)
                .WithMany()
                .HasForeignKey(s => s.ProfileId);

            modelBuilder.Entity<GameResult>()
                .HasOne(gr => gr.Winner)
                .WithMany()
                .HasForeignKey(gr => gr.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameResult>()
                .HasOne(gr => gr.Loser)
                .WithMany()
                .HasForeignKey(gr => gr.LoserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<CurrentGame>().HasData(
                new CurrentGame { Id = 1, Player1Id = 0, Player2Id = 0, IsActive = false, StartTime = null, CurrentWinnerId = null }
                );           
        }
    }
}