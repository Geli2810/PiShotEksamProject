using Microsoft.Identity.Client;
using PiShotProject.Models;

namespace TestPiShot;

[TestClass]
public class ProfileTests
{
    private readonly Profile profile = new Profile("John Doe", "https://st.depositphotos.com/1536130/60618/v/1600/depositphotos_606180794-stock-illustration-basketball-player-hand-drawn-line.jpg");


    //Name Tests
    [TestMethod]
    public void TestProfileName()
    {
        Assert.AreEqual("John Doe", profile.Name);
    }

    [TestMethod]
    public void ProfileNameCannotBeNull()
    {
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = null);
    }

    [TestMethod]
    public void ProfileNameCannotBeWhiteSpaces()
    {
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = " ");
    }
    [TestMethod]
    public void ProfileNameCannotBeEmpty()
    {
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = "");
    }

    [TestMethod]
    public void ProfileNameCannotBe1Character()
    {
        //assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => profile.Name = "A");
    }
    [TestMethod]
    public void ProfileNameCannotBe51CharactersOrOver()
    {
        //assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => profile.Name = new string('A', 51));
    }
    [TestMethod]
    public void ProfileNameCanBe50Characters()
    {         
        //act
        profile.Name = new string('A', 50);
        //assert
        Assert.AreEqual(new string('A', 50), profile.Name);
    }
    [TestMethod]
    public void ProfileNameCanBe2Characters()
    {
        //act
        profile.Name = "AB";
        //assert
        Assert.AreEqual("AB", profile.Name);
    }


    //ImagePath Tests
    [TestMethod]
    public void TestProfileImagePath()
    {
        Assert.AreEqual("https://st.depositphotos.com/1536130/60618/v/1600/depositphotos_606180794-stock-illustration-basketball-player-hand-drawn-line.jpg", profile.ProfileImagePath);
    }

    //Constructor Test'
    [TestMethod]
    public void TestDefaultProfileConstructor()
    {
        Profile newProfile = new Profile();
        Assert.AreEqual("Default Name", newProfile.Name);
        Assert.AreEqual("https://st.depositphotos.com/1536130/60618/v/1600/depositphotos_606180794-stock-illustration-basketball-player-hand-drawn-line.jpg", newProfile.ProfileImagePath);
    }

}
