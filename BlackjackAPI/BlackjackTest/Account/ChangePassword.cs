using BlackjackCommon.Interfaces.Repository;

namespace BlackjackTest.Account
{
	[TestClass]
	public class ChangePassword
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
		public void ChangePassword_SuccessfulPasswordChange_ReturnsNull()
		{
			// Arrange
			//salt and hash for 'password'
			byte[] hashedPassword = Convert.FromBase64String("2WG9Qvv+Mg4OElDUfZQOBA==");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeSaltHashInfo =
			(
				hashed_pw: hashedPassword,
				salt: salt
			);

			_mockAccountRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);
			_mockAccountRepository.Setup(dal => dal.UpdatePassword(1, It.IsAny<string>(), It.IsAny<string>())).Returns(true);


			// Act
			var result = _accountLogic.ChangePassword(1, "password", "newPass", "newPass");

			// Assert
			Assert.IsTrue(result.Success);
			Assert.IsNull(result.Message);
		}

		[TestMethod]
		public void ChangePassword_NewPasswordsDontMatch_ReturnsErrorMessage()
		{
			// Act
			var result = _accountLogic.ChangePassword(1, "oldPass", "newPass123", "newPass456");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "New passwords don't match");
		}

		[TestMethod]
		public void ChangePassword_NewPasswordSameAsOldPassword_ReturnsErrorMessage()
		{
			// Act
			var result = _accountLogic.ChangePassword(1, "oldPass", "oldPass", "oldPass");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "New password can't be the same as old password!");
		}

		[TestMethod]
		public void ChangePassword_OldPasswordsDontMatch_ReturnsErrorMessage()
		{
			// Arrange
			//salt and hash for 'password'
			byte[] hashedPassword = Convert.FromBase64String("2WG9Qvv+Mg4OElDUfZQOBA==");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeSaltHashInfo =
			(
				hashed_pw: hashedPassword,
				salt: salt
			);

			_mockAccountRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);

			// Act
			var result = _accountLogic.ChangePassword(1, "passwordt", "newPass", "newPass");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "Old passwords don't match");
		}

		[TestMethod]
		public void ChangePassword_DatabaseUpdateFails_ReturnsErrorMessage()
		{
			// Arrange
			//salt and hash for 'password'
			byte[] hashedPassword = Convert.FromBase64String("2WG9Qvv+Mg4OElDUfZQOBA==");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeSaltHashInfo =
			(
				hashed_pw: hashedPassword,
				salt: salt
			);

			_mockAccountRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);
			_mockAccountRepository.Setup(dal => dal.UpdatePassword(1, It.IsAny<string>(), It.IsAny<string>())).Returns(false);

			// Act
			var result = _accountLogic.ChangePassword(1, "password", "newPass", "newPass");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "An error occurred. Please try again later.");

		}

		[TestMethod]
		public void ChangePassword_EmptyHashedPasswordField_ReturnsErrorMessage()
		{
			// Arrange
			byte[] hashedPassword = Convert.FromBase64String("");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeSaltHashInfo =
			(
				hashed_pw: hashedPassword,
				salt: salt
			);

			_mockAccountRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);
			_mockAccountRepository.Setup(dal => dal.UpdatePassword(1, It.IsAny<string>(), It.IsAny<string>())).Returns(false);

			// Act
			var result = _accountLogic.ChangePassword(1, "password", "newPass", "newPass");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "An error occurred. Please try again later.");
		}

		[TestMethod]
		public void ChangePassword_EmptySaltField_ReturnsErrorMessage()
		{
			// Arrange
			byte[] hashedPassword = Convert.FromBase64String("2WG9Qvv+Mg4OElDUfZQOBA==");
			byte[] salt = Convert.FromBase64String("");

			var fakeSaltHashInfo =
			(
				hashed_pw: hashedPassword,
				salt: salt
			);

			_mockAccountRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);
			_mockAccountRepository.Setup(dal => dal.UpdatePassword(1, It.IsAny<string>(), It.IsAny<string>())).Returns(false);

			// Act
			var result = _accountLogic.ChangePassword(1, "password", "newPass", "newPass");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual(result.Message, "An error occurred. Please try again later.");

		}
	}
}
