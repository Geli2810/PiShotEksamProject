using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PiShotProject.ClassDB;
using PiShotProject.Models;
using PiShotProject.Repositories;
using System;
using System.Linq;

namespace TestPiShot;

//[TestClass]
//public class ProfileRepositoryTests
//{
//    private ProfileRepository _repo;
//    private PiShotDBContext _dbContext;

//    [TestInitialize]
//    public void Setup()
//    {
//        var options = new DbContextOptionsBuilder<PiShotDBContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        _dbContext = new PiShotDBContext(options);
//        _repo = new ProfileRepository(_dbContext);

//        // Seed data til stats test
//        var p1 = new Profile { Id = 1, Name = "Shooter 1" };
//        var p2 = new Profile { Id = 2, Name = "Shooter 2" };
//        _dbContext.Profiles.AddRange(p1, p2);

//        var game1 = new Game { Id = 1, Profile1Id = 1, Profile2Id = 2, WinnerId = 1, IsActive = false };
//        _dbContext.Games.Add(game1);

//        _dbContext.Scores.Add(new Score { ProfileId = 1, GameId = 1 });
//        _dbContext.Scores.Add(new Score { ProfileId = 1, GameId = 1 });
//        _dbContext.ShotAttempts.Add(new ShotAttempt { ProfileId = 1, GameId = 1 });
//        _dbContext.ShotAttempts.Add(new ShotAttempt { ProfileId = 1, GameId = 1 });
//        _dbContext.ShotAttempts.Add(new ShotAttempt { ProfileId = 1, GameId = 1 });

//        _dbContext.SaveChanges();
//    }

//    [TestCleanup]
//    public void Cleanup()
//    {
//        _dbContext.Database.EnsureDeleted();
//        _dbContext.Dispose();
//    }

//    [TestMethod]
//    public void GetAllProfilesWithStats_Calculates_Correctly()
//    {
//        var stats = _repo.GetAllProfilesWithStats();
//        var p1Stats = stats.First(p => p.Id == 1);
//        var p2Stats = stats.First(p => p.Id == 2);

//        Assert.AreEqual(2, p1Stats.Goals);
//        Assert.AreEqual(3, p1Stats.Attempts);
//        Assert.AreEqual(66.7, p1Stats.Accuracy);
//        Assert.AreEqual(1, p1Stats.Wins);
//        Assert.AreEqual(1, p2Stats.Losses);
//    }

//    [TestMethod]
//    public void AddTest()
//    {
//        _repo.AddProfile(new Profile { Name = "Bobob" });
//        Assert.AreEqual(3, _repo.GetAllProfiles().Count());
//    }
//}