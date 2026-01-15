using EasyReasy.Database;
using Npgsql;
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
        public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
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
                UserDbModel? result = await Repository.GetByIdAsync(createdUser.Id, session);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(createdUser.Id, result.Id);
                Assert.Equal(username, result.Username);
                Assert.Equal(email, result.Email);
                Assert.Equal(passwordHash, result.PasswordHash);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentUserId = Guid.NewGuid();

                // Act
                UserDbModel? result = await Repository.GetByIdAsync(nonExistentUserId, session);

                // Assert
                Assert.Null(result);
            }
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
        public async Task GetByUsernameAsync_WhenUserExists_ReturnsUser()
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
                UserDbModel? result = await Repository.GetByUsernameAsync(username, session);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(createdUser.Id, result.Id);
                Assert.Equal(username, result.Username);
                Assert.Equal(email, result.Email);
                Assert.Equal(passwordHash, result.PasswordHash);
            }
        }

        [Fact]
        public async Task GetByUsernameAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "nonexistentuser";

                // Act
                UserDbModel? result = await Repository.GetByUsernameAsync(username, session);

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

        [Fact]
        public async Task GetCountAsync_WhenNoUsers_ReturnsZero()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                // Act
                int count = await Repository.GetCountAsync(session);

                // Assert
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public async Task GetCountAsync_WhenUsersExist_ReturnsCorrectCount()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                await Repository.CreateAsync(
                    username: "user1",
                    email: "user1@example.com",
                    passwordHash: "hash1",
                    roles: null,
                    session: session);

                await Repository.CreateAsync(
                    username: "user2",
                    email: "user2@example.com",
                    passwordHash: "hash2",
                    roles: null,
                    session: session);

                await Repository.CreateAsync(
                    username: "user3",
                    email: "user3@example.com",
                    passwordHash: "hash3",
                    roles: null,
                    session: session);

                // Act
                int count = await Repository.GetCountAsync(session);

                // Assert
                Assert.Equal(3, count);
            }
        }

        [Fact]
        public async Task GetByUsernameAsync_WhenUserExistsWithDifferentCase_ReturnsUser()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "TestUser";
                string email = "testuser@example.com";
                string passwordHash = "hashed_password";
                
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: username,
                    email: email,
                    passwordHash: passwordHash,
                    roles: null,
                    session: session);

                // Act & Assert - Test various case combinations
                UserDbModel? result1 = await Repository.GetByUsernameAsync("testuser", session);
                Assert.NotNull(result1);
                Assert.Equal(createdUser.Id, result1.Id);
                Assert.Equal(username, result1.Username); // Original casing preserved

                UserDbModel? result2 = await Repository.GetByUsernameAsync("TESTUSER", session);
                Assert.NotNull(result2);
                Assert.Equal(createdUser.Id, result2.Id);

                UserDbModel? result3 = await Repository.GetByUsernameAsync("TeStUsEr", session);
                Assert.NotNull(result3);
                Assert.Equal(createdUser.Id, result3.Id);
            }
        }

        [Fact]
        public async Task GetByEmailAsync_WhenUserExistsWithDifferentCase_ReturnsUser()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "testuser";
                string email = "Test@Example.com";
                string passwordHash = "hashed_password";
                
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: username,
                    email: email,
                    passwordHash: passwordHash,
                    roles: null,
                    session: session);

                // Act & Assert - Test various case combinations
                UserDbModel? result1 = await Repository.GetByEmailAsync("test@example.com", session);
                Assert.NotNull(result1);
                Assert.Equal(createdUser.Id, result1.Id);
                Assert.Equal(email, result1.Email); // Original casing preserved

                UserDbModel? result2 = await Repository.GetByEmailAsync("TEST@EXAMPLE.COM", session);
                Assert.NotNull(result2);
                Assert.Equal(createdUser.Id, result2.Id);

                UserDbModel? result3 = await Repository.GetByEmailAsync("TeSt@ExAmPlE.CoM", session);
                Assert.NotNull(result3);
                Assert.Equal(createdUser.Id, result3.Id);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenUsernameExistsWithDifferentCase_ThrowsPostgresException()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username = "ExistingUser";
                string email1 = "user1@example.com";
                string email2 = "user2@example.com";
                string passwordHash = "hashed_password";
                
                await Repository.CreateAsync(
                    username: username,
                    email: email1,
                    passwordHash: passwordHash,
                    roles: null,
                    session: session);

                // Act & Assert - Try to create user with same username but different case
                PostgresException exception = await Assert.ThrowsAsync<PostgresException>(async () =>
                {
                    await Repository.CreateAsync(
                        username: "existinguser",
                        email: email2,
                        passwordHash: passwordHash,
                        roles: null,
                        session: session);
                });

                Assert.Equal("23505", exception.SqlState); // Unique violation
                Assert.Contains("idx_auth_user_username_lower", exception.ConstraintName ?? string.Empty);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenEmailExistsWithDifferentCase_ThrowsPostgresException()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string username1 = "user1";
                string username2 = "user2";
                string email = "Existing@Example.com";
                string passwordHash = "hashed_password";
                
                await Repository.CreateAsync(
                    username: username1,
                    email: email,
                    passwordHash: passwordHash,
                    roles: null,
                    session: session);

                // Act & Assert - Try to create user with same email but different case
                PostgresException exception = await Assert.ThrowsAsync<PostgresException>(async () =>
                {
                    await Repository.CreateAsync(
                        username: username2,
                        email: "existing@example.com",
                        passwordHash: passwordHash,
                        roles: null,
                        session: session);
                });

                Assert.Equal("23505", exception.SqlState); // Unique violation
                Assert.Contains("idx_auth_user_email_lower", exception.ConstraintName ?? string.Empty);
            }
        }

        [Fact]
        public async Task GetAllAsync_WhenUsersExist_ReturnsAllUsers()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                UserDbModel user1 = await Repository.CreateAsync(
                    username: "user1",
                    email: "user1@example.com",
                    passwordHash: "hash1",
                    roles: null,
                    session: session);

                UserDbModel user2 = await Repository.CreateAsync(
                    username: "user2",
                    email: "user2@example.com",
                    passwordHash: "hash2",
                    roles: new[] { "write" },
                    session: session);

                UserDbModel user3 = await Repository.CreateAsync(
                    username: "user3",
                    email: "user3@example.com",
                    passwordHash: "hash3",
                    roles: new[] { "admin" },
                    session: session);

                // Act
                IEnumerable<UserDbModel> result = await Repository.GetAllAsync(session);

                // Assert
                List<UserDbModel> resultList = result.ToList();
                Assert.Equal(3, resultList.Count);
                Assert.All(resultList, u => Assert.Equal(string.Empty, u.PasswordHash));
                Assert.Contains(resultList, u => u.Id == user1.Id && u.Username == "user1");
                Assert.Contains(resultList, u => u.Id == user2.Id && u.Username == "user2");
                Assert.Contains(resultList, u => u.Id == user3.Id && u.Username == "user3");
                
                // Verify ordering by username
                Assert.Equal("user1", resultList[0].Username);
                Assert.Equal("user2", resultList[1].Username);
                Assert.Equal("user3", resultList[2].Username);
            }
        }

        [Fact]
        public async Task GetAllAsync_WhenNoUsers_ReturnsEmptyCollection()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                // Act
                IEnumerable<UserDbModel> result = await Repository.GetAllAsync(session);

                // Assert
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task UpdateRolesAsync_WhenValidRoles_UpdatesRoles()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: "testuser",
                    email: "test@example.com",
                    passwordHash: "hash",
                    roles: new[] { "write" },
                    session: session);

                IEnumerable<string> newRoles = new[] { "admin", "write" };

                // Act
                bool result = await Repository.UpdateRolesAsync(createdUser.Id, newRoles, session);

                // Assert
                Assert.True(result);

                UserDbModel? updatedUser = await Repository.GetByIdAsync(createdUser.Id, session);
                Assert.NotNull(updatedUser);
                Assert.Contains("admin", updatedUser.RolesJson);
                Assert.Contains("write", updatedUser.RolesJson);
            }
        }

        [Fact]
        public async Task UpdateRolesAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentUserId = Guid.NewGuid();
                IEnumerable<string> roles = new[] { "write" };

                // Act
                bool result = await Repository.UpdateRolesAsync(nonExistentUserId, roles, session);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateRolesAsync_WhenEmptyRoles_UpdatesToEmptyArray()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: "testuser",
                    email: "test@example.com",
                    passwordHash: "hash",
                    roles: new[] { "write", "admin" },
                    session: session);

                // Act
                bool result = await Repository.UpdateRolesAsync(createdUser.Id, Array.Empty<string>(), session);

                // Assert
                Assert.True(result);

                UserDbModel? updatedUser = await Repository.GetByIdAsync(createdUser.Id, session);
                Assert.NotNull(updatedUser);
                Assert.Equal("[]", updatedUser.RolesJson);
            }
        }

        [Fact]
        public async Task AddRoleAsync_WhenValidRole_AddsRole()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: "testuser",
                    email: "test@example.com",
                    passwordHash: "hash",
                    roles: new[] { "write" },
                    session: session);

                // Act
                bool result = await Repository.AddRoleAsync(createdUser.Id, "admin", session);

                // Assert
                Assert.True(result);

                UserDbModel? updatedUser = await Repository.GetByIdAsync(createdUser.Id, session);
                Assert.NotNull(updatedUser);
                Assert.Contains("write", updatedUser.RolesJson);
                Assert.Contains("admin", updatedUser.RolesJson);
            }
        }

        [Fact]
        public async Task AddRoleAsync_WhenRoleAlreadyExists_DoesNotDuplicate()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: "testuser",
                    email: "test@example.com",
                    passwordHash: "hash",
                    roles: new[] { "write" },
                    session: session);

                // Act - Try to add the same role again
                bool result = await Repository.AddRoleAsync(createdUser.Id, "write", session);

                // Assert - Should return true because role already exists (idempotent behavior)
                Assert.True(result);

                UserDbModel? updatedUser = await Repository.GetByIdAsync(createdUser.Id, session);
                Assert.NotNull(updatedUser);
                Assert.Contains("write", updatedUser.RolesJson);
                Assert.DoesNotContain("write", updatedUser.RolesJson.Replace("\"write\"", "", StringComparison.Ordinal));
            }
        }

        [Fact]
        public async Task AddRoleAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentUserId = Guid.NewGuid();

                // Act
                bool result = await Repository.AddRoleAsync(nonExistentUserId, "write", session);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task RemoveRoleAsync_WhenRoleExists_RemovesRole()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: "testuser",
                    email: "test@example.com",
                    passwordHash: "hash",
                    roles: new[] { "write", "admin" },
                    session: session);

                // Act
                bool result = await Repository.RemoveRoleAsync(createdUser.Id, "write", session);

                // Assert
                Assert.True(result);

                UserDbModel? updatedUser = await Repository.GetByIdAsync(createdUser.Id, session);
                Assert.NotNull(updatedUser);
                Assert.DoesNotContain("write", updatedUser.RolesJson);
                Assert.Contains("admin", updatedUser.RolesJson);
            }
        }

        [Fact]
        public async Task RemoveRoleAsync_WhenRoleDoesNotExist_ReturnsTrue()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                UserDbModel createdUser = await Repository.CreateAsync(
                    username: "testuser",
                    email: "test@example.com",
                    passwordHash: "hash",
                    roles: new[] { "write" },
                    session: session);

                // Act - Try to remove a role that doesn't exist
                bool result = await Repository.RemoveRoleAsync(createdUser.Id, "admin", session);

                // Assert - Should return true (idempotent operation)
                Assert.True(result);

                UserDbModel? updatedUser = await Repository.GetByIdAsync(createdUser.Id, session);
                Assert.NotNull(updatedUser);
                Assert.Contains("write", updatedUser.RolesJson);
            }
        }

        [Fact]
        public async Task RemoveRoleAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentUserId = Guid.NewGuid();

                // Act
                bool result = await Repository.RemoveRoleAsync(nonExistentUserId, "write", session);

                // Assert
                Assert.False(result);
            }
        }
    }
}
