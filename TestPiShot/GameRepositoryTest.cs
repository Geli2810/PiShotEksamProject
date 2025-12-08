using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Models;
using PiShotProject.Repositories;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace TestPiShot;

[TestClass]
public class GameRepositoryTest
{

    private PiShotDBContext _dbContext;
    private GameRepository _gameRepository;
    private Game _game;
    private Profile _p1;
    private Profile _p2;

    [TestInitialize]
    public void Init()
    {
        var options = new DbContextOptionsBuilder<PiShotDBContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())

        .Options;

        _dbContext = new PiShotDBContext(options);

        _gameRepository = new GameRepository(_dbContext);

        _p1 = new Profile("Jens") { Id = 1 };
        _p2 = new Profile("Jensine") { Id = 2 };

        _dbContext.Profiles.Add(_p1);
        _dbContext.Profiles.Add(_p2);
        _dbContext.SaveChanges();

        _game = new Game
        {
            Profile1 = _p1,
            Profile2 = _p2
        };

    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose(); 
    }

    //[TestMethod]
    //public void GameProfilesAreAssignedCorrectly()
    //{
    //    Assert.AreEqual(_p1, _game.Profile1);
    //    Assert.AreEqual(_p2, _game.Profile2);
    //}

    //[TestMethod]
    //public void GameWinner_Player1()
    //{
    //    int score1 = 10;
    //    int score2 = 7;

    //    if (score1 > score2)
    //        _game.GameWinner = _p1.Id;

    //    Assert.AreEqual(_p1.Id, _game.GameWinner);
    //}

    //[TestMethod]
    //public void GameWinner_Player2()
    //{
    //    int score1 = 5;
    //    int score2 = 10;

    //    if (score2 > score1)
    //        _game.GameWinner = _p2.Id;

    //    Assert.AreEqual(_p2.Id, _game.GameWinner);
    //}

    //[TestMethod]
    //public void GetGamesByWinnerProfile()
    //{
    //    var game1 = new Game { Profile1 = _p1, Profile2 = _p2 };
    //    var game2 = new Game { Profile1 = _p2, Profile2 = _p1 };
    //    _gameRepository.AddNewGame(game1);
    //    _gameRepository.AddNewGame(game2);

    //    _gameRepository.SetWinner(game1, 10, 5); 
    //    _gameRepository.SetWinner(game2, 3, 7);  

    //    var gamesWonByP1 = _gameRepository.GetGamesByWinner(_p1.Id);

    //    Assert.AreEqual(2, gamesWonByP1.Count);
    //    Assert.IsTrue(gamesWonByP1.All(g => g.GameWinner == _p1.Id));
    //}

    //[TestMethod]
    //public void GetAllGamesForProfile()
    //{
    //    var game1 = new Game { Profile1 = _p1, Profile2 = _p2 };
    //    var game2 = new Game { Profile1 = _p2, Profile2 = _p1 };

    //    var p3 = new Profile("Extra");
    //    _dbContext.Profiles.Add(p3);
    //    _dbContext.SaveChanges();

    //    var game3 = new Game { Profile1 = _p2, Profile2 = p3 };

    //    _gameRepository.AddNewGame(game1);
    //    _gameRepository.AddNewGame(game2);
    //    _gameRepository.AddNewGame(game3);

    //    var gamesForP1 = _gameRepository.GetGamesForProfile(_p1.Id);

    //    Assert.AreEqual(2, gamesForP1.Count);
    //    Assert.IsTrue(gamesForP1.All(g => g.Profile1.Id == _p1.Id || g.Profile2.Id == _p1.Id));
    //}

}
