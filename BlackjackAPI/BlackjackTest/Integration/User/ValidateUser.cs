using BlackjackCommon.Entities.User;
using BlackjackDAL;
using BlackjackDAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlackjackTest.Integration.User
{
	[TestClass]
	public class AttemptLoginIntegrationTests
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
        public void ValidateUser_ValidCredentials_Returns_UserID()
        {
			// Arrange
			var newUser = new BlackjackCommon.Entities.User.User
			{
				user_name = "testuser",
				user_passwordhash = "2WG9Qvv+Mg4OElDUfZQOBA==",
				user_passwordsalt = "p/o/W2ZF/F0y9Om3pyjVew==",
				user_is_moderator = false,
				user_status = UserStatus.active,
				user_total_earnings_amt = 0,
				user_total_losses_amt = 0,
			};

			_context.User.Add(newUser);
			_context.SaveChanges();

			//Act
			int user_id = _userLogic.ValidateUser("testuser", "password");

			// Assert
			Assert.AreEqual(1, user_id);
		}

        [TestMethod]
        public void ValidateUser_InvalidPassword_Returns_0()
        {
			// Arrange
			var newUser = new BlackjackCommon.Entities.User.User
			{
				user_name = "testuser",
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
			int user_id = _userLogic.ValidateUser("testuser", "password1");

            // Assert
            Assert.AreEqual(0, user_id);
        }

        [TestMethod]
        public void ValidateUser_InvalidUsername_Returns_0()
        {
			// Arrange
			var newUser = new BlackjackCommon.Entities.User.User
			{
				user_name = "testuser",
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
			int user_id = _userLogic.ValidateUser("Testuser", "password");

            // Assert
            Assert.AreEqual(0, user_id);
        }
    }
}
