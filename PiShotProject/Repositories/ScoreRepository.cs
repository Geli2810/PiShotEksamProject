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
        public void AddAttempt(int profileId)
        {
            RegisterShot(profileId, isGoal: false);
        }

        /// <summary>
        /// Registrerer et forsøg MED mål.
        /// Laver både ShotAttempt og Score i samme transaktion
        /// og håndhæver at spillerne SKIFTER til at skyde.
        /// </summary>
        public void AddScore(int profileId)
        {
            RegisterShot(profileId, isGoal: true);
        }

        /// <summary>
        /// Fælles logik for skud (forsøg + evt. mål).
        /// - Tjekker at der findes en aktiv kamp
        /// - Tjekker at profilen er en af spillerne i kampen
        /// - Tjekker at det ikke er samme spiller som skød sidst
        /// - Opretter altid en ShotAttempt
        /// - Opretter Score hvis isGoal = true
        /// </summary>
        private void RegisterShot(int profileId, bool isGoal)
        {
            var game = GetCurrentGameEntity();
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
            var lastAttempt = _context.ShotAttempts
                .Where(a =>
                    a.AttemptedAt >= game.StartTime.Value &&
                    (a.ProfileId == p1Id || a.ProfileId == p2Id))
                .OrderByDescending(a => a.AttemptedAt)
                .FirstOrDefault();

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
            _context.ShotAttempts.Add(attempt);

            // Hvis mål → opret også score
            if (isGoal)
            {
                var score = new Score
                {
                    ProfileId = profileId,
                    ScoredAt = now
                };
                _context.Scores.Add(score);
            }

            _context.SaveChanges();
        }

        public CurrentGame GetCurrentGameEntity()
        {
            return _context.CurrentGame
                .Include(cg => cg.Player1)
                .Include(cg => cg.Player2)
                .FirstOrDefault(cg => cg.Id == 1);
        }

        public (int P1Score, int P2Score) GetScoresSinceGameStart(int player1Id, int player2Id, DateTime gameStartTime)
        {
            int total1 = _context.Scores
                .Count(s => s.ProfileId == player1Id && s.ScoredAt >= gameStartTime);

            int total2 = _context.Scores
                .Count(s => s.ProfileId == player2Id && s.ScoredAt >= gameStartTime);

            return (total1, total2);
        }

        public void UpdateTiebreakStatus(int p1Id, int p2Id)
        {
            var game = _context.CurrentGame
                .FirstOrDefault(cg => cg.Id == 1);

            if (game != null && game.Player1Id == p1Id && game.Player2Id == p2Id)
            {
                game.IsTiebreak = true;
                game.TiebreakOffsetP1 = 5;
                game.TiebreakOffsetP2 = 5;
                _context.SaveChanges();
            }
        }
    }
}
