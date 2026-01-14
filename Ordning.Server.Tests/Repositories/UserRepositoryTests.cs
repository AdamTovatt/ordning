using EasyReasy.Database;
using Ordning.Server.Tests.TestUtilities;
using Ordning.Server.Users.Repositories;

namespace Ordning.Server.Tests.Repositories
{
    /// <summary>
    /// Integration tests for UserRepository.
    /// </summary>
    public class UserRepositoryTests : RepositoryTestBase
    {
        private UserRepository Repository { get; set; } = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Repository = new UserRepository(TestDatabaseManager.DataSource, SessionFactory);
        }

        [Fact]
        public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "testuser";
                string email = "test@example.com";
                string passwordHash = "hashed_password";
                
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: username,
                    email: email,
                    passwordHash: passwordHash,
                    roles: null,
                    session: session);

                // Act
                UserDbModel? result = await Repository.GetByEmailAsync(email, session);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(createdUser.Id, result.Id);
                Assert.Equal(username, result.Username);
                Assert.Equal(email, result.Email);
                Assert.Equal(passwordHash, result.PasswordHash);
            }
        }

        [Fact]
        public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string email = "nonexistent@example.com";

                // Act
                UserDbModel? result = await Repository.GetByEmailAsync(email, session);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenValidData_CreatesUser()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "newuser";
                string email = "newuser@example.com";
                string passwordHash = "hashed_password";

                // Act
                UserDbModel result = await Repository.CreateAsync(
                    username: username,
                    email: email,
                    passwordHash: passwordHash,
                    roles: null,
                    session: session);

                // Assert
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(username, result.Username);
                Assert.Equal(email, result.Email);
                Assert.Equal(passwordHash, result.PasswordHash);
                Assert.Equal("[]", result.RolesJson);

                // Verify can retrieve by email
                UserDbModel? retrieved = await Repository.GetByEmailAsync(email, session);
                Assert.NotNull(retrieved);
                Assert.Equal(result.Id, retrieved.Id);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenValidDataWithRoles_CreatesUserWithRoles()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "adminuser";
                string email = "admin@example.com";
                string passwordHash = "hashed_password";
                IEnumerable<string> roles = new[] { "Admin", "User" };

                // Act
                UserDbModel result = await Repository.CreateAsync(
                    username: username,
                    email: email,
                    passwordHash: passwordHash,
                    roles: roles,
                    session: session);

                // Assert
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(username, result.Username);
                Assert.Equal(email, result.Email);
                Assert.Equal(passwordHash, result.PasswordHash);
                Assert.Contains("Admin", result.RolesJson);
                Assert.Contains("User", result.RolesJson);

                // Verify can retrieve by email
                UserDbModel? retrieved = await Repository.GetByEmailAsync(email, session);
                Assert.NotNull(retrieved);
                Assert.Equal(result.Id, retrieved.Id);
                Assert.Contains("Admin", retrieved.RolesJson);
                Assert.Contains("User", retrieved.RolesJson);
            }
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenUserExists_UpdatesPassword()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "updateuser";
                string email = "update@example.com";
                string originalPasswordHash = "original_hash";
                string newPasswordHash = "new_hash";
                
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: username,
                    email: email,
                    passwordHash: originalPasswordHash,
                    roles: null,
                    session: session);

                // Act
                bool result = await Repository.UpdatePasswordAsync(
                    userId: createdUser.Id,
                    passwordHash: newPasswordHash,
                    session: session);

                // Assert
                Assert.True(result);

                // Verify password was updated
                UserDbModel? retrieved = await Repository.GetByEmailAsync(email, session);
                Assert.NotNull(retrieved);
                Assert.Equal(newPasswordHash, retrieved.PasswordHash);
            }
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentUserId = Guid.NewGuid();
                string newPasswordHash = "new_hash";

                // Act
                bool result = await Repository.UpdatePasswordAsync(
                    userId: nonExistentUserId,
                    passwordHash: newPasswordHash,
                    session: session);

                // Assert
                Assert.False(result);
            }
        }
    }
}
