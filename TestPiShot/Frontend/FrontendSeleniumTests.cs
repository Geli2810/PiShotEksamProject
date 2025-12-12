using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace PiShotTests
{
    [TestClass]
    public class PiShotArenaTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private const string _url = "https://white-cliff-00bf1d610.3.azurestaticapps.net/";

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _driver.Navigate().GoToUrl(_url);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("app")));
        }

        [TestCleanup]
        public void TearDown()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        // Test 1: Profile Management (Create, View, Edit, Delete)
        [TestMethod]
        public void ProfileLifecycle_CreateViewEditDelete()
        {
            // 1. Create a Profile
            string originalName = "Lifecycle_User_" + new Random().Next(1000);
            CreateTempPlayer(originalName);

            // 2. Open "View Profile" Modal
            var viewBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath($"//div[@class='profile-manage-card' and .//h4[text()='{originalName}']]//button")
            ));
            viewBtn.Click();

            // Verify View Modal Opened
            _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("stats-detail-grid")));

            // 3. Go to "Edit Profile" from View Modal
            _driver.FindElement(By.XPath("//button[contains(., 'Edit Profile')]")).Click();

            // Verify Edit Modal Opened
            var nameInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='modal-box']//input")));

            // 4. Change Name
            string newName = originalName + "_Edited";
            nameInput.Clear();
            nameInput.SendKeys(newName);

            // Click Save
            _driver.FindElement(By.XPath("//button[contains(., 'Save Changes')]")).Click();

            // 5. Verify Name Change in Grid
            // Wait for old name to disappear and new name to appear
            _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath($"//h4[text()='{originalName}']")));
            _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//h4[text()='{newName}']")));

            // 6. Delete Profile
            // Open View Modal again for the NEW name
            var viewBtnNew = _driver.FindElement(By.XPath($"//div[@class='profile-manage-card' and .//h4[text()='{newName}']]//button"));
            viewBtnNew.Click();

            // Click Delete
            var deleteBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(., 'Delete')]")));
            deleteBtn.Click();

            // Confirm JS Alert
            var alert = _wait.Until(ExpectedConditions.AlertIsPresent());
            alert.Accept();

            // 7. Verify Gone
            _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath($"//h4[text()='{newName}']")));

            TestContext.WriteLine("Profile Lifecycle (Create -> Edit -> Delete) Passed");
        }

        // Test 2: Leaderboard & NBA Twin Logic
        [TestMethod]
        public void Leaderboard_CheckNbaTwinFeature()
        {
            // Ensure we have a player to select
            string playerName = "StatsTester";
            CreateTempPlayer(playerName);

            // 1. Navigate to Leaderboard
            _driver.FindElement(By.XPath("//button[contains(., 'Leaderboard')]")).Click();

            // 2. Find the Dropdown to select a player
            var dropdownElement = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".stats-control select")));
            var select = new SelectElement(dropdownElement);

            // 3. Select our player
            select.SelectByText(playerName);

            // 4. Verify "NBA MATCH" Card appears
            var nbaCard = _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("nba-card")));

            // Verify content inside NBA card
            string nbaText = nbaCard.Text;
            Assert.IsTrue(nbaText.Contains("NBA MATCH"), "NBA Card header missing");
            Assert.IsTrue(nbaText.Contains("% ACC"), "Accuracy percentage missing in NBA card");

            TestContext.WriteLine("Leaderboard and NBA Twin calculation Passed");
        }

        // Test 3: Full Game - First to 5 wins
        [TestMethod]
        public void Game_FirstToFive_ShouldTriggerWinModal()
        {
            // Prep
            string p1 = "Winner_" + new Random().Next(100);
            string p2 = "Loser_" + new Random().Next(100);
            CreateTempPlayer(p1);
            CreateTempPlayer(p2);

            // Start Game
            _driver.FindElement(By.XPath("//button[contains(., 'Lobby')]")).Click();

            var selects = _wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.TagName("select")));
            new SelectElement(selects[0]).SelectByText(p1);
            new SelectElement(selects[1]).SelectByText(p2);
            _driver.FindElement(By.CssSelector(".start-btn")).Click();

            // Wait for Game View
            _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("game-view")));

            // Identify P1 Goal Button
            var p1GoalBtn = _driver.FindElement(By.XPath("(//div[contains(@class,'control-column')])[1]//button[contains(., 'Goal')]"));
            var p1ScoreDisplay = _driver.FindElement(By.CssSelector(".score-card.p1 .score-digit"));

            // Score 5 Goals
            for (int i = 1; i <= 5; i++)
            {
                p1GoalBtn.Click();
                // Wait for score to update to 'i' before clicking again
                // This prevents clicking faster than the API can process
                _wait.Until(d => p1ScoreDisplay.Text == i.ToString());
            }

            // Verify winner modal
            // Once score hits 5, your fetchLiveScores() logic triggers autoDeclareWinner
            // and the .winner-modal should appear
            var winnerModal = _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("winner-modal")));

            Assert.IsTrue(winnerModal.Text.Contains($"{p1} WINS!"), "Winner modal did not show correct name");

            // Click Record Result
            var recordBtn = winnerModal.FindElement(By.TagName("button"));
            recordBtn.Click();

            // Verify back to Lobby
            _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("match-setup-card")));

            TestContext.WriteLine("Game First-to-5 Auto-Win Logic Passed");
        }

        // Helper Methods
        private void CreateTempPlayer(string name)
        {
            var profilesBtn = _driver.FindElement(By.XPath("//button[contains(., 'Profiles')]"));

            // Check if we need to switch tabs
            if (!profilesBtn.GetAttribute("class").Contains("active"))
            {
                profilesBtn.Click();
                // Wait for the transition/animation to allow the input to be interactable
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[placeholder='Enter Name']")));
            }

            var input = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[placeholder='Enter Name']")));

            // 1. Clear and Type
            input.Clear();
            input.SendKeys(name);

            // 2. IMPORTANT FIX: Trigger the change event
            // Sending Tab forces the browser to register the input change immediately
            input.SendKeys(Keys.Tab);

            // 3. Give Vue a tiny moment to update the v-model 'newName' variable
            Thread.Sleep(300);

            // 4. Now click Create
            var createBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".create-card button.btn-icon")));
            createBtn.Click();

            // 5. Wait for the name to appear in grid
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//h4[contains(text(), '{name}')]")));
            }
            catch (WebDriverTimeoutException)
            {
                // Optional: Add a debug message if it fails
                TestContext.WriteLine($"Failed to create player '{name}'. API might be down or file permissions issue.");
                throw;
            }
        }
    }
}