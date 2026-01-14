using System.Text.Json;
using EasyReasy.Database;
using Ordning.Server.Database;
using Ordning.Server.Items.Models;
using Ordning.Server.Items.Repositories;
using Ordning.Server.Locations.Repositories;
using Ordning.Server.Tests.TestUtilities;

namespace Ordning.Server.Tests.Repositories
{
    /// <summary>
    /// Integration tests for ItemRepository.
    /// </summary>
    public class ItemRepositoryTests : RepositoryTestBase
    {
        private ItemRepository Repository { get; set; } = null!;
        private LocationRepository LocationRepository { get; set; } = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Repository = new ItemRepository(TestDatabaseManager.DataSource, SessionFactory);
            LocationRepository = new LocationRepository(TestDatabaseManager.DataSource, SessionFactory);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemExists_ReturnsItem()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                string name = "Test Item";
                string description = "Test Description";
                Dictionary<string, string> properties = new Dictionary<string, string> { { "key1", "value1" } };

                ItemDbModel createdItem = await Repository.CreateAsync(
                    id: itemId,
                    name: name,
                    description: description,
                    locationId: locationId,
                    properties: properties,
                    session: session);

                // Act
                ItemDbModel? result = await Repository.GetByIdAsync(itemId, session);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(createdItem.Id, result.Id);
                Assert.Equal(name, result.Name);
                Assert.Equal(description, result.Description);
                Assert.Equal(locationId, result.LocationId);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemDoesNotExist_ReturnsNull()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid itemId = Guid.NewGuid();

                // Act
                ItemDbModel? result = await Repository.GetByIdAsync(itemId, session);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetAllAsync_WhenItemsExist_ReturnsAllItems()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();
                Guid itemId3 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Item 1",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Item 2",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId3,
                    name: "Item 3",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                IEnumerable<ItemDbModel> result = await Repository.GetAllAsync(session);

                // Assert
                Assert.Contains(result, i => i.Id == itemId1);
                Assert.Contains(result, i => i.Id == itemId2);
                Assert.Contains(result, i => i.Id == itemId3);
                Assert.True(result.Count() >= 3);

