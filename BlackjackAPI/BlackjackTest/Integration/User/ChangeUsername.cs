using BlackjackCommon.Entities.User;
using BlackjackDAL;
using BlackjackDAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlackjackTest.Unit.User
{
    [TestClass]
    public class ChangeUsernameIntegrationTests
    {
        private AppDbContext? _context;
        private UserLogic? _userLogic;

        [TestInitialize]
        public void Initialize()
        {
            Environment.SetEnvironmentVariable("JWT", "yMMp3FmNqsWZQmrgKKH3oPiXS9oJKRxu4NVyzKCHngJrWLvPUj6vVGWvwMGPWME3orgVHhRvJGDzH2hb65WrUwdDMqy2mFHBMRJGFYiTgwet5JhoJDKVLkQHtpZF2iGq");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new AppDbContext(options);

            var userRepository = new UserRepository(_context);
            _userLogic = new UserLogic(userRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }

        [TestMethod]
        public void ChangeUsername_SuccessfulUpdate_ReturnsTrue()
        {
            // Arrange
            var newUser = new BlackjackCommon.Entities.User.User
            {
                user_name = "username",
                user_passwordhash = "2WG9Qvv+Mg4OElDUfZQOBA==",
                user_passwordsalt = "p/o/W2ZF/F0y9Om3pyjVew==",
                user_is_moderator = false,
                user_status = UserStatus.active,
                user_total_earnings_amt = 0,
                user_total_losses_amt = 0,
            };

            _context?.User.Add(newUser);
            _context?.SaveChanges();

            // Act
            var result = _userLogic?.ChangeUsername(1, "newname");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Message);
        }

        [TestMethod]
        public void ChangeUsername_UsernameAlreadyTaken_ReturnsErrorMessage()
        {
            // Arrange
            var newUser = new BlackjackCommon.Entities.User.User
            {
                user_name = "newname",
                user_passwordhash = "2WG9Qvv+Mg4OElDUfZQOBA==",
                user_passwordsalt = "p/o/W2ZF/F0y9Om3pyjVew==",
                user_is_moderator = false,
                user_status = UserStatus.active,
                user_total_earnings_amt = 0,
                user_total_losses_amt = 0,
            };

            _context?.User.Add(newUser);
            _context?.SaveChanges();

            // Act
            var result = _userLogic?.ChangeUsername(2, "newname");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Username already in use.", result.Message);
        }

        [TestMethod]
        public void ChangeUsername_UsernameAlreadyTakenByCurrentUser_ReturnsErrorMessage()
        {
            // Arrange
            var newUser = new BlackjackCommon.Entities.User.User
            {
                user_name = "newname",
                user_passwordhash = "2WG9Qvv+Mg4OElDUfZQOBA==",
                user_passwordsalt = "p/o/W2ZF/F0y9Om3pyjVew==",
                user_is_moderator = false,
                user_status = UserStatus.active,
                user_total_earnings_amt = 0,
                user_total_losses_amt = 0,
            };

            _context?.User.Add(newUser);
            _context?.SaveChanges();

            // Act
            var result = _userLogic?.ChangeUsername(1, "newname");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("You already have this username.", result.Message);
        }
    }
}
