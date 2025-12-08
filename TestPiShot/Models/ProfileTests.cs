using PiShotProject.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestPiShot.Models;

[TestClass]
public class ProfileTests
{
    [TestMethod]
    public void Constructor_Should_Set_Default_Image_If_Null()
    {
        // Act
        var profile = new Profile("Test User", null);

        // Assert
        Assert.AreEqual(Profile.DefaultProfileImagePath, profile.ProfileImage);
    }

    [TestMethod]
    public void Name_Should_Throw_If_Null_Or_Empty()
    {
        var profile = new Profile();

        Assert.ThrowsException<ArgumentException>(() => profile.Name = "");
        Assert.ThrowsException<ArgumentException>(() => profile.Name = "   ");
        Assert.ThrowsException<ArgumentException>(() => profile.Name = null);
    }

    [TestMethod]
    public void Name_Should_Throw_If_Too_Short()
    {
        var profile = new Profile();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => profile.Name = "A");
    }

    [TestMethod]
    public void Name_Should_Throw_If_Too_Long()
    {
        var profile = new Profile();
        string longName = new string('A', 51);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => profile.Name = longName);
    }

    [TestMethod]
    public void Name_Should_Accept_Valid_Length()
    {
        var profile = new Profile();
        profile.Name = "Bo";
        Assert.AreEqual("Bo", profile.Name);

        profile.Name = new string('A', 50);
        Assert.AreEqual(new string('A', 50), profile.Name);
    }
}