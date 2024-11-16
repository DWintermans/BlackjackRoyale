using BlackjackCommon.Interfaces.Repository;

namespace BlackjackTest.User
{
	[TestClass]
	public class ChangePassword
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

			_mockUserRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);
			_mockUserRepository.Setup(dal => dal.UpdatePassword(1, It.IsAny<string>(), It.IsAny<string>()));


			// Act
			var result = _userLogic.ChangePassword(1, "password", "newPass!1", "newPass!1");

			// Assert
			Assert.IsTrue(result.Success);
			Assert.IsNull(result.Message);
		}

		[TestMethod]
		public void ChangePassword_NewPasswordsDontMatch_ReturnsErrorMessage()
		{
			// Act
			var result = _userLogic.ChangePassword(1, "oldPass!1", "newPass123!1", "newPass456!1");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("New passwords don't match.", result.Message);
		}

		[TestMethod]
		public void ChangePassword_NewPasswordSameAsOldPassword_ReturnsErrorMessage()
		{
			// Act
			var result = _userLogic.ChangePassword(1, "oldPass!1", "oldPass!1", "oldPass!1");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("New password can't be the same as old password.", result.Message);
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

			_mockUserRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);

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
			byte[] hashedPassword = Convert.FromBase64String("");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeSaltHashInfo =
			(
				hashed_pw: hashedPassword,
				salt: salt
			);

			_mockUserRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);
			_mockUserRepository.Setup(dal => dal.UpdatePassword(1, It.IsAny<string>(), It.IsAny<string>()));

			// Act
			var result = _userLogic.ChangePassword(1, "password!1", "newPass!1", "newPass!1");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("An unexpected error occurred.", result.Message);
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

			_mockUserRepository.Setup(dal => dal.RetrieveSalt_HashInformation(1)).Returns(fakeSaltHashInfo);
			_mockUserRepository.Setup(dal => dal.UpdatePassword(1, It.IsAny<string>(), It.IsAny<string>()));

			// Act
			var result = _userLogic.ChangePassword(1, "password!1", "newPass!1", "newPass!1");

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("An unexpected error occurred.", result.Message);

		}
	}
}
