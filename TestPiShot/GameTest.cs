using PiShotProject;

namespace TestPiShot;

[TestClass]
public class GameTest
{
    private readonly Game game = new Game();
    private readonly ProfileRepository profile1;
    private readonly ProfileRepository profile2;

    [TestMethod]
    public void GameWinner()
    {
        var player1 = profile1.AddProfile(new Profile("Jens"));
        var player2 = profile1.AddProfile(new Profile("Jensine"));
    }
}
