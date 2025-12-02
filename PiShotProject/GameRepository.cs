using PiShotProject.ClassDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject
{
    public class GameRepository
    {
        private readonly PiShotDBContext _dbContext;
            public GameRepository(PiShotDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public List<Game> GetAllGames()
        {
            return _dbContext.Games.ToList();
        }
        public Game AddGame(Game game)
        {
            _dbContext.Games.Add(game);
            _dbContext.SaveChanges();
            return game;
        }

        public Game? GetGameById(int id)
        {
            return _dbContext.Games.FirstOrDefault(p => p.Id == id);
        }

        public int AddGameWinner(Game game,int id)
        {
            return game.GameWinner;
        }
    }
}
