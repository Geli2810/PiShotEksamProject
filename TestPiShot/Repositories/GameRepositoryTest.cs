using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Models;
using PiShotProject.Repositories;

namespace TestPiShot;

//[TestClass]
//public class GameRepositoryTest
//{
//    private PiShotDBContext _dbContext;
//    private GameRepository _gameRepository;
//    private Profile _p1;
//    private Profile _p2;

//    [TestInitialize]
//    public void Init()
//    {
//        var options = new DbContextOptionsBuilder<PiShotDBContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        _dbContext = new PiShotDBContext(options);
//        _gameRepository = new GameRepository(_dbContext);

//        _p1 = new Profile("Jens") { Id = 1 };
//        _p2 = new Profile("Jensine") { Id = 2 };

//        _dbContext.Profiles.Add(_p1);
//        _dbContext.Profiles.Add(_p2);
//        _dbContext.SaveChanges();
//    }

//    [TestCleanup]
//    public void Cleanup()
//    {
//        _dbContext.Database.EnsureDeleted();
//        _dbContext.Dispose();
//    }

//    [TestMethod]
//    public void StartGame_ShouldCreateNewActiveGame()
//    {
//        var game = _gameRepository.StartGame(_p1.Id, _p2.Id);

//        Assert.IsNotNull(game);
//        Assert.IsTrue(game.IsActive);
//        Assert.AreEqual(_p1.Id, game.Profile1Id);
//        Assert.AreEqual(_p2.Id, game.Profile2Id);
//        Assert.IsNull(game.EndTime);
//    }

//    [TestMethod]
//    public void StartGame_ShouldDeactivatePreviousGame()
//    {
//        _gameRepository.StartGame(_p1.Id, _p2.Id);

//        var game2 = _gameRepository.StartGame(_p1.Id, _p2.Id);

//        var games = _dbContext.Games.ToList();

//        Assert.AreEqual(2, games.Count);
//        Assert.IsFalse(games[0].IsActive);
//        Assert.IsNotNull(games[0].EndTime);
//        Assert.IsTrue(games[1].IsActive);
//    }

//    [TestMethod]
//    public void GetActiveGame_ShouldReturnCurrentGame()
//    {
//        _gameRepository.StartGame(_p1.Id, _p2.Id);

//        var activeGame = _gameRepository.GetActiveGame();

//        Assert.IsNotNull(activeGame);
//        Assert.IsTrue(activeGame.IsActive);
//    }

//    [TestMethod]
//    public void FinishGame_ShouldSetWinnerAndDeactivate()
//    {
//        var game = _gameRepository.StartGame(_p1.Id, _p2.Id);

//        _gameRepository.FinishGame(game.Id, _p1.Id);

//        var finishedGame = _dbContext.Games.Find(game.Id);

//        Assert.IsNotNull(finishedGame);
//        Assert.IsFalse(finishedGame.IsActive);
//        Assert.AreEqual(_p1.Id, finishedGame.WinnerId);
//        Assert.IsNotNull(finishedGame.EndTime);
//    }

//    [TestMethod]
//    public void ResetGame_ShouldDeactivateGame()
//    {
//        var game = _gameRepository.StartGame(_p1.Id, _p2.Id);

//        _gameRepository.ResetGame(game.Id);

//        var resetGame = _dbContext.Games.Find(game.Id);
//        Assert.IsFalse(resetGame.IsActive);
//    }
//}