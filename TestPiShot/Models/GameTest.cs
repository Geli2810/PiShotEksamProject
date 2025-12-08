using PiShotProject.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestPiShot.Models;
[TestClass]
public class GameTest
{
    [TestMethod]
    public void Finish_Should_Set_Winner_And_Deactivate()
    {
        // Arrange
        var game = new Game { Id = 1, Profile1Id = 10, Profile2Id = 20, IsActive = true };

        // Act
        game.Finish(10);

        // Assert
        Assert.IsFalse(game.IsActive, "Spillet skal stoppe (IsActive = false)");
        Assert.AreEqual(10, game.WinnerId, "Vinder ID skal være sat");
        Assert.IsNotNull(game.EndTime, "Sluttidspunkt skal være registreret");
    }

    [TestMethod]
    public void Finish_Should_Throw_If_Winner_Is_Not_Player()
    {
        var game = new Game { Id = 1, Profile1Id = 10, Profile2Id = 20, IsActive = true };
        Assert.ThrowsException<ArgumentException>(() => game.Finish(99));
    }

    [TestMethod]
    public void Cancel_Should_Deactivate_Without_Winner()
    {
        // Arrange
        var game = new Game { IsActive = true };

        // Act
        game.Cancel();

        // Assert
        Assert.IsFalse(game.IsActive);
        Assert.IsNull(game.WinnerId, "Der må ikke være en vinder ved afbrydelse");
        Assert.IsNotNull(game.EndTime);
    }
}