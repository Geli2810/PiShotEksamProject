using PiShotProject.Interfaces;
using PiShotProject.ClassDB;
using PiShotProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PiShotProject.Repositories
{
    public class ScoreRepository : IScoreRepository
    {
        private readonly PiShotDBContext _context;
        private const int initialShotsPerPlayer = 10;

        public ScoreRepository(PiShotDBContext context)
        {
            _context = context;
        }

        public void AddScore(int profileId, int? gameId)
        {
            if (gameId == null) return;

            var game = _context.Games.Find(gameId.Value);
            if (game == null || !game.IsActive) return;

            var score = new Score
            {
                ProfileId = profileId,
                GameId = gameId
            };

            var attempt = new ShotAttempt
            {
                ProfileId = profileId,
                GameId = gameId
            };

            _context.Scores.Add(score);
            _context.ShotAttempts.Add(attempt);
            _context.SaveChanges();

            EvaluateEndCondition(game);
        }

        public void AddAttempt(int profileId, int? gameId)
        {
            if (gameId == null) return;

            var game = _context.Games.Find(gameId.Value);
            if (game == null || !game.IsActive) return;

            _context.ShotAttempts.Add(new ShotAttempt
            {
                ProfileId = profileId,
                GameId = gameId
            });

            _context.SaveChanges();

            EvaluateEndCondition(game);
        }

        public (int p1Score, int p2Score) GetGameScore(int gameId)
        {
            var game = _context.Games.Find(gameId);
            if (game == null) return (0, 0);

            int s1 = _context.Scores.Count(s => s.GameId == gameId && s.ProfileId == game.Profile1Id);
            int s2 = _context.Scores.Count(s => s.GameId == gameId && s.ProfileId == game.Profile2Id);

            return (s1, s2);
        }

        public (int p1Attempts, int p2Attempts) GetGameAttempts(int gameId)
        {
            var game = _context.Games.Find(gameId);
            if (game == null) return (0, 0);

            int a1 = _context.ShotAttempts.Count(a => a.GameId == gameId && a.ProfileId == game.Profile1Id);
            int a2 = _context.ShotAttempts.Count(a => a.GameId == gameId && a.ProfileId == game.Profile2Id);

            return (a1, a2);
        }

        private void EvaluateEndCondition(Game game)
        {
            _context.Entry(game).State = EntityState.Unchanged;

            if (game.Profile1Id == null || game.Profile2Id == null) return;

            int p1Id = game.Profile1Id.Value;
            int p2Id = game.Profile2Id.Value;

            int s1 = _context.Scores.Count(s => s.GameId == game.Id && s.ProfileId == p1Id);
            int s2 = _context.Scores.Count(s => s.GameId == game.Id && s.ProfileId == p2Id);

            int a1 = _context.ShotAttempts.Count(a => a.GameId == game.Id && a.ProfileId == p1Id);
            int a2 = _context.ShotAttempts.Count(a => a.GameId == game.Id && a.ProfileId == p2Id);

            bool reachedInitialRound = (a1 >= initialShotsPerPlayer) && (a2 >= initialShotsPerPlayer);
            bool attemptsAreEqual = (a1 == a2);
            int diff = s1 - s2;

            if (reachedInitialRound && attemptsAreEqual)
            {
                if (System.Math.Abs(diff) >= 2)
                {
                    int winnerId = diff > 0 ? p1Id : p2Id;
                    FinishGameWithWinner(game, winnerId);
                }
                return;
            }
        }

        private void FinishGameWithWinner(Game game, int winnerId)
        {
            game.Finish(winnerId);
            _context.SaveChanges();
        }
    }
}
