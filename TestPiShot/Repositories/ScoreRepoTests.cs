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
    public void AddScore_ShouldAddScoreToDatabase()
    {
        // Arrange
        int profileId = 10;

        // Act
        _repository.AddScore(profileId);

        // Assert
        var score = _context.Scores.FirstOrDefault();
        Assert.IsNotNull(score, "Score was not added to DB");
        Assert.AreEqual(profileId, score.ProfileId);

        // Check if date is recent (within last second)
        Assert.IsTrue((DateTime.Now - score.ScoredAt.Value).TotalSeconds < 1, "ScoredAt time is incorrect");
    }

    #endregion

    #region AddAttempt Tests

    [TestMethod]
    public void AddAttempt_ShouldAddAttemptToDatabase()
    {
        // Arrange
        int profileId = 5;

        // Act
        _repository.AddAttempt(profileId);

        // Assert
        var attempt = _context.ShotAttempts.FirstOrDefault();
        Assert.IsNotNull(attempt, "ShotAttempt was not added to DB");
        Assert.AreEqual(profileId, attempt.ProfileId);

        // Check if date is recent
        Assert.IsTrue((DateTime.Now - attempt.AttemptedAt).TotalSeconds < 1, "AttemptedAt time is incorrect");
    }

    #endregion

    #region GetCurrentGameEntity Tests

    [TestMethod]
    public void GetCurrentGameEntity_WhenExists_ShouldReturnGameWithId1()
    {
        // Arrange
        var game = new CurrentGame
        {
            Id = 1,
            Player1 = new Profile { Name = "P1" },
            Player2 = new Profile { Name = "P2" }
        };
        _context.CurrentGame.Add(game);
        _context.SaveChanges();

        // Act
        var result = _repository.GetCurrentGameEntity();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("P1", result.Player1.Name);
    }

    [TestMethod]
    public void GetCurrentGameEntity_WhenNoneExists_ShouldReturnNull()
    {
        // Act
        var result = _repository.GetCurrentGameEntity();

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region GetScoresSinceGameStart Tests

    [TestMethod]
    public void GetScoresSinceGameStart_ShouldCountOnlyScoresAfterStartTime()
    {
        // Arrange
        int p1Id = 1;
        int p2Id = 2;
        DateTime gameStart = new DateTime(2023, 1, 1, 12, 0, 0);

        // Seed Data
        _context.Scores.AddRange(
            // P1: 1 score BEFORE game, 2 scores AFTER game
            new Score { ProfileId = p1Id, ScoredAt = gameStart.AddMinutes(-5) },

            new Score { ProfileId = p1Id, ScoredAt = gameStart.AddMinutes(1) },
            new Score { ProfileId = p1Id, ScoredAt = gameStart.AddMinutes(2) },

            // P2: 1 score AFTER game
            new Score { ProfileId = p2Id, ScoredAt = gameStart.AddMinutes(5) }
            );
        _context.SaveChanges();

        // Act
        var result = _repository.GetScoresSinceGameStart(p1Id, p2Id, gameStart);

        // Assert
        Assert.AreEqual(2, result.P1Score, "Player 1 score count is wrong");
        Assert.AreEqual(1, result.P2Score, "Player 2 score count is wrong");
    }

    [TestMethod]
    public void GetScoreSinceGameStart_NoScores_ShouldReturnZero()
    {
        // Arrange
        DateTime gameStart = DateTime.Now;

        // Act
        var result = _repository.GetScoresSinceGameStart(1, 2, gameStart);

        // Assert
        Assert.AreEqual(0, result.P1Score);
        Assert.AreEqual(0, result.P2Score);
    }

    #endregion

    #region UpdateTiebreakStatus Tests

    [TestMethod]
    public void UpdateTiebreakStatus_MatchingPlayers_ShouldUpdateGame()
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
        _context.SaveChanges();

        // Act
        _repository.UpdateTiebreakStatus(p1Id, p2Id);

        // Assert
        var updatedGame = _context.CurrentGame.First();
        Assert.IsTrue(updatedGame.IsTiebreak);
        Assert.AreEqual(5, updatedGame.TiebreakOffsetP1);
        Assert.AreEqual(5, updatedGame.TiebreakOffsetP2);
    }

    [TestMethod]
    public void UpdateTiebreakStatus_WrongPlayerIds_ShouldNotUpdate()
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
        _context.SaveChanges();

        // Act
        _repository.UpdateTiebreakStatus(p1Id, p2Id);

        // Assert
        var dbGame = _context.CurrentGame.First();
        Assert.IsFalse(dbGame.IsTiebreak, "Should not enable tiebreak if Ids don't match");
    }

    [TestMethod]
    public void UpdateTiebreakStatus_GameDoesNotExist_shouldDoNothing()
    {
        // Arrange
        // Database is empty

        // Act
        try
        {
            _repository.UpdateTiebreakStatus(1, 2);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Method threw exception when game null: {ex.Message}");
        }

        // Assert
        Assert.AreEqual(0, _context.CurrentGame.Count());
    }

    #endregion
}
