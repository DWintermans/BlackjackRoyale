using BlackjackCommon.Entities.User;
using BlackjackDAL;
using BlackjackDAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlackjackTest.Integration.User
{
    [TestClass]
    public class CreateAccountIntegrationTests
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
        [DataRow("username1", "password!1")]
        [DataRow("username2", "password!2")]
        public void CreateAccount_SuccessfulAccountCreation_Returns_JWT(string username, string password)
        {
            // Act
            var result = _userLogic.CreateAccount(username, password);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Account created successfully.", result.Message);

            var jwtUserId = _userLogic.GetUserIDFromJWT(result.Data);

            // verify db
            var createdUser = _context.User.SingleOrDefault(u => u.user_id == jwtUserId);
            Assert.IsNotNull(createdUser);
            Assert.AreEqual(username, createdUser.user_name);
        }

        [TestMethod]
        [DataRow("", "password1", "Username is required.")]
        [DataRow("username2", "", "Password is required.")]
        [DataRow("username3", "short", "Password must be at least 6 characters long.")]
        [DataRow("username4", "256Chars_ajsSgyN9hmBy6yNAGVPvFZtcgQcGazcciFbR7W6iAp3jsSgyN9hmBy6yNAGVPvFZtcgQcGazcciFbR7W6iAp3jsSgyN9hmBy6yNAGVPvFZtcgQcGazcciFbR7W6iAp3jsSgyN9hmBy6yNAGVPvFZtcgQcGazcciFbR7W6iAp3jsSgyN9hmBy6yNAGVPvFZtcgQcGazcciFbR7W6iAp3jsSgyN9hmBy6yNAGVPvFZtcgQcGazcciFbaa", "Password cannot exceed 255 characters.")]
        [DataRow("username5", "longpasswordwithoutspecialchar", "Password must contain atleast one number and one special charachter.")]
        public void CreateAccount_InvalidVariables_Returns_ErrorResponse(string username, string password, string expectedErrorMessage)
        {
            // Act
            var result = _userLogic.CreateAccount(username, password);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(expectedErrorMessage, result.Message);

            // verify db
            var userCount = _context.User.Count(u => u.user_name == username);
            Assert.AreEqual(0, userCount);
        }

        [TestMethod]
        [DataRow("existingUser")]
        public void CreateAccount_UsernameIsTaken_ReturnsErrorMessage(string username)
        {
            // Arrange
            var newUser = new BlackjackCommon.Entities.User.User
            {
                user_name = username,
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
            var result = _userLogic.CreateAccount(username, "password!1");

            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Username already in use.", result.Message);

            // verify db
            var userCount = _context.User.Count(u => u.user_name == username);
            Assert.AreEqual(1, userCount);
        }
    }
}






