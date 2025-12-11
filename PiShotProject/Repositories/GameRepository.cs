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

        public async Task<CurrentGame?> GetStateAsync()
        {
            // Loader spiller-navigationer så vi kan finde WinnerName/WinnerImage
            return await _context.CurrentGame
                .Include(g => g.Player1)
                .Include(g => g.Player2)
                .FirstOrDefaultAsync(g => g.Id == 1);
        }

        public async Task AddResultAsync(GameResult result)
        {
            await _context.GameResults.AddAsync(result);
            await _context.SaveChangesAsync();
        }

        public async Task SaveStateAsync(CurrentGame game)
        {
            _context.CurrentGame.Update(game);
            await _context.SaveChangesAsync();
        }
    }
}
