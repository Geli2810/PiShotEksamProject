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
        //act
        profile.Name = null;
        //assert
        Assert.ThrowsException<ArgumentNullException>(() => profile.Name = null);
    }

    public void ProfileNameCannotBeWhiteSpaces()
    {
        //act
        profile.Name = "   ";
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = " ");
    }

    public void ProfileNameCannotBeEmpty()
    {
        //act
        profile.Name = "";
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = "");
    }


    public void ProfileNameCannotBe1Character()
    {
        //act
        profile.Name = "A";
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = "A");
    }

    public void ProfileNameCannotBe51CharactersOrOver()
    {
        //act
        profile.Name = new string('A', 51);
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = new string('A', 51));
    }

    public void ProfileNameCanBe50Characters()
    {         
        //act
        profile.Name = new string('A', 50);
        //assert
        Assert.AreEqual(new string('A', 50), profile.Name);
    }

    public void ProfileNameCanBe2Characters()
    {
        //act
        profile.Name = "AB";
        //assert
        Assert.AreEqual("AB", profile.Name);
    }


    //ImagePath Tests
    public void TestProfileImagePath()
    {
        Assert.AreEqual("/images/johndoe.png", profile.ProfileImagePath);
    }

    public void TestProfileImagePathCannotBeNull()
    {
        //act
        profile.ProfileImagePath = null;
        //assert
        Assert.ThrowsException<ArgumentNullException>(() => profile.ProfileImagePath = null!);
    }

    public void TestProfileImagePathCannotBeEmpty()
    {
        //act
        profile.ProfileImagePath = "";
        //assert
        Assert.ThrowsException<ArgumentException>(() => profile.ProfileImagePath = "");
    }

    //Constructor Test
    public void TestDefaultProfileConstructor()
    {
        Profile newProfile = new Profile();
        Assert.AreEqual("Default Name", newProfile.Name);
        Assert.AreEqual("default/path/to/image.png", newProfile.ProfileImagePath);
    }

}
