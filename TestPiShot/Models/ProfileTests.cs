using PiShotProject.Models;

namespace TestPiShot;

[TestClass]
public class ProfileTests
{
    [TestMethod]
    public void Constructor_Defalut_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var profile = new Profile();

        // Assert
        Assert.AreEqual("Default Name", profile.Name);
        Assert.AreEqual(Profile.DefaultProfileImagePath, profile.ProfileImage);
    }

    [TestMethod]
    public void Constructor_WithParameters_ValidValues_ShoutSetProperties()
    {
        // Arrange
        string expectedName = "Jordan";
        string expectedImage = "custom-image.jpg";

        // Act
        var profile = new Profile(expectedName, expectedImage);

        // Assert
        Assert.AreEqual(expectedName, profile.Name);
        Assert.AreEqual(expectedImage, profile.ProfileImage);
    }

    [TestMethod]
    public void Constructor_Parameterized_NullImage_ShouldUseDefaultImage()
    {
        // Arrange
        string name = "Alice";
        string? image = null;

        // Act
        var profile = new Profile(name, image);

        // Assert
        Assert.AreEqual(Profile.DefaultProfileImagePath, profile.ProfileImage);
    }

    [TestMethod]
    public void Id_SetAndGet_ShouldWorkCorrectly()
    {
        // Arrange
        var profile = new Profile();
        int expectedId = 42;

        // Act
        profile.Id = expectedId;

        // Assert
        Assert.AreEqual(expectedId, profile.Id);
    }

    #region Name Property Tests

    [TestMethod]
    public void Name_Set_ValidValue_ShouldUpdateProperty()
    {
        // Arrange
        var profile = new Profile();
        string newName = "Kobe";

        // Act
        profile.Name = newName;

        // Assert
        Assert.AreEqual(newName, profile.Name);
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(25)]
    [DataRow(50)]
    public void Name_Set_ValidLengths_ShouldSucceed(int length)
    {
        // Arrange
        var profile = new Profile();
        string name = new string('a', length);

        // Act
        profile.Name = name;

        // Assert
        Assert.AreEqual(name, profile.Name);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Name_Set_NullOrWhitespace_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var profile = new Profile();

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => profile.Name = invalidName);
    }

    [TestMethod]
    public void Name_Set_TooShort_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var profile = new Profile();
        string shortName = "A";

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => profile.Name = shortName);
        StringAssert.Contains(ex.Message, "between 2 and 50");
    }

    [TestMethod]
    public void Name_Set_TooLong_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var profile = new Profile();
        string longName = new string('A', 51);

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => profile.Name = longName);
        StringAssert.Contains(ex.Message, "between 2 and 50");
    }

    #endregion

    #region ProfileImage Property Tests

    [TestMethod]
    public void ProfileImage_Set_ValidString_ShouldUpdateProperty()
    {
        // Arrange
        var profile = new Profile();
        string newImage = "image.jpg";

        // Act
        profile.ProfileImage = newImage;

        // Assert
        Assert.AreEqual(newImage, profile.ProfileImage);
    }

    [TestMethod]
    public void ProfileImage_Set_Null_ShouldRevertToDefault()
    {
        // Arrange
        var profile = new Profile("ValidName", "original.jpg");

        // Act
        profile.ProfileImage = null;

        // Assert
        Assert.AreEqual(Profile.DefaultProfileImagePath, profile.ProfileImage);
    }

    [TestMethod]
    public void ProfileImage_Set_EmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var profile = new Profile();
        string emptyImage = "";

        // Act
        profile.ProfileImage = emptyImage;

        // Assert
        Assert.AreEqual(emptyImage, profile.ProfileImage);
    }

    #endregion

    [TestMethod]
    public void ToString_ShouldReturnName()
    {
        // Arrange
        string name = "LeBron";
        string image = "lebron.jpg";
        var profile = new Profile(name, image);
        string expected = $"Profile: {name}, Image Path: {image}";

        // Act
        string result = profile.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }
}
