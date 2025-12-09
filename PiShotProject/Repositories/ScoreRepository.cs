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

        public void AddScore(int profileId)
        {
            var score = new Score { ProfileId = profileId, ScoredAt = DateTime.Now };
            _context.Scores.Add(score);
            _context.SaveChanges();
        }

        public void AddAttempt(int profileId)
        {
            var attempt = new ShotAttempt { ProfileId = profileId, AttemptedAt = DateTime.Now };
            _context.ShotAttempts.Add(attempt);
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
