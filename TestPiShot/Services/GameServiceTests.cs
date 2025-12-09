using Moq;
using PiShotProject.Interfaces;
using PiShotProject.Models;
using PiShotProject.Services;

namespace TestPiShot;

[TestClass]
public class GameServiceTests
{
    private Mock<IGameRepository> _mockRepo;
    private GameService _service;

    [TestInitialize]
    public void Setup()
    {
        // Create a fresh mock and service before every test
        _mockRepo = new Mock<IGameRepository>();
        _service = new GameService(_mockRepo.Object);
    }

    #region StartNewGame Tests

    [TestMethod]
    public void StartNewGame_ShouldInitializeGameAndSave()
    {
        // Arrange
        var request = new StartGameRequest { Player1Id = 10, Player2Id = 20 };

        // Simulate existing state being null (first game) or existing
        _mockRepo.Setup(r => r.GetState()).Returns((CurrentGame?)null);

        // Act
        _service.StartNewGame(request);

        // Assert
        _mockRepo.Verify(r => r.SaveState(It.Is<CurrentGame>(g =>
            g.Player1Id == 10 &&
            g.Player2Id == 20 &&
            g.IsActive == true &&
            g.StartTime != null &&
            g.IsTiebreak == false
        )), Times.Once);
    }

    #endregion

    #region DeclareWinner Tests

    [TestMethod]
    public void DeclareWinner_WhenGameActive_ShouldUpdateWinnerAndSave()
    {
        // Arrange
        var activeGame = new CurrentGame { Id = 1, IsActive = true, Player1Id = 1, Player2Id = 2 };
        _mockRepo.Setup(r => r.GetState()).Returns(activeGame);

        // Act
        _service.DeclareWinner(1);

        // Assert
        Assert.AreEqual(1, activeGame.CurrentWinnerId);
        _mockRepo.Verify(r => r.SaveState(activeGame), Times.Once);
    }

    [TestMethod]
    public void DeclareWinner_WhenGameInactive_ShouldDoNothing()
    {
        // Arrange
        var inactiveGame = new CurrentGame { Id = 1, IsActive = false };
        _mockRepo.Setup(r => r.GetState()).Returns(inactiveGame);

        // Act
        _service.DeclareWinner(1);

        // Assert
        _mockRepo.Verify(r => r.SaveState(It.IsAny<CurrentGame>()), Times.Never);
    }

    #endregion

    #region GetCurrentStatus Tests

