using Microsoft.EntityFrameworkCore;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using PiShotProject.ClassDB;

namespace PiShotProject.Repositories
{
    public class ScoreRepository : IScoreRepository
    {
        private readonly PiShotDBContext _context;

        public ScoreRepository(PiShotDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registrerer et forsøg UDEN mål.
        /// Håndhæver at der er aktiv kamp og at spillerne SKIFTER til at skyde.
        /// </summary>
        public async Task AddAttemptAsync(int profileId)
        {
            await RegisterShotAsync(profileId, isGoal: false);
        }

        /// <summary>
        /// Registrerer et forsøg MED mål.
        /// Laver både ShotAttempt og Score i samme transaktion
        /// og håndhæver at spillerne SKIFTER til at skyde.
        /// </summary>
        public async Task AddScoreAsync(int profileId)
        {
            await RegisterShotAsync(profileId, isGoal: true);
        }

        /// <summary>
        /// Fælles logik for skud (forsøg + evt. mål).
        /// - Tjekker at der findes en aktiv kamp
        /// - Tjekker at profilen er en af spillerne i kampen
        /// - Tjekker at det ikke er samme spiller som skød sidst
        /// - Opretter altid en ShotAttempt
        /// - Opretter Score hvis isGoal = true
        /// </summary>
        private async Task RegisterShotAsync(int profileId, bool isGoal)
        {
            var game = await GetCurrentGameEntityAsync();

            if (game == null || !game.StartTime.HasValue)
            {
                throw new InvalidOperationException("Der er ingen aktiv kamp.");
            }

            if (!game.Player1Id.HasValue || !game.Player2Id.HasValue)
            {
                throw new InvalidOperationException("Kampen har ikke to aktive spillere.");
            }

            var p1Id = game.Player1Id.Value;
            var p2Id = game.Player2Id.Value;

            if (profileId != p1Id && profileId != p2Id)
            {
                throw new InvalidOperationException("Spilleren er ikke en del af den aktive kamp.");
            }

            // Find sidste forsøg i DENNE kamp (siden starttid) fra én af de to spillere
            var lastAttempt = await _context.ShotAttempts
                .Where(a =>
                    a.AttemptedAt >= game.StartTime.Value &&
                    (a.ProfileId == p1Id || a.ProfileId == p2Id))
                .OrderByDescending(a => a.AttemptedAt)
                .FirstOrDefaultAsync();

            // Hvis sidste forsøg er lavet af samme spiller -> ikke din tur
            if (lastAttempt != null && lastAttempt.ProfileId == profileId)
            {
                throw new InvalidOperationException("Det er den anden spillers tur til at skyde.");
            }

            var now = DateTime.Now;

            // Opret forsøg
            var attempt = new ShotAttempt
            {
                ProfileId = profileId,
                AttemptedAt = now
            };
            await _context.ShotAttempts.AddAsync(attempt);

            // Hvis mål → opret også score
            if (isGoal)
            {
                var score = new Score
                {
                    ProfileId = profileId,
                    ScoredAt = now
                };
                    await _context.Scores.AddAsync(score);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<CurrentGame> GetCurrentGameEntityAsync()
        {
            return await _context.CurrentGame
                .Include(cg => cg.Player1)
                .Include(cg => cg.Player2)
                .FirstOrDefaultAsync(cg => cg.Id == 1);
        }

        public async Task UpdateTiebreakStatusAsync(int p1Id, int p2Id)
        {
            var game = await _context.CurrentGame
                .FirstOrDefaultAsync(cg => cg.Id == 1);

            if (game != null && game.Player1Id == p1Id && game.Player2Id == p2Id)
            {
                game.IsTiebreak = true;
                game.TiebreakOffsetP1 = 5;
                game.TiebreakOffsetP2 = 5;
                await _context.SaveChangesAsync();
            }
        }
    }
}
