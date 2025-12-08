using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;
using PiShotProject.ClassDB;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Repositories
{
    public class GameRepository : IGameRepostitory
    {
        private readonly PiShotDBContext _context;

        public GameRepository(PiShotDBContext context)
        {
            _context = context;
        }

        public CurrentGame? GetState()
        {
            return _context.CurrentGame.Include(g => g.Winner).FirstOrDefault(g => g.Id == 1);
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
