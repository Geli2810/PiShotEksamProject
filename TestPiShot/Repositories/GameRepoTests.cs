using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Models;
using PiShotProject.Repositories;

namespace TestPiShot;

[TestClass]
public class GameRepoTests
{
    private PiShotDBContext _context;
    private GameRepository _repository;
    private DbContextOptions<PiShotDBContext> _options;

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<PiShotDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

        _context = new PiShotDBContext(_options);
        _repository = new GameRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetState Tests

    [TestMethod]
    public async Task GetState_WhenGameExists_ShouldReturnGameWithId1()
    {
        // Arrange
        var p1 = new Profile { Name = "P1" };
        var p2 = new Profile { Name = "P2" };
        _context.Profiles.AddRange(p1, p2);
        await _context.SaveChangesAsync();

        var game = new CurrentGame
        {
            Id = 1,
            IsTiebreak = false,
            Player1Id = p1.Id,
            Player2Id = p2.Id
        };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetStateAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
    }

    [TestMethod]
    public async Task GetState_ShouldIncludePlayerNavigationProperties()
    {
        // Arrange
        // We create profiles to ensure the .Include works correctly
        var p1 = new Profile { Id = 10, Name = "Player One" };
        var p2 = new Profile { Id = 20, Name = "Player Two" };

        var game = new CurrentGame
        {
            Id = 1,
            Player1Id = 10,
            Player1 = p1,
            Player2Id = 20,
            Player2 = p2
        };

        _context.Profiles.AddRange(p1, p2);
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetStateAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Player1, "Player1 was not included");
        Assert.IsNotNull(result.Player2, "Player2 was not included");
        Assert.AreEqual("Player One", result.Player1?.Name);
        Assert.AreEqual("Player Two", result.Player2?.Name);
    }

    [TestMethod]
    public async Task GetState_WhenNoGameId1_ShouldReturnNull()
    {
        // Arrange
        // Add a game with a different ID to ensure filter works
        var game = new CurrentGame { Id = 2 };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetStateAsync();

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetState_WhenDbEmpty_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetStateAsync();

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region AddResult Tests

    [TestMethod]
    public async Task AddResult_ShouldAddGameResultToDatabase()
    {
        // Arrange
        // 1. Create dummy profiles to serve as Winner and Loser
        var p1 = new Profile { Name = "Tiger Woods" };
        var p2 = new Profile { Name = "Phil Mickelson" };

        _context.Profiles.AddRange(p1, p2);
        await _context.SaveChangesAsync();

        // 2. Create the result object using the IDs from above
        var resultObj = new GameResult
        {
            WinnerId = p1.Id,
            LoserId = p2.Id,
            PlayedOn = DateTime.Now,
            GameScore = "10-8"
        };

        // Act
        await _repository.AddResultAsync(resultObj);

        // Assert
        Assert.AreEqual(1, await _context.GameResults.CountAsync());

        var storedResult = await _context.GameResults.FirstAsync();
        Assert.AreEqual(p1.Id, storedResult.WinnerId);
        Assert.AreEqual(p2.Id, storedResult.LoserId);
        Assert.AreEqual("10-8", storedResult.GameScore);
    }

    #endregion

    #region SaveState Tests

    [TestMethod]
    public async Task SaveState_ShouldUpdateExistingGame()
    {
        // Arrange
        // 1. Seed the DB with initial state
        var initialGame = new CurrentGame { Id = 1, IsTiebreak = false, TiebreakOffsetP1 = 0 };
        _context.CurrentGame.Add(initialGame);
        await _context.SaveChangesAsync();

        // Detach the entity to simulate the object coming from a service/controller
        // rather than being the exact tracked instance in memory.
        _context.Entry(initialGame).State = EntityState.Detached;

        // 2. Create updated version
        var updatedGame = new CurrentGame { Id = 1, IsTiebreak = true, TiebreakOffsetP1 = 5 };

        // Act
        await _repository.SaveStateAsync(updatedGame);

        // Assert
        var dbGame = await _context.CurrentGame.FirstAsync();
        Assert.IsTrue(dbGame.IsTiebreak, "IsTiebreak was not updated");
        Assert.AreEqual(5, dbGame.TiebreakOffsetP1, "Offset was not updated");
    }

    [TestMethod]
    public async Task SaveState_ShouldPersistChanges()
    {
        // Arrange
        // Testing that SaveChanges is actually called by checking context state
        var game = new CurrentGame { Id = 1 };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        game.TiebreakOffsetP2 = 99;

        // Act
        await _repository.SaveStateAsync(game);

        // Assert
        // We create a NEW context to verify data was actually written to the "DB"
        // and not just sitting in the previous context's change tracker.
        using (var newContext = new PiShotDBContext(_options))
        {
            var savedGame = newContext.CurrentGame.First();
            Assert.AreEqual(99, savedGame.TiebreakOffsetP2);
        }
    }

    #endregion
}
