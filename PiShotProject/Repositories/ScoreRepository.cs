using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Interfaces;
using PiShotProject.Models;

namespace PiShotProject.Repositories
{
    public class ScoreRepository : IScoreRepository
    {
        private readonly PiShotDBContext _context;

        public ScoreRepository(PiShotDBContext context)
        {
            _context = context;
        }

        public async Task<CurrentGame?> GetCurrentGameEntityAsync()
        {
            return await _context.CurrentGame
                .Include(cg => cg.Player1)
                .Include(cg => cg.Player2)
                .FirstOrDefaultAsync(cg => cg.Id == 1);
        }

        // ---------------------------------------------------------
        // LOGIC 1: REGISTER ATTEMPT (Handles Turn Validation)
        // ---------------------------------------------------------
        public async Task AddAttemptAsync(int profileId)
        {
            var game = await GetCurrentGameEntityAsync();

            if (game == null || !game.IsActive || !game.StartTime.HasValue)
            {
                throw new InvalidOperationException("No active game found.");
            }

            // 1. Validation: Is player part of this game?
            if (profileId != game.Player1Id && profileId != game.Player2Id)
            {
                throw new InvalidOperationException("Player is not in the current game.");
            }

            // 2. Turn Logic: Find the ABSOLUTE last attempt in this game
            var lastAttempt = await _context.ShotAttempts
                .Where(a => a.AttemptedAt >= game.StartTime.Value)
                .OrderByDescending(a => a.AttemptedAt)
                .FirstOrDefaultAsync();

            // 3. If the last person to shoot was ME, deny the shot.
            if (lastAttempt != null && lastAttempt.ProfileId == profileId)
            {
                throw new InvalidOperationException("It is the other player's turn.");
            }

            // 4. Create the Attempt
            var attempt = new ShotAttempt
            {
                ProfileId = profileId,
                AttemptedAt = DateTime.UtcNow
            };

            await _context.ShotAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();
        }

        // ---------------------------------------------------------
        // LOGIC 2: REGISTER SCORE (Connects to existing Attempt)
        // ---------------------------------------------------------
        public async Task AddScoreAsync(int profileId)
        {
            var game = await GetCurrentGameEntityAsync();

            if (game == null || !game.IsActive || !game.StartTime.HasValue)
            {
                throw new InvalidOperationException("No active game found.");
            }

            // 1. Find the most recent attempt by THIS player
            // We look back 15 seconds max to ensure we don't count an old attempt
            var cutoffTime = DateTime.UtcNow.AddSeconds(-15);

            var recentAttempt = await _context.ShotAttempts
                .Where(a => a.ProfileId == profileId
                            && a.AttemptedAt >= game.StartTime.Value
                            && a.AttemptedAt >= cutoffTime)
                .OrderByDescending(a => a.AttemptedAt)
                .FirstOrDefaultAsync();

            if (recentAttempt == null)
            {
                // Edge Case: The Pi sent a Goal signal, but the Attempt signal failed or timed out.
                // OPTION A: Throw error (Strict Mode)
                throw new InvalidOperationException("No recent shot attempt found. Goal rejected.");

                // OPTION B: Auto-create attempt (Forgiving Mode) - Uncomment if you prefer this
                /* 
                   await AddAttemptAsync(profileId); 
                */
            }

            // 2. Add the Score
            var score = new Score
            {
                ProfileId = profileId,
                ScoredAt = DateTime.UtcNow
            };

            await _context.Scores.AddAsync(score);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTiebreakStatusAsync(int p1Id, int p2Id)
        {
            var game = await _context.CurrentGame.FirstOrDefaultAsync(cg => cg.Id == 1);

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