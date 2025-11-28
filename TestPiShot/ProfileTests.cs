using Microsoft.Identity.Client;
using PiShotProject;

namespace TestPiShot;

[TestClass]
public class ProfileTests
{
    private readonly Profile profile = new Profile("John Doe", "/images/johndoe.png");


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
        Assert.AreEqual("/images/johndoe.png", profile.ProfileImagePath);
    }

    [TestMethod]
    public void TestProfileImagePathCannotBeNull()
    {
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.ProfileImagePath = null!);
    }

    [TestMethod]
    public void TestProfileImagePathCannotBeEmpty()
    {
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.ProfileImagePath = "");
    }

    //Constructor Test'
    [TestMethod]
    public void TestDefaultProfileConstructor()
    {
        Profile newProfile = new Profile();
        Assert.AreEqual("Default Name", newProfile.Name);
        Assert.AreEqual("default/path/to/image.png", newProfile.ProfileImagePath);
    }

}
