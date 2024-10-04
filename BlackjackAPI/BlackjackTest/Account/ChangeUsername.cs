using BlackjackCommon.Interfaces.Repository;

namespace BlackjackTest.Account
{
	[TestClass]
	public class ChangeUsername
	{
		private Mock<IAccountRepository> _mockAccountRepository;
		private AccountLogic _accountLogic;

		[TestInitialize]
		public void Initialize()
		{
			_mockAccountRepository = new Mock<IAccountRepository>();
			_accountLogic = new AccountLogic(_mockAccountRepository.Object);
		}

		[TestMethod]
		public void ChangeUsername_SuccessfulUpdate_ReturnsTrue()
		{
			// Arrange
			_mockAccountRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(false);
			_mockAccountRepository.Setup(dal => dal.UpdateUsername(1, It.IsAny<string>())).Returns(true);

			// Act
			var result = _accountLogic.ChangeUsername(1, "newname");

			// Assert
			Assert.IsTrue(result.Success);
			Assert.IsNull(result.Message);
		}

		[TestMethod]
		public void ChangeUsername_UsernameAlreadyTaken_ReturnsErrorMessage()
		{
			// Arrange
			_mockAccountRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(true);

			// Act
			var result = _accountLogic.ChangeUsername(1, "newname");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "Username already in use.");
		}

		[TestMethod]
		public void ChangeUsername_DatabaseUpdateFails_ReturnsErrorMessage()
		{
			// Arrange
			_mockAccountRepository.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(false);
			_mockAccountRepository.Setup(dal => dal.UpdateUsername(1, It.IsAny<string>())).Returns(false);

			// Act
			var result = _accountLogic.ChangeUsername(1, "newname");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "An error occurred. Please try again later.");
		}

	}
}
