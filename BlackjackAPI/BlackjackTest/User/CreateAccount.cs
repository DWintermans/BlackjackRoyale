﻿using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.Models;

namespace BlackjackTest.User
{
	[TestClass]
	public class CreateAccount
	{
		private Mock<IUserRepository> _mockUserRepository;
		private UserLogic _userLogic;

		[TestInitialize]
		public void Initialize()
		{
			_mockUserRepository = new Mock<IUserRepository>();
			_userLogic = new UserLogic(_mockUserRepository.Object);
		}

		[TestMethod]
		[DataRow("username1", "password1", 1)]
		[DataRow("username2", "password2", 2)]
		public void CreateAccount_SuccessfulAccountCreation_Returns_JWT(string username, string password, int user_id)
		{
			// Arrange
			_mockUserRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(false);
			_mockUserRepository.Setup(dal => dal.CreateAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(user_id);

			// Act
			var result = _userLogic.CreateAccount(username, password);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual("Account created successfully.", result.Message);

			int jwt_user_id = _userLogic.GetUserIDFromJWT(result.JWT);
			Assert.AreEqual(user_id, jwt_user_id);
		}

		[TestMethod]
		[DataRow("existingUser")]
		public void CreateAccount_UsernameIsTaken_ReturnsErrorMessage(string username)
		{
			// Arrange
			_mockUserRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(true);

			// Act
			var result = _userLogic.CreateAccount(username, "password");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Username already in use.", result.Message);

		}

		[TestMethod]
		[DataRow("newUser", "password")]
		public void CreateAccount_DatabaseAccountCreationFails_ReturnsErrorMessage(string username, string password)
		{
			// Arrange
			_mockUserRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(false);
			_mockUserRepository.Setup(dal => dal.CreateAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(0);

			// Act
			var result = _userLogic.CreateAccount(username, password);

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("An unexpected error occurred.", result.Message);
		}
	}
}