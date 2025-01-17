using BlackjackLogic.Interfaces.Repository;

namespace BlackjackTest.Unit.User
{
    [TestClass]
    public class ChangeUsername
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
        public void ChangeUsername_SuccessfulUpdate_ReturnsTrue()
        {
            // Arrange
            _mockUserRepository?.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(false);
            _mockUserRepository?.Setup(dal => dal.IsUsernameTakenByCurrentUser(It.IsAny<int>(), It.IsAny<string>())).Returns(false);
            _mockUserRepository?.Setup(dal => dal.UpdateUsername(1, It.IsAny<string>()));

            // Act
            var result = _userLogic?.ChangeUsername(1, "newname");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Message);
        }

        [TestMethod]
        public void ChangeUsername_UsernameAlreadyTaken_ReturnsErrorMessage()
        {
            // Arrange
            _mockUserRepository?.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(true);
            _mockUserRepository?.Setup(dal => dal.IsUsernameTakenByCurrentUser(It.IsAny<int>(), It.IsAny<string>())).Returns(false);

            // Act
            var result = _userLogic?.ChangeUsername(1, "newname");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Username already in use.", result.Message);
        }

        [TestMethod]
        public void ChangeUsername_UsernameAlreadyTakenByCurrentUser_ReturnsErrorMessage()
        {
            // Arrange
            _mockUserRepository?.Setup(dal => dal.IsUsernameTaken(It.IsAny<string>())).Returns(true);
            _mockUserRepository?.Setup(dal => dal.IsUsernameTakenByCurrentUser(It.IsAny<int>(), It.IsAny<string>())).Returns(true);


            // Act
            var result = _userLogic?.ChangeUsername(1, "newname");

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("You already have this username.", result.Message);
        }
    }
}
