using PiShotProject.Interfaces;
using PiShotProject.ClassDB;
using Microsoft.EntityFrameworkCore;
using PiShotProject.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace PiShotProject.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly PiShotDBContext _context;
        public GameRepository(PiShotDBContext context)
        {
            _context = context;
        }

        public Game StartGame(int p1Id, int p2Id)
        {
            var p1 = _context.Profiles.Find(p1Id);
            var p2 = _context.Profiles.Find(p2Id);
            if (p1 == null || p2 == null)
            {
                throw new ArgumentException("One or both player IDs do not exist in Profiles table.");
            }

            var active = GetActiveGame();
            if (active != null)
            {
                active.Cancel();
                _context.SaveChanges();
            }

            var newGame = new Game
            {
                Profile1Id = p1Id,
                Profile2Id = p2Id,
                IsActive = true,
                StartTime = DateTime.Now
            };

            _context.Games.Add(newGame);
            _context.SaveChanges();
            return newGame;
        }

        public Game? GetActiveGame()
        {
            return _context.Games
                .Include(g => g.Profile1)
                .Include(g => g.Profile2)
                .FirstOrDefault(g => g.IsActive);
        }

        public void FinishGame(int gameId, int winnerId)
        {
            var game = _context.Games.Find(gameId);
            if (game != null)
            {
                game.Finish(winnerId);
                _context.SaveChanges();
            }
        }

        public void ResetGame(int gameId)
        {
            var game = _context.Games.Find(gameId);
            if (game != null)
            {
                game.Cancel();
                _context.SaveChanges();
            }
        }
    }
}