using EasyReasy.Database;
using Ordning.Server.Database;
using Ordning.Server.Items.Repositories;
using Ordning.Server.Locations.Repositories;
using Ordning.Server.Tests.TestUtilities;

namespace Ordning.Server.Tests.Repositories
{
    /// <summary>
    /// Integration tests for LocationRepository.
    /// </summary>
    public class LocationRepositoryTests : RepositoryTestBase
    {
        private LocationRepository Repository { get; set; } = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Repository = new LocationRepository(TestDatabaseManager.DataSource, SessionFactory);
        }

        [Fact]
        public async Task GetByIdAsync_WhenLocationExists_ReturnsLocation()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id = "test-location";
                string name = "Test Location";
                string description = "Test Description";

                LocationDbModel createdLocation = await Repository.CreateAsync(
                    id: id,
                    name: name,
                    description: description,
                    parentLocationId: null,
                    session: session);

                // Act
                LocationDbModel? result = await Repository.GetByIdAsync(id, session);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(createdLocation.Id, result.Id);
                Assert.Equal(name, result.Name);
                Assert.Equal(description, result.Description);
                Assert.Null(result.ParentLocationId);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WhenLocationDoesNotExist_ReturnsNull()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id = "nonexistent-location";

                // Act
                LocationDbModel? result = await Repository.GetByIdAsync(id, session);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenValidData_CreatesLocation()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id = "new-location";
                string name = "New Location";
                string description = "New Description";

                // Act
                LocationDbModel result = await Repository.CreateAsync(
                    id: id,
                    name: name,
                    description: description,
                    parentLocationId: null,
                    session: session);

                // Assert
                Assert.Equal(id, result.Id);
                Assert.Equal(name, result.Name);
                Assert.Equal(description, result.Description);
                Assert.Null(result.ParentLocationId);

                // Verify can retrieve by ID
                LocationDbModel? retrieved = await Repository.GetByIdAsync(id, session);
                Assert.NotNull(retrieved);
                Assert.Equal(result.Id, retrieved.Id);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenValidDataWithParent_CreatesLocationWithParent()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string parentId = $"parent-location-{Guid.NewGuid()}";
                string parentName = "Parent Location";

                LocationDbModel parent = await Repository.CreateAsync(
                    id: parentId,
                    name: parentName,
                    description: null,
                    parentLocationId: null,
                    session: session);

                string childId = $"child-location-{Guid.NewGuid()}";
                string childName = "Child Location";

                // Act
                LocationDbModel result = await Repository.CreateAsync(
                    id: childId,
                    name: childName,
                    description: null,
                    parentLocationId: parentId,
                    session: session);

                // Assert
                Assert.Equal(childId, result.Id);
                Assert.Equal(childName, result.Name);
                Assert.Equal(parentId, result.ParentLocationId);

