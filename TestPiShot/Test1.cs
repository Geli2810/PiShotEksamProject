using PiShotProject;

namespace TestPiShot
{
    [TestClass]
    public sealed class Test1
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Class1Repo repo = new Class1Repo();
        }

        [TestMethod]
        public void TestMethod1()
        {
            Class1 class1 = new Class1();
            Assert.ThrowsException<ArgumentException>(() => class1.Day = 32);

            Assert.ThrowsException<ArgumentException>(() => class1.Month = 25);

            Assert.ThrowsException<ArgumentException>(() => class1.Year = -1);


        }


        [TestMethod]
        public void TestMethod2()
        {
            Class1 class1 = new Class1();
            class1.Day = 1;
            class1.Month = 1;
            class1.Year = 2023;
            Assert.AreEqual(1, class1.Day);
            Assert.AreEqual(1, class1.Month);
            Assert.AreEqual(2023, class1.Year);
        }

        [TestMethod]
        public void TestMethod3()
        {
            Class1Repo repo = new Class1Repo();
            Class1 class1 = new Class1(2022, 12, 31);
            repo.Add(class1);
        }

        [TestMethod]
        public void TestMethod4()
        {
            Class1Repo repo = new Class1Repo();
            var result = repo.GetAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);

        }

    }
}