                // Verify ordering by name
                List<ItemDbModel> resultList = result.Where(i => i.Id == itemId1 || i.Id == itemId2 || i.Id == itemId3).ToList();
                Assert.Equal(3, resultList.Count);
                Assert.Equal("Item 1", resultList[0].Name);
                Assert.Equal("Item 2", resultList[1].Name);
                Assert.Equal("Item 3", resultList[2].Name);
            }
        }

        [Fact]
        public async Task GetByLocationIdAsync_WhenItemsExistInLocation_ReturnsItems()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId1 = $"location-1-{Guid.NewGuid()}";
                string locationId2 = $"location-2-{Guid.NewGuid()}";

                await LocationRepository.CreateAsync(
                    id: locationId1,
                    name: "Location 1",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await LocationRepository.CreateAsync(
                    id: locationId2,
                    name: "Location 2",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();
                Guid itemId3 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Item 1",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Item 2",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId3,
                    name: "Item 3",
                    description: null,
                    locationId: locationId2,
                    properties: null,
                    session: session);

                // Act
                IEnumerable<ItemDbModel> result = await Repository.GetByLocationIdAsync(locationId1, session);

                // Assert
                Assert.Equal(2, result.Count());
                Assert.Contains(result, i => i.Id == itemId1);
                Assert.Contains(result, i => i.Id == itemId2);
                Assert.DoesNotContain(result, i => i.Id == itemId3);

                // Verify ordering by name
                List<ItemDbModel> resultList = result.ToList();
                Assert.Equal("Item 1", resultList[0].Name);
                Assert.Equal("Item 2", resultList[1].Name);
            }
        }

        [Fact]
        public async Task GetByLocationIdAsync_WhenNoItemsInLocation_ReturnsEmpty()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"empty-location-{Guid.NewGuid()}";

                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Empty Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Act
                IEnumerable<ItemDbModel> result = await Repository.GetByLocationIdAsync(locationId, session);

                // Assert
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenValidData_CreatesItem()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                string name = "New Item";
                string description = "New Description";

                // Act
                ItemDbModel result = await Repository.CreateAsync(
                    id: itemId,
                    name: name,
                    description: description,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Assert
                Assert.Equal(itemId, result.Id);
                Assert.Equal(name, result.Name);
                Assert.Equal(description, result.Description);
                Assert.Equal(locationId, result.LocationId);

                // Verify can retrieve by ID
                ItemDbModel? retrieved = await Repository.GetByIdAsync(itemId, session);
                Assert.NotNull(retrieved);
                Assert.Equal(result.Id, retrieved.Id);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenValidDataWithProperties_CreatesItemWithProperties()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "color", "red" },
                    { "size", "large" },
                    { "weight", "5kg" }
                };

                // Act
                ItemDbModel result = await Repository.CreateAsync(
                    id: itemId,
                    name: "Item with Properties",
                    description: null,
                    locationId: locationId,
                    properties: properties,
                    session: session);

                // Assert
                Assert.Equal(itemId, result.Id);
                Assert.NotEmpty(result.PropertiesJson);

                // Verify properties are stored correctly
                ItemDbModel? retrieved = await Repository.GetByIdAsync(itemId, session);
                Assert.NotNull(retrieved);
                Assert.NotEmpty(retrieved.PropertiesJson);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenValidDataWithoutProperties_CreatesItemWithEmptyProperties()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();

                // Act
                ItemDbModel result = await Repository.CreateAsync(
                    id: itemId,
                    name: "Item without Properties",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Assert
                Assert.Equal(itemId, result.Id);
                Assert.Equal("{}", result.PropertiesJson);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenItemExists_UpdatesItem()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                string originalName = "Original Name";
                string originalDescription = "Original Description";

                await Repository.CreateAsync(
                    id: itemId,
                    name: originalName,
                    description: originalDescription,
                    locationId: locationId,
                    properties: null,
                    session: session);

                string newName = "Updated Name";
                string newDescription = "Updated Description";

                // Act
                bool result = await Repository.UpdateAsync(
                    id: itemId,
                    name: newName,
                    description: newDescription,
                    properties: null,
                    session: session);

                // Assert
                Assert.True(result);

                // Verify item was updated
                ItemDbModel? retrieved = await Repository.GetByIdAsync(itemId, session);
                Assert.NotNull(retrieved);
                Assert.Equal(newName, retrieved.Name);
                Assert.Equal(newDescription, retrieved.Description);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentId = Guid.NewGuid();
                string name = "Some Name";

                // Act
                bool result = await Repository.UpdateAsync(
                    id: nonExistentId,
                    name: name,
                    description: null,
                    properties: null,
                    session: session);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenUpdatingProperties_UpdatesProperties()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                Dictionary<string, string> originalProperties = new Dictionary<string, string> { { "key1", "value1" } };

                await Repository.CreateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    locationId: locationId,
                    properties: originalProperties,
                    session: session);

                Dictionary<string, string> newProperties = new Dictionary<string, string>
                {
                    { "key1", "updated-value1" },
                    { "key2", "value2" }
                };

                // Act
                bool result = await Repository.UpdateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    properties: newProperties,
                    session: session);

                // Assert
                Assert.True(result);

                // Verify properties were updated
                ItemDbModel? retrieved = await Repository.GetByIdAsync(itemId, session);
                Assert.NotNull(retrieved);
                Assert.NotEmpty(retrieved.PropertiesJson);

                // Verify actual property content
                Dictionary<string, string>? deserializedProperties = JsonSerializer.Deserialize<Dictionary<string, string>>(retrieved.PropertiesJson);
                Assert.NotNull(deserializedProperties);
                Assert.Equal(2, deserializedProperties.Count);
                Assert.Equal("updated-value1", deserializedProperties["key1"]);
                Assert.Equal("value2", deserializedProperties["key2"]);
            }
        }

        [Fact]
        public async Task UpdateAsync_WhenPropertiesIsNull_ClearsPropertiesToEmpty()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                Dictionary<string, string> originalProperties = new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                };

                await Repository.CreateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    locationId: locationId,
                    properties: originalProperties,
                    session: session);

                // Act
                bool result = await Repository.UpdateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    properties: null,
                    session: session);

                // Assert
                Assert.True(result);

                // Verify properties were cleared to empty
                ItemDbModel? retrieved = await Repository.GetByIdAsync(itemId, session);
                Assert.NotNull(retrieved);
                Assert.Equal("{}", retrieved.PropertiesJson);

                // Verify deserialized properties are empty
                Dictionary<string, string>? deserializedProperties = JsonSerializer.Deserialize<Dictionary<string, string>>(retrieved.PropertiesJson);
                Assert.NotNull(deserializedProperties);
                Assert.Empty(deserializedProperties);
            }
        }

        [Fact]
        public async Task DeleteAsync_WhenItemExists_DeletesItem()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                string name = "Delete Item";

                await Repository.CreateAsync(
                    id: itemId,
                    name: name,
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                bool result = await Repository.DeleteAsync(itemId, session);

                // Assert
                Assert.True(result);

                // Verify item was deleted
                ItemDbModel? retrieved = await Repository.GetByIdAsync(itemId, session);
                Assert.Null(retrieved);
            }
        }

        [Fact]
        public async Task DeleteAsync_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentId = Guid.NewGuid();

                // Act
                bool result = await Repository.DeleteAsync(nonExistentId, session);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task ExistsAsync_WhenItemExists_ReturnsTrue()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId,
                    name: "Exists Item",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                bool result = await Repository.ExistsAsync(itemId, session);

                // Assert
                Assert.True(result);
            }
        }

        [Fact]
        public async Task ExistsAsync_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid nonExistentId = Guid.NewGuid();

                // Act
                bool result = await Repository.ExistsAsync(nonExistentId, session);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task MoveItemsAsync_WhenSingleItem_MovesItem()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId1 = $"location-1-{Guid.NewGuid()}";
                string locationId2 = $"location-2-{Guid.NewGuid()}";

                await LocationRepository.CreateAsync(
                    id: locationId1,
                    name: "Location 1",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await LocationRepository.CreateAsync(
                    id: locationId2,
                    name: "Location 2",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                // Act
                int result = await Repository.MoveItemsAsync(new[] { itemId }, locationId2, session);

                // Assert
                Assert.Equal(1, result);

                // Verify item was moved
                ItemDbModel? retrieved = await Repository.GetByIdAsync(itemId, session);
                Assert.NotNull(retrieved);
                Assert.Equal(locationId2, retrieved.LocationId);
            }
        }

        [Fact]
        public async Task MoveItemsAsync_WhenMultipleItems_MovesAllItems()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId1 = $"location-1-{Guid.NewGuid()}";
                string locationId2 = $"location-2-{Guid.NewGuid()}";

                await LocationRepository.CreateAsync(
                    id: locationId1,
                    name: "Location 1",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await LocationRepository.CreateAsync(
                    id: locationId2,
                    name: "Location 2",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();
                Guid itemId3 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Item 1",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Item 2",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId3,
                    name: "Item 3",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                // Act
                int result = await Repository.MoveItemsAsync(new[] { itemId1, itemId2, itemId3 }, locationId2, session);

                // Assert
                Assert.Equal(3, result);

                // Verify all items were moved
                ItemDbModel? retrieved1 = await Repository.GetByIdAsync(itemId1, session);
                ItemDbModel? retrieved2 = await Repository.GetByIdAsync(itemId2, session);
                ItemDbModel? retrieved3 = await Repository.GetByIdAsync(itemId3, session);

                Assert.NotNull(retrieved1);
                Assert.NotNull(retrieved2);
                Assert.NotNull(retrieved3);
                Assert.Equal(locationId2, retrieved1.LocationId);
                Assert.Equal(locationId2, retrieved2.LocationId);
                Assert.Equal(locationId2, retrieved3.LocationId);
            }
        }

        [Fact]
        public async Task MoveItemsAsync_WhenItemsDoNotExist_ReturnsZero()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"location-{Guid.NewGuid()}";

                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid nonExistentId1 = Guid.NewGuid();
                Guid nonExistentId2 = Guid.NewGuid();

                // Act
                int result = await Repository.MoveItemsAsync(new[] { nonExistentId1, nonExistentId2 }, locationId, session);

                // Assert
                Assert.Equal(0, result);
            }
        }

        [Fact]
        public async Task MoveItemsAsync_WhenMixedExistingAndNonExistingIds_OnlyMovesExistingItems()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId1 = $"location-1-{Guid.NewGuid()}";
                string locationId2 = $"location-2-{Guid.NewGuid()}";

                await LocationRepository.CreateAsync(
                    id: locationId1,
                    name: "Location 1",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await LocationRepository.CreateAsync(
                    id: locationId2,
                    name: "Location 2",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid existingItemId1 = Guid.NewGuid();
                Guid existingItemId2 = Guid.NewGuid();
                Guid nonExistentId1 = Guid.NewGuid();
                Guid nonExistentId2 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: existingItemId1,
                    name: "Item 1",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: existingItemId2,
                    name: "Item 2",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: session);

                // Act
                int result = await Repository.MoveItemsAsync(new[] { existingItemId1, nonExistentId1, existingItemId2, nonExistentId2 }, locationId2, session);

                // Assert
                Assert.Equal(2, result);

                // Verify only existing items were moved
                ItemDbModel? retrieved1 = await Repository.GetByIdAsync(existingItemId1, session);
                ItemDbModel? retrieved2 = await Repository.GetByIdAsync(existingItemId2, session);

                Assert.NotNull(retrieved1);
                Assert.NotNull(retrieved2);
                Assert.Equal(locationId2, retrieved1.LocationId);
                Assert.Equal(locationId2, retrieved2.LocationId);
            }
        }

        [Fact]
        public async Task MoveItemsAsync_WhenEmptyIdList_ReturnsZero()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"location-{Guid.NewGuid()}";

                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Act
                int result = await Repository.MoveItemsAsync(Array.Empty<Guid>(), locationId, session);

                // Assert
                Assert.Equal(0, result);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertiesAreSerialized_StoresAsJsonb()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "color", "blue" },
                    { "material", "wood" }
                };

                // Act
                ItemDbModel result = await Repository.CreateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    locationId: locationId,
                    properties: properties,
                    session: session);

                // Assert
                Assert.NotEmpty(result.PropertiesJson);
                Assert.Contains("color", result.PropertiesJson);
                Assert.Contains("blue", result.PropertiesJson);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WhenPropertiesExist_DeserializesProperties()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                };

                await Repository.CreateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    locationId: locationId,
                    properties: properties,
                    session: session);

                // Act
                ItemDbModel? result = await Repository.GetByIdAsync(itemId, session);

                // Assert
                Assert.NotNull(result);
                Assert.NotEmpty(result.PropertiesJson);
                
                // Verify properties can be deserialized
                Item domainItem = result.ToDomainItem();
                Assert.Equal(2, domainItem.Properties.Count);
                Assert.Equal("value1", domainItem.Properties["key1"]);
                Assert.Equal("value2", domainItem.Properties["key2"]);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenLocationDoesNotExist_ThrowsDatabaseConstraintViolationException()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                Guid itemId = Guid.NewGuid();
                string name = "Item";
                string invalidLocationId = "nonexistent-location";

                // Act & Assert
                DatabaseConstraintViolationException exception = await Assert.ThrowsAsync<DatabaseConstraintViolationException>(async () =>
                {
                    await Repository.CreateAsync(
                        id: itemId,
                        name: name,
                        description: null,
                        locationId: invalidLocationId,
                        properties: null,
                        session: session);
                });

                Assert.Contains("Location does not exist", exception.Message);
            }
        }

        [Fact]
        public async Task MoveItemsAsync_WhenLocationDoesNotExist_ThrowsDatabaseConstraintViolationException()
        {
            // Arrange
            string locationId1 = $"location-1-{Guid.NewGuid()}";
            string invalidLocationId = "nonexistent-location";
            Guid itemId = Guid.NewGuid();

            await using (IDbSession createSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                await LocationRepository.CreateAsync(
                    id: locationId1,
                    name: "Location 1",
                    description: null,
                    parentLocationId: null,
                    session: createSession);

                await Repository.CreateAsync(
                    id: itemId,
                    name: "Item",
                    description: null,
                    locationId: locationId1,
                    properties: null,
                    session: createSession);

                await createSession.CommitAsync();
            }

            // Act & Assert
            await using (IDbSession moveSession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                DatabaseConstraintViolationException exception = await Assert.ThrowsAsync<DatabaseConstraintViolationException>(async () =>
                {
                    await Repository.MoveItemsAsync(
                        itemIds: new[] { itemId },
                        newLocationId: invalidLocationId,
                        session: moveSession);
                });

                Assert.Contains("Location does not exist", exception.Message);
            }

            // Verify item was not moved
            await using (IDbSession verifySession = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                ItemDbModel? item = await Repository.GetByIdAsync(itemId, verifySession);
                Assert.NotNull(item);
                Assert.Equal(locationId1, item.LocationId);
            }
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTermMatchesName_ReturnsMatchingItems()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();
                Guid itemId3 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Hammer Tool",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Screwdriver",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId3,
                    name: "Hammer Drill",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("hammer", 0, 10, session);

                // Assert
                Assert.True(totalCount >= 2);
                List<ItemDbModel> resultsList = results.ToList();
                Assert.Contains(resultsList, i => i.Id == itemId1);
                Assert.Contains(resultsList, i => i.Id == itemId3);
            }
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTermMatchesDescription_ReturnsMatchingItems()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Item One",
                    description: "This is a power tool",
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Item Two",
                    description: "This is a manual tool",
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("power", 0, 10, session);

                // Assert
                Assert.True(totalCount >= 1);
                List<ItemDbModel> resultsList = results.ToList();
                Assert.Contains(resultsList, i => i.Id == itemId1);
            }
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTermMatchesProperties_ReturnsMatchingItems()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();

                Dictionary<string, string> properties1 = new Dictionary<string, string>
                {
                    { "color", "red" },
                    { "brand", "Bosch" }
                };

                Dictionary<string, string> properties2 = new Dictionary<string, string>
                {
                    { "color", "blue" },
                    { "brand", "Makita" }
                };

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Tool One",
                    description: null,
                    locationId: locationId,
                    properties: properties1,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Tool Two",
                    description: null,
                    locationId: locationId,
                    properties: properties2,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("Bosch", 0, 10, session);

                // Assert
                Assert.True(totalCount >= 1);
                List<ItemDbModel> resultsList = results.ToList();
                Assert.Contains(resultsList, i => i.Id == itemId1);
            }
        }

        [Fact]
        public async Task SearchAsync_WhenPhraseMatch_ReturnsHigherRankedResults()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Hammer Drill",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Hammer",
                    description: "Drill attachment",
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("hammer drill", 0, 10, session);

                // Assert
                Assert.True(totalCount >= 2);
                List<ItemDbModel> resultsList = results.ToList();
                // Phrase match should rank higher, so "Hammer Drill" should appear first
                Assert.Equal(itemId1, resultsList[0].Id);
            }
        }

        [Fact]
        public async Task SearchAsync_WhenBothWordsMatch_ReturnsMediumRankedResults()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Hammer",
                    description: "Drill",
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Hammer",
                    description: "Tool",
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("hammer drill", 0, 10, session);

                // Assert
                Assert.True(totalCount >= 1);
                List<ItemDbModel> resultsList = results.ToList();
                Assert.Contains(resultsList, i => i.Id == itemId1);
            }
        }

        [Fact]
        public async Task SearchAsync_WhenNoMatches_ReturnsEmpty()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                await Repository.CreateAsync(
                    id: Guid.NewGuid(),
                    name: "Hammer",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("nonexistentterm12345", 0, 10, session);

                // Assert
                Assert.Equal(0, totalCount);
                Assert.Empty(results);
            }
        }

        [Fact]
        public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                // Create multiple items with same search term
                for (int i = 0; i < 5; i++)
                {
                    await Repository.CreateAsync(
                        id: Guid.NewGuid(),
                        name: $"Hammer {i}",
                        description: null,
                        locationId: locationId,
                        properties: null,
                        session: session);
                }

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("hammer", 2, 2, session);

                // Assert
                Assert.True(totalCount >= 5);
                Assert.Equal(2, results.Count());
            }
        }

        [Fact]
        public async Task SearchAsync_WithPagination_ReturnsCorrectTotalCount()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId1 = Guid.NewGuid();
                Guid itemId2 = Guid.NewGuid();
                Guid itemId3 = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId1,
                    name: "Hammer One",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId2,
                    name: "Hammer Two",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                await Repository.CreateAsync(
                    id: itemId3,
                    name: "Hammer Three",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("hammer", 0, 2, session);

                // Assert
                Assert.Equal(2, results.Count());
                Assert.True(totalCount >= 3);
            }
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTermIsCaseInsensitive_ReturnsMatches()
        {
            // Arrange
            await using (IDbSession session = await TestDatabaseManager.CreateTransactionSessionAsync())
            {
                string locationId = $"test-location-{Guid.NewGuid()}";
                await LocationRepository.CreateAsync(
                    id: locationId,
                    name: "Test Location",
                    description: null,
                    parentLocationId: null,
                    session: session);

                Guid itemId = Guid.NewGuid();

                await Repository.CreateAsync(
                    id: itemId,
                    name: "Hammer Tool",
                    description: null,
                    locationId: locationId,
                    properties: null,
                    session: session);

                // Act
                (IEnumerable<ItemDbModel> results, int totalCount) = await Repository.SearchAsync("HAMMER", 0, 10, session);

                // Assert
                Assert.True(totalCount >= 1);
                List<ItemDbModel> resultsList = results.ToList();
                Assert.Contains(resultsList, i => i.Id == itemId);
            }
        }
    }
}
