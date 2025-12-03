using PiShotProject;

namespace TestPiShot;

[TestClass]
public class GameTest
{
    [TestMethod]
    public void Game_DefaultConstructor()
    {
        var game = new Game();
        Assert.AreEqual(0, game.Id);
        Assert.IsNull(game.Profile1);
        Assert.IsNull(game.Profile2);
        Assert.AreEqual(0, game.GameWinner);
    }

    [TestMethod]
    public void Game_Properties()
    {
        var game = new Game();
        var p1 = new Profile("Jens");
        var p2 = new Profile("Jensine");

        game.Id = 5;
        game.Profile1 = p1;
        game.Profile2 = p2;
        game.GameWinner = 1;

        Assert.AreEqual(5, game.Id);
        Assert.AreEqual(p1, game.Profile1);
        Assert.AreEqual(p2, game.Profile2);
        Assert.AreEqual(1, game.GameWinner);
    }
}
