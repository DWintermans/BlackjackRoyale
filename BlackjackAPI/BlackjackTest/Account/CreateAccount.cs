using BlackjackCommon.Interfaces.Repository;

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
		public void CreateAcccount_SuccessfulAccountCreation_Returns_JWT()
		{
			// Arrange
			_mockUserRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(false);
			_mockUserRepository.Setup(dal => dal.CreateAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(123);

			// Act
			var result = _userLogic.CreateAccount("username", "password");

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual("Account created successfully", result.Message);
			int user_id = _userLogic.GetUserIDFromJWT(result.JWT);
			Assert.AreEqual(123, user_id);
		}

		[TestMethod]
		public void CreateAcccount_UsernameIsTaken_ReturnsErrorMessage()
		{
			// Arrange
			_mockUserRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(true);

			// Act
			var result = _userLogic.CreateAccount("username", "password");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Username is already in use.", result.Message);
		}

		[TestMethod]
		public void CreateAcccount_DatabaseAccountCreationFails_ReturnsErrorMessage()
		{
			// Arrange
			_mockUserRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(false);
			_mockUserRepository.Setup(dal => dal.CreateAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(0);

			// Act
			var result = _userLogic.CreateAccount("username", "password");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("An error occurred. Please try again later.", result.Message);
		}
	}
}