    [TestMethod]
    public void GetCurrentStatus_NoGame_ShouldReturnInactiveStatus()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetState()).Returns((CurrentGame?)null);

        // Act
        var result = _service.GetCurrentStatus();

        // Assert
        Assert.IsFalse(result.IsActive);
        Assert.AreEqual(0, result.Player1Id);
        Assert.IsNull(result.CurrentWinnerId);
    }

    [TestMethod]
    public void GetCurrentStatus_ActiveGame_NoWinner_ShouldReturnStatus()
    {
        // Arrange
        var game = new CurrentGame { IsActive = true, Player1Id = 10, Player2Id = 20, CurrentWinnerId = null };
        _mockRepo.Setup(r => r.GetState()).Returns(game);

        // Act
        var result = _service.GetCurrentStatus();

        // Assert
        Assert.IsTrue(result.IsActive);
        Assert.AreEqual(10, result.Player1Id);
        Assert.IsNull(result.WinnerName);
    }

    [TestMethod]
    public void GetCurrentStatus_WinnerIsPlayer1_ShouldReturnP1etails()
    {
        // Arrange
        var p1 = new Profile { Id = 10, Name = "Alice", ProfileImage = "alice.jpg" };
        var p2 = new Profile { Id = 20, Name = "Bob" };

        var game = new CurrentGame
        {
            IsActive = true,
            Player1Id = 10,
            Player2Id = 20,
            CurrentWinnerId = 10,
            Player1 = p1,
            Player2 = p2
        };

        _mockRepo.Setup(r => r.GetState()).Returns(game);

        // Act
        var result = _service.GetCurrentStatus();

        // Assert
        Assert.AreEqual(10, result.CurrentWinnerId);
        Assert.AreEqual("Alice", result.WinnerName);
        Assert.AreEqual("alice.jpg", result.WinnerImage);
    }

    [TestMethod]
    public void GetCurrentStatus_WinnerIsPlayer2_ShouldReturnP2Details()
    {
        // Arrange
        var p1 = new Profile { Id = 10, Name = "Alice" };
        var p2 = new Profile { Id = 20, Name = "Bob", ProfileImage = "bob.jpg" };
        
        var game = new CurrentGame
        {
            IsActive = true,
            Player1Id = 10,
            Player2Id = 20,
            CurrentWinnerId = 20,
            Player1 = p1,
            Player2 = p2
        };

        _mockRepo.Setup(r => r.GetState()).Returns(game);
        
        // Act
        var result = _service.GetCurrentStatus();
        
        // Assert
        Assert.AreEqual(20, result.CurrentWinnerId);
        Assert.AreEqual("Bob", result.WinnerName);
        Assert.AreEqual("bob.jpg", result.WinnerImage);
    }

    #endregion

    #region RecordGameResult Tests

    [TestMethod]
    public void RecordGameResult_ShouldAddResultAndResetGame()
    {
        // Arrange
        int p1Id = 10;
        int p2Id = 20;
        var game = new CurrentGame { IsActive = true, Player1Id = p1Id, Player2Id = p2Id };

        _mockRepo.Setup(r => r.GetState()).Returns(game);

        // Act
        _service.RecordGameResult(p1Id); // P1 wins

        // Assert
        // 1. Verify Result was added correctly
        _mockRepo.Verify(r => r.AddResult(It.Is<GameResult>(res =>
            res.WinnerId == p1Id &&
            res.LoserId == p2Id &&
            (DateTime.UtcNow - res.PlayedOn).TotalSeconds < 5
        )), Times.Once);

        // 2. Verify Game state was reset and saved
        Assert.IsFalse(game.IsActive);
        Assert.IsNull(game.StartTime);
        Assert.IsNull(game.CurrentWinnerId);
        _mockRepo.Verify(r => r.SaveState(game), Times.Once);
    }

    [TestMethod]
    public void RecordGameResult_NoActiveGame_ShouldThrowException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetState()).Returns((CurrentGame?)null);

        // Act & Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(() => _service.RecordGameResult(1));
        Assert.AreEqual("No active game found", ex.Message);
    }

    [TestMethod]
    public void RecordGameResult_GameInactive_ShouldThrowException()
    {
        // Arrange
        var game = new CurrentGame { IsActive = false };
        _mockRepo.Setup(r => r.GetState()).Returns(game);

        // Act & Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(() => _service.RecordGameResult(1));
        Assert.AreEqual("No active game found", ex.Message);
    }

    #endregion

    #region StopCurrentGame Tests

    [TestMethod]
    public void StopCurrentGame_ShouldResetStateAndSave()
    {
        // Arrange
        var game = new CurrentGame
        {
            IsActive = true,
            StartTime = DateTime.Now,
            IsTiebreak = true,
            CurrentWinnerId = 5
        };
        _mockRepo.Setup(r => r.GetState()).Returns(game);

        // Act
        _service.StopCurrentGame();

        // Assert
        Assert.IsFalse(game.IsActive);
        Assert.IsNull(game.StartTime);
        Assert.IsNull(game.CurrentWinnerId);
        Assert.IsFalse(game.IsTiebreak);

        _mockRepo.Verify(r => r.SaveState(game), Times.Once);
    }

    [TestMethod]
    public void StopCurrentGame_IfNoGame_ShouldDoNothing()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetState()).Returns((CurrentGame?)null);
        
        // Act
        _service.StopCurrentGame();
        
        // Assert
        _mockRepo.Verify(r => r.SaveState(It.IsAny<CurrentGame>()), Times.Never);
    }

    #endregion
}
