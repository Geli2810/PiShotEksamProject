using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Interfaces;
using PiShotProject.Models;

namespace PiShotProject.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly PiShotDBContext _context;

        public GameRepository(PiShotDBContext context)
        {
            _context = context;
        }

        public CurrentGame? GetState()
        {
            // Loader spiller-navigationer så vi kan finde WinnerName/WinnerImage
            return _context.CurrentGame
                .Include(g => g.Player1)
                .Include(g => g.Player2)
                .FirstOrDefault(g => g.Id == 1);
        }

        public void AddResult(GameResult result)
        {
            _context.GameResults.Add(result);
            _context.SaveChanges();
        }

        public void SaveState(CurrentGame game)
        {
            _context.CurrentGame.Update(game);
            _context.SaveChanges();
        }
    }
}
