using BlackjackLogic.Interfaces.Repository;

namespace BlackjackTest.Unit.User
{
    [TestClass]
    public class AttemptLogin
    {
        private Mock<IUserRepository>? _mockUserRepository;
        private UserLogic? _userLogic;

        [TestInitialize]
        public void Initialize()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userLogic = new UserLogic(_mockUserRepository.Object);
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
                salt
            );

            //mock repo
            _mockUserRepository?.Setup(r => r.RetrieveLoginInformation("testuser")).Returns(fakeDBLoginInfo);

            // Act
            Assert.IsNotNull(_userLogic);
            int result = _userLogic.ValidateUser("testuser", "password");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
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
                salt
            );

            //mock repo
            _mockUserRepository?.Setup(r => r.RetrieveLoginInformation("testuser")).Returns(fakeDBLoginInfo);

            // Act
            Assert.IsNotNull(_userLogic);
            int result = _userLogic.ValidateUser("testuser", "PASSWORD");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
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
                salt
            );

            //mock repo
            _mockUserRepository?.Setup(r => r.RetrieveLoginInformation("testuser")).Returns(fakeDBLoginInfo);

            // Act
            Assert.IsNotNull(_userLogic);
            int result = _userLogic.ValidateUser("Testuser", "password");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(0, result);
        }
    }
}
