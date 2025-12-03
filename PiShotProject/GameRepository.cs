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
        
        public Game AddNewGame(Game game)
        {
            _dbContext.Games.Add(game);
            _dbContext.SaveChanges();
            return game;
        }
        public List<Game> GetAllGames()
        {
            return _dbContext.Games.ToList();
        }
        public Game? GetGameById(int id)
        {
            return _dbContext.Games.FirstOrDefault(p => p.Id == id);
        }

        public Game SetWinner(Game game, int score1, int score2)
        {
            if (score1 == score2)
                throw new InvalidOperationException("Tie is not allowed");

            game.GameWinner = score1 > score2
                ? game.Profile1!.Id
                : game.Profile2!.Id;

            _dbContext.Games.Update(game);
            _dbContext.SaveChanges();

            return game;
        }

        public List<Game> GetGamesByWinner(int profileId)
        {
            return _dbContext.Games
                .Where(g => g.GameWinner == profileId)
                .ToList();
        }

        public List<Game> GetGamesForProfile(int profileId)
        {
            return _dbContext.Games
                .Where(g => g.Profile1!.Id == profileId || g.Profile2!.Id == profileId)
                .ToList();
        }
    }
}
