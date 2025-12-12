using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Models;
using PiShotProject.Repositories;

namespace TestPiShot;

[TestClass]
public class ScoreRepoTests
{
    private PiShotDBContext _context;
    private ScoreRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        // Create a unique In-Memory database for each test to ensure isolation
        var options = new DbContextOptionsBuilder<PiShotDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

        _context = new PiShotDBContext(options);
        _repository = new ScoreRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AddScore Tests

    [TestMethod]
    public async Task AddScore_ShouldAddScoreToDatabase()
    {
        // Arrange
        int profileId = 10;

        var game = new CurrentGame { Id = 1, StartTime = DateTime.UtcNow, Player1Id = 10, Player2Id = 20, IsActive = true };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        var preExistingAttempt = new ShotAttempt
        {
            ProfileId = profileId,
            AttemptedAt = DateTime.UtcNow
        };
        await _context.ShotAttempts.AddAsync(preExistingAttempt);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AddScoreAsync(profileId);

        // Assert
        var score = await _context.Scores.FirstOrDefaultAsync();
        Assert.IsNotNull(score, "Score was not added to DB");
        Assert.AreEqual(profileId, score.ProfileId);

        // Check if date is recent (within last second)
        Assert.IsTrue((DateTime.UtcNow - score.ScoredAt.Value).TotalSeconds < 1, "ScoredAt time is incorrect");
    }

    #endregion

    #region AddAttempt Tests

    [TestMethod]
    public async Task AddAttempt_ShouldAddAttemptToDatabase()
    {
        // Arrange
        int profileId = 5;

        var game = new CurrentGame { Id = 1, StartTime = DateTime.UtcNow.AddSeconds(-1), Player1Id = 5, Player2Id = 6, IsActive = true };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AddAttemptAsync(profileId);

        // Assert
        var attempt = await _context.ShotAttempts.FirstOrDefaultAsync();
        Assert.IsNotNull(attempt, "ShotAttempt was not added to DB");
        Assert.AreEqual(profileId, attempt.ProfileId);

        // Check if date is recent
        Assert.IsTrue((DateTime.UtcNow - attempt.AttemptedAt).TotalSeconds < 1, "AttemptedAt time is incorrect");
    }

    #endregion

    #region GetCurrentGameEntity Tests

    [TestMethod]
    public async Task GetCurrentGameEntity_WhenExists_ShouldReturnGameWithId1()
    {
        // Arrange
        var game = new CurrentGame
        {
            Id = 1,
            Player1 = new Profile { Name = "P1" },
            Player2 = new Profile { Name = "P2" }
        };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentGameEntityAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("P1", result.Player1.Name);
    }

    [TestMethod]
    public async Task GetCurrentGameEntity_WhenNoneExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetCurrentGameEntityAsync();

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region UpdateTiebreakStatus Tests

    [TestMethod]
    public async Task UpdateTiebreakStatus_MatchingPlayers_ShouldUpdateGame()
    {
        // Arrange
        int p1Id = 10;
        int p2Id = 20;
        var game = new CurrentGame
        {
            Id = 1,
            Player1Id = p1Id,
            Player2Id = p2Id,
            IsTiebreak = false
        };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateTiebreakStatusAsync(p1Id, p2Id);

        // Assert
        var updatedGame = await _context.CurrentGame.FirstAsync();
        Assert.IsTrue(updatedGame.IsTiebreak);
        Assert.AreEqual(5, updatedGame.TiebreakOffsetP1);
        Assert.AreEqual(5, updatedGame.TiebreakOffsetP2);
    }

    [TestMethod]
    public async Task UpdateTiebreakStatus_WrongPlayerIds_ShouldNotUpdate()
    {
        // Arrange
        int p1Id = 10;
        int p2Id = 20;
        var game = new CurrentGame
        {
            Id = 1,
            Player1Id = 999,
            Player2Id = p2Id,
            IsTiebreak = false
        };
        _context.CurrentGame.Add(game);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateTiebreakStatusAsync(p1Id, p2Id);

        // Assert
        var dbGame = await _context.CurrentGame.FirstAsync();
        Assert.IsFalse(dbGame.IsTiebreak, "Should not enable tiebreak if Ids don't match");
    }

    [TestMethod]
    public async Task UpdateTiebreakStatus_GameDoesNotExist_shouldDoNothing()
    {
        // Arrange
        // Database is empty

        // Act
        try
        {
            await _repository.UpdateTiebreakStatusAsync(1, 2);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Method threw exception when game null: {ex.Message}");
        }

        // Assert
        Assert.AreEqual(0, await _context.CurrentGame.CountAsync());
    }

    #endregion
}
