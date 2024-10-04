using BlackjackCommon.Interfaces.Repository;

namespace BlackjackTest.Account
{
	[TestClass]
	public class AttemptLogin
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
		public void ValidateUser_ValidCredentials_Returns_UserID()
		{
			// Arrange
			byte[] hashedPassword = Convert.FromBase64String("2WG9Qvv+Mg4OElDUfZQOBA==");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeDBLoginInfo = (
				user_id: 1,
				user_name: "testuser",
				hashed_pw: hashedPassword,
				salt: salt
			);

			//mock repo
			_mockAccountRepository.Setup(r => r.RetrieveLoginInformation("testuser")).Returns(fakeDBLoginInfo);

			// Act
			int result = _accountLogic.ValidateUser("testuser", "password");

			// Assert
			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public void ValidateUser_InvalidPassword_Returns_0()
		{
			// Arrange
			byte[] hashedPassword = Convert.FromBase64String("2WG9Qvv+Mg4OElDUfZQOBA==");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeDBLoginInfo = (
				user_id: 1,
				user_name: "testuser",
				hashed_pw: hashedPassword,
				salt: salt
			);

			//mock repo
			_mockAccountRepository.Setup(r => r.RetrieveLoginInformation("testuser")).Returns(fakeDBLoginInfo);

			// Act
			int result = _accountLogic.ValidateUser("testuser", "PASSWORD");

			// Assert
			Assert.AreEqual(0, result);
		}

		[TestMethod]
		public void ValidateUser_InvalidUsername_Returns_0()
		{
			// Arrange
			byte[] hashedPassword = Convert.FromBase64String("2WG9Qvv+Mg4OElDUfZQOBA==");
			byte[] salt = Convert.FromBase64String("p/o/W2ZF/F0y9Om3pyjVew==");

			var fakeDBLoginInfo = (
				user_id: 1,
				user_name: "testuser",
				hashed_pw: hashedPassword,
				salt: salt
			);

			//mock repo
			_mockAccountRepository.Setup(r => r.RetrieveLoginInformation("testuser")).Returns(fakeDBLoginInfo);

			// Act
			int result = _accountLogic.ValidateUser("Testuser", "password");

			// Assert
			Assert.AreEqual(0, result);
		}
	}
}
