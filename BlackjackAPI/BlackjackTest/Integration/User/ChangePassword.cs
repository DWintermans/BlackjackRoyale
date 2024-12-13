using BlackjackCommon.Entities.User;
using BlackjackDAL;
using BlackjackDAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace BlackjackTest.Integration.User
{
    [TestClass]
    public class ChangePasswordIntegrationTests
    {
        private AppDbContext _context;
        private UserLogic _userLogic;

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
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void ChangePassword_SuccessfulPasswordChange_ReturnsNull()
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

            _context.User.Add(newUser);
            _context.SaveChanges();

            // Act
            var result = _userLogic.ChangePassword(1, "password", "newPass!1", "newPass!1");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Message);

            //verify db
            var updatedUser = _context.User.SingleOrDefault(u => u.user_id == 1);
            Assert.IsNotNull(updatedUser);
            Assert.AreNotEqual("2WG9Qvv+Mg4OElDUfZQOBA==", updatedUser.user_passwordhash);
        }

        [TestMethod]
        public void ChangePassword_OldPasswordsDontMatch_ReturnsErrorMessage()
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

            _context.User.Add(newUser);
            _context.SaveChanges();

            // Act
            var result = _userLogic.ChangePassword(1, "passwordt!1", "newPass!1", "newPass!1");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Old passwords don't match.", result.Message);
        }

        [TestMethod]
        public void ChangePassword_EmptyHashedPasswordField_ReturnsErrorMessage()
        {
            // Arrange
            var newUser = new BlackjackCommon.Entities.User.User
            {
                user_name = "username",
                user_passwordhash = "",
                user_passwordsalt = "p/o/W2ZF/F0y9Om3pyjVew==",
                user_is_moderator = false,
                user_status = UserStatus.active,
                user_total_earnings_amt = 0,
                user_total_losses_amt = 0,
            };

            _context.User.Add(newUser);
            _context.SaveChanges();

            // Act
            var result = _userLogic.ChangePassword(1, "password", "newPass!1", "newPass!1");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("An unexpected error occurred.", result.Message);
        }

        [TestMethod]
        public void ChangePassword_EmptySaltField_ReturnsErrorMessage()
        {
            // Arrange
            var newUser = new BlackjackCommon.Entities.User.User
            {
                user_name = "username",
                user_passwordhash = "2WG9Qvv+Mg4OElDUfZQOBA==",
                user_passwordsalt = "",
                user_is_moderator = false,
                user_status = UserStatus.active,
                user_total_earnings_amt = 0,
                user_total_losses_amt = 0,
            };

            _context.User.Add(newUser);
            _context.SaveChanges();

            // Act
            var result = _userLogic.ChangePassword(1, "password", "newPass!1", "newPass!1");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("An unexpected error occurred.", result.Message);

        }
    }
}
