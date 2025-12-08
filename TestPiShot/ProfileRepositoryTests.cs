using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Models;
using PiShotProject.Repositories;

namespace TestPiShot;

[TestClass]
public class ProfileRepositoryTests
{
    private ProfileRepository _repo;
    private PiShotDBContext _dbContext;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<PiShotDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // 2. Create the DbContext and Repository
        _dbContext = new PiShotDBContext(options);
        _dbContext.Database.EnsureCreated();

        _repo = new ProfileRepository(_dbContext);

        // 3. Seed the database with initial data
        var profiles = new List<Profile>
        {
            new Profile { Id = 1, Name = "Bob", ProfileImage = "Bob.png" },
            new Profile { Id = 2, Name = "Alice", ProfileImage = "Alice.png" }
        };

        _dbContext.Profiles.AddRange(profiles);
        _dbContext.SaveChanges();
    }

    [TestMethod]
    public void GetAllTest()
    {
        IEnumerable<Profile> profile = _repo.GetAllProfiles();
        Assert.AreEqual(1, profile.First().Id);
        Assert.AreEqual("Bob", profile.First().Name);
        Assert.AreEqual("Bob.png", profile.First().ProfileImage);

        Assert.AreEqual(2, profile.Count());
    }

    [TestMethod]
    public void AddTest()
    {
        _repo.AddProfile(new Profile { Name = "Bobob", ProfileImage = "Bobob.png" });
        IEnumerable<Profile> allProfile = _repo.GetAllProfiles();
        Assert.AreEqual(3, allProfile.Count());
    }

    [TestMethod]
    public void AddExceptionTest()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _repo.AddProfile(new Profile { Name = "B", ProfileImage = "path.ipatj"}));
        Assert.ThrowsException<ArgumentException>(() => _repo.AddProfile(new Profile { Name = null, ProfileImage = "path.ipatj" }));
    }

    [TestMethod]
    public void GetByIdTest()
    {
        var p = _repo.AddProfile(new Profile { Name = "Bob", ProfileImage = "Bob.png" });
        Profile? profile = _repo.GetProfileById(p.Id);
        Assert.IsNotNull(profile);
        Assert.IsNull(_repo.GetProfileById(-1));
    }

    [TestMethod]
    public void UpdateTest()
    {
        Profile p = _repo.AddProfile(new Profile { Name = "Bøg", ProfileImage = "Bøg.png"});
        Profile? profile = _repo.UpdateProfile(new Profile { Name = "Bok", ProfileImage = "Bok.png"}, p.Id);
        Assert.IsNotNull(profile);
        Profile? profile2 = _repo.GetProfileById(profile.Id);
        Assert.AreEqual("Bok", profile2.Name);

        Assert.IsNull(_repo.UpdateProfile(new Profile { Name = "Buk", ProfileImage = "Buk.png" }, -1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _repo.UpdateProfile(new Profile { Name = "a", ProfileImage = "aaa.png"}, profile2.Id));
    }

    [TestMethod]
    public void DeleteTest()
    {
        Profile p = _repo.AddProfile(new Profile { Name = "Jens AI", ProfileImage = "JensAI.png"});
        Assert.IsNotNull(p);
        Assert.AreEqual("Jens AI", p.Name);
        Assert.AreEqual(3, p.Id);

        Profile? profile = _repo.DeleteProfile(p.Id);
        var pro = _repo.GetProfileById(3);
        Assert.IsNull(pro);
    }
}