                // Verify can retrieve by ID
                LocationDbModel? retrieved = await Repository.GetByIdAsync(childId, session);
                Assert.NotNull(retrieved);
                Assert.Equal(parentId, retrieved.ParentLocationId);
            }
        }

        [Fact]
        public async Task GetAllAsync_WhenLocationsExist_ReturnsAllLocations()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id1 = $"location-1-{Guid.NewGuid()}";
                string id2 = $"location-2-{Guid.NewGuid()}";
                string id3 = $"location-3-{Guid.NewGuid()}";

                await Repository.CreateAsync(
                    id: id1,
                    name: "Location 1",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await Repository.CreateAsync(
                    id: id2,
                    name: "Location 2",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await Repository.CreateAsync(
                    id: id3,
                    name: "Location 3",
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Act
                IEnumerable<LocationDbModel> result = await Repository.GetAllAsync(session);

                // Assert
                Assert.Contains(result, l => l.Id == id1);
                Assert.Contains(result, l => l.Id == id2);
                Assert.Contains(result, l => l.Id == id3);
                Assert.True(result.Count() >= 3);
            }
        }

        [Fact]
        public async Task GetChildrenAsync_WhenChildrenExist_ReturnsChildren()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string parentId = $"parent-location-{Guid.NewGuid()}";
                string child1Id = $"child-1-{Guid.NewGuid()}";
                string child2Id = $"child-2-{Guid.NewGuid()}";
                string otherId = $"other-location-{Guid.NewGuid()}";

                await Repository.CreateAsync(
                    id: parentId,
                    name: "Parent Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await Repository.CreateAsync(
                    id: child1Id,
                    name: "Child 1",
                    description: null,
                    parentLocationId: parentId,
                    session: session);

                await Repository.CreateAsync(
                    id: child2Id,
                    name: "Child 2",
                    description: null,
                    parentLocationId: parentId,
                    session: session);

                await Repository.CreateAsync(
                    id: otherId,
                    name: "Other Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Act
                IEnumerable<LocationDbModel> result = await Repository.GetChildrenAsync(parentId, session);

                // Assert
                Assert.Equal(2, result.Count());
                Assert.Contains(result, l => l.Id == child1Id);
                Assert.Contains(result, l => l.Id == child2Id);
                Assert.DoesNotContain(result, l => l.Id == otherId);
            }
        }

        [Fact]
        public async Task GetChildrenAsync_WhenNoChildren_ReturnsEmpty()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string parentId = $"parent-location-{Guid.NewGuid()}";

                await Repository.CreateAsync(
                    id: parentId,
                    name: "Parent Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Act
                IEnumerable<LocationDbModel> result = await Repository.GetChildrenAsync(parentId, session);

                // Assert
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenLocationExists_UpdatesLocation()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id = "update-location";
                string originalName = "Original Name";
                string originalDescription = "Original Description";

                await Repository.CreateAsync(
                    id: id,
                    name: originalName,
                    description: originalDescription,
                    parentLocationId: null,
                    session: session);

                string newName = "Updated Name";
                string newDescription = "Updated Description";

                // Act
                bool result = await Repository.UpdateAsync(
                    id: id,
                    name: newName,
                    description: newDescription,
                    parentLocationId: null,
                    session: session);

                // Assert
                Assert.True(result);

                // Verify location was updated
                LocationDbModel? retrieved = await Repository.GetByIdAsync(id, session);
                Assert.NotNull(retrieved);
                Assert.Equal(newName, retrieved.Name);
                Assert.Equal(newDescription, retrieved.Description);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenUpdatingParent_UpdatesParent()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string parent1Id = $"parent-1-{Guid.NewGuid()}";
                string parent2Id = $"parent-2-{Guid.NewGuid()}";
                string childId = $"child-location-{Guid.NewGuid()}";

                await Repository.CreateAsync(
                    id: parent1Id,
                    name: "Parent 1",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await Repository.CreateAsync(
                    id: parent2Id,
                    name: "Parent 2",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await Repository.CreateAsync(
                    id: childId,
                    name: "Child Location",
                    description: null,
                    parentLocationId: parent1Id,
                    session: session);

                // Act
                bool result = await Repository.UpdateAsync(
                    id: childId,
                    name: "Child Location",
                    description: null,
                    parentLocationId: parent2Id,
                    session: session);

                // Assert
                Assert.True(result);

                // Verify parent was updated
                LocationDbModel? retrieved = await Repository.GetByIdAsync(childId, session);
                Assert.NotNull(retrieved);
                Assert.Equal(parent2Id, retrieved.ParentLocationId);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenLocationDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string nonExistentId = "nonexistent-location";
                string name = "Some Name";

                // Act
                bool result = await Repository.UpdateAsync(
                    id: nonExistentId,
                    name: name,
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task DeleteAsync_WhenLocationExists_DeletesLocation()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id = "delete-location";
                string name = "Delete Location";

                await Repository.CreateAsync(
                    id: id,
                    name: name,
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Act
                bool result = await Repository.DeleteAsync(id, session);

                // Assert
                Assert.True(result);

                // Verify location was deleted
                LocationDbModel? retrieved = await Repository.GetByIdAsync(id, session);
                Assert.Null(retrieved);
            }
        }

        [Fact]
        public async Task DeleteAsync_WhenLocationDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string nonExistentId = "nonexistent-location";

                // Act
                bool result = await Repository.DeleteAsync(nonExistentId, session);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task DeleteAsync_WhenLocationHasChildren_ThrowsDatabaseConstraintViolationException()
        {
            // Arrange - use unique IDs to avoid conflicts with other tests
            string parentId = $"parent-location-{Guid.NewGuid()}";
            string childId = $"child-location-{Guid.NewGuid()}";

            await using (IDbSession createSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                await Repository.CreateAsync(
                    id: parentId,
                    name: "Parent Location",
                    description: null,
                    parentLocationId: null,
                    session: createSession);

                await Repository.CreateAsync(
                    id: childId,
                    name: "Child Location",
                    description: null,
                    parentLocationId: parentId,
                    session: createSession);

                await createSession.CommitAsync();
            }

            // Act & Assert - deletion should fail with user-friendly message
            await using (IDbSession deleteSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                DatabaseConstraintViolationException exception = await Assert.ThrowsAsync<DatabaseConstraintViolationException>(async () =>
                {
                    await Repository.DeleteAsync(parentId, deleteSession);
                });

                Assert.Contains("child locations", exception.Message);
            }

            // Verify parent still exists (deletion was prevented) - use new session
            await using (IDbSession verifySession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                LocationDbModel? parent = await Repository.GetByIdAsync(parentId, verifySession);
                Assert.NotNull(parent);

                // Verify child still exists with parent reference
                LocationDbModel? child = await Repository.GetByIdAsync(childId, verifySession);
                Assert.NotNull(child);
                Assert.Equal(parentId, child.ParentLocationId);
            }
        }

        [Fact]
        public async Task DeleteAsync_WhenLocationHasItems_ThrowsDatabaseConstraintViolationException()
        {
            // Arrange
            string locationId = $"location-{Guid.NewGuid()}";
            Guid itemId = Guid.NewGuid();

            ItemRepository itemRepository = new ItemRepository(TestDatabaseManager.DataSource, SessionFactory);

            await using (IDbSession createSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                await Repository.CreateAsync(
                    id: locationId,
                    name: "Location",
                    description: null,
                    parentLocationId: null,
                    session: createSession);

                await itemRepository.CreateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: createSession);

                await createSession.CommitAsync();
            }

            // Act & Assert - deletion should fail with user-friendly message
            await using (IDbSession deleteSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                DatabaseConstraintViolationException exception = await Assert.ThrowsAsync<DatabaseConstraintViolationException>(async () =>
                {
                    await Repository.DeleteAsync(locationId, deleteSession);
                });

                Assert.Contains("contains items", exception.Message);
            }

            // Verify location still exists (deletion was prevented)
            await using (IDbSession verifySession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                LocationDbModel? location = await Repository.GetByIdAsync(locationId, verifySession);
                Assert.NotNull(location);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenParentDoesNotExist_ThrowsDatabaseConstraintViolationException()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id = $"location-{Guid.NewGuid()}";
                string name = "Location";
                string invalidParentId = "nonexistent-parent";

                // Act & Assert
                DatabaseConstraintViolationException exception = await Assert.ThrowsAsync<DatabaseConstraintViolationException>(async () =>
                {
                    await Repository.CreateAsync(
                        id: id,
                        name: name,
                        description: null,
                        parentLocationId: invalidParentId,
                        session: session);
                });

                Assert.Contains("Parent location does not exist", exception.Message);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenParentDoesNotExist_ThrowsDatabaseConstraintViolationException()
        {
            // Arrange
            string locationId = $"location-{Guid.NewGuid()}";
            string invalidParentId = "nonexistent-parent";

            await using (IDbSession createSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                await Repository.CreateAsync(
                    id: locationId,
                    name: "Location",
                    description: null,
                    parentLocationId: null,
                    session: createSession);

                await createSession.CommitAsync();
            }

            // Act & Assert
            await using (IDbSession updateSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                DatabaseConstraintViolationException exception = await Assert.ThrowsAsync<DatabaseConstraintViolationException>(async () =>
                {
                    await Repository.UpdateAsync(
                        id: locationId,
                        name: "Location",
                        description: null,
                        parentLocationId: invalidParentId,
                        session: updateSession);
                });

                Assert.Contains("Parent location does not exist", exception.Message);
            }
        }

        [Fact]
        public async Task ExistsAsync_WhenLocationExists_ReturnsTrue()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string id = "exists-location";

                await Repository.CreateAsync(
                    id: id,
                    name: "Exists Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Act
                bool result = await Repository.ExistsAsync(id, session);

                // Assert
                Assert.True(result);
            }
        }

        [Fact]
        public async Task ExistsAsync_WhenLocationDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string nonExistentId = "nonexistent-location";

                // Act
                bool result = await Repository.ExistsAsync(nonExistentId, session);

                // Assert
                Assert.False(result);
            }
        }
    }
}
