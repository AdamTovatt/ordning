using EasyReasy.Auth;
using EasyReasy.Database;
using Moq;
using Ordning.Server.Users.Models;
using Ordning.Server.Users.Repositories;
using Ordning.Server.Users.Services;

namespace Ordning.Server.Tests.Services
{
    /// <summary>
    /// Unit tests for UserService.
    /// </summary>
    public class UserServiceTests
    {
        private Mock<IUserRepository> MockRepository { get; set; } = null!;
        private IPasswordHasher PasswordHasher { get; set; } = null!;
        private UserService Service { get; set; } = null!;

        public UserServiceTests()
        {
            MockRepository = new Mock<IUserRepository>();
            PasswordHasher = new SecurePasswordHasher();
            Service = new UserService(MockRepository.Object, PasswordHasher);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WhenValidCredentials_ReturnsUser()
        {
            // Arrange
            string email = "test@example.com";
            string password = "TestPassword123!";
            string passwordHash = PasswordHasher.HashPassword(password, email);
            
            UserDbModel userDbModel = new UserDbModel
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = email,
                PasswordHash = passwordHash,
                RolesJson = "[]"
            };

            MockRepository
                .Setup(r => r.GetByEmailAsync(email, null))
                .ReturnsAsync(userDbModel);

            // Act
            User? result = await Service.ValidateCredentialsAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userDbModel.Id.ToString(), result.Id);
            Assert.Equal(userDbModel.Username, result.Username);
            Assert.Equal(userDbModel.Email, result.Email);
            MockRepository.Verify(r => r.GetByEmailAsync(email, null), Times.Once);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WhenUserNotFound_ReturnsNull()
        {
            // Arrange
            string email = "nonexistent@example.com";
            string password = "TestPassword123!";

            MockRepository
                .Setup(r => r.GetByEmailAsync(email, null))
                .ReturnsAsync((UserDbModel?)null);

            // Act
            User? result = await Service.ValidateCredentialsAsync(email, password);

            // Assert
            Assert.Null(result);
            MockRepository.Verify(r => r.GetByEmailAsync(email, null), Times.Once);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WhenInvalidPassword_ReturnsNull()
        {
            // Arrange
            string email = "test@example.com";
            string correctPassword = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";
            string passwordHash = PasswordHasher.HashPassword(correctPassword, email);
            
            UserDbModel userDbModel = new UserDbModel
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = email,
                PasswordHash = passwordHash,
                RolesJson = "[]"
            };

            MockRepository
                .Setup(r => r.GetByEmailAsync(email, null))
                .ReturnsAsync(userDbModel);

            // Act
            User? result = await Service.ValidateCredentialsAsync(email, wrongPassword);

            // Assert
            Assert.Null(result);
            MockRepository.Verify(r => r.GetByEmailAsync(email, null), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WhenValidData_CreatesUserWithHashedPassword()
        {
            // Arrange
            string username = "testuser";
            string email = "test@example.com";
            string password = "TestPassword123!";
            Guid userId = Guid.NewGuid();
            
            UserDbModel createdUserDbModel = new UserDbModel
            {
                Id = userId,
                Username = username,
                Email = email,
                PasswordHash = "hashed_password",
                RolesJson = "[]"
            };

            MockRepository
                .Setup(r => r.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<IDbSession?>()))
                .ReturnsAsync(createdUserDbModel);

            // Act
            User result = await Service.CreateUserAsync(username, email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId.ToString(), result.Id);
            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);
            
            MockRepository.Verify(
                r => r.CreateAsync(
                    username,
                    email,
                    It.Is<string>(hash => !string.IsNullOrEmpty(hash) && hash != password),
                    null,
                    null),
                Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WhenValidDataWithRoles_CreatesUserWithRoles()
        {
            // Arrange
            string username = "testuser";
            string email = "test@example.com";
            string password = "TestPassword123!";
            IEnumerable<string> roles = new[] { "Admin", "User" };
            Guid userId = Guid.NewGuid();
            
            UserDbModel createdUserDbModel = new UserDbModel
            {
                Id = userId,
                Username = username,
                Email = email,
                PasswordHash = "hashed_password",
                RolesJson = "[\"Admin\",\"User\"]"
            };

            MockRepository
                .Setup(r => r.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<IDbSession?>()))
                .ReturnsAsync(createdUserDbModel);

            // Act
            User result = await Service.CreateUserAsync(username, email, password, roles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId.ToString(), result.Id);
            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);
            Assert.Equal(roles, result.Roles);
            
            MockRepository.Verify(
                r => r.CreateAsync(
                    username,
                    email,
                    It.IsAny<string>(),
                    roles,
                    null),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenValidUserId_UpdatesPassword()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string userIdString = userId.ToString();
            string email = "test@example.com";
            string newPassword = "NewPassword123!";
            
            MockRepository
                .Setup(r => r.UpdatePasswordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<IDbSession?>()))
                .ReturnsAsync(true);

            // Act
            bool result = await Service.UpdatePasswordAsync(userIdString, newPassword, email);

            // Assert
            Assert.True(result);
            MockRepository.Verify(
                r => r.UpdatePasswordAsync(
                    userId,
                    It.Is<string>(hash => !string.IsNullOrEmpty(hash) && hash != newPassword),
                    null),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenInvalidUserId_ReturnsFalse()
        {
            // Arrange
            string invalidUserId = "not-a-valid-guid";
            string email = "test@example.com";
            string newPassword = "NewPassword123!";

            // Act
            bool result = await Service.UpdatePasswordAsync(invalidUserId, newPassword, email);

            // Assert
            Assert.False(result);
            MockRepository.Verify(
                r => r.UpdatePasswordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<IDbSession?>()),
                Times.Never);
        }

        [Fact]
        public async Task GetUserCountAsync_WhenCalled_ReturnsCountFromRepository()
        {
            // Arrange
            int expectedCount = 5;
            MockRepository
                .Setup(r => r.GetCountAsync(null))
                .ReturnsAsync(expectedCount);

            // Act
            int result = await Service.GetUserCountAsync();

            // Assert
            Assert.Equal(expectedCount, result);
            MockRepository.Verify(r => r.GetCountAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetUserCountAsync_WhenNoUsers_ReturnsZero()
        {
            // Arrange
            MockRepository
                .Setup(r => r.GetCountAsync(null))
                .ReturnsAsync(0);

            // Act
            int result = await Service.GetUserCountAsync();

            // Assert
            Assert.Equal(0, result);
            MockRepository.Verify(r => r.GetCountAsync(null), Times.Once);
        }
    }
}
