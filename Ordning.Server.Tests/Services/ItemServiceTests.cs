using EasyReasy.Database;
using Moq;
using Ordning.Server.Items.Models;
using Ordning.Server.Items.Repositories;
using Ordning.Server.Items.Services;
using Ordning.Server.Locations.Repositories;

namespace Ordning.Server.Tests.Services
{
    /// <summary>
    /// Unit tests for ItemService.
    /// </summary>
    public class ItemServiceTests
    {
        private Mock<IItemRepository> MockItemRepository { get; set; } = null!;
        private Mock<ILocationRepository> MockLocationRepository { get; set; } = null!;
        private ItemService Service { get; set; } = null!;

        public ItemServiceTests()
        {
            MockItemRepository = new Mock<IItemRepository>();
            MockLocationRepository = new Mock<ILocationRepository>();
            Service = new ItemService(MockItemRepository.Object, MockLocationRepository.Object);
        }

        [Fact]
        public async Task GetItemByIdAsync_WhenItemExists_ReturnsItem()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();
            ItemDbModel itemDbModel = new ItemDbModel
            {
                Id = itemId,
                Name = "Test Item",
                Description = "Test Description",
                LocationId = "test-location",
                PropertiesJson = "{}"
            };

            MockItemRepository
                .Setup(r => r.GetByIdAsync(itemId, null))
                .ReturnsAsync(itemDbModel);

            // Act
            Item? result = await Service.GetItemByIdAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(itemId, result.Id);
            Assert.Equal(itemDbModel.Name, result.Name);
            Assert.Equal(itemDbModel.Description, result.Description);
            MockItemRepository.Verify(r => r.GetByIdAsync(itemId, null), Times.Once);
        }

        [Fact]
        public async Task GetItemByIdAsync_WhenItemDoesNotExist_ReturnsNull()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();

            MockItemRepository
                .Setup(r => r.GetByIdAsync(itemId, null))
                .ReturnsAsync((ItemDbModel?)null);

            // Act
            Item? result = await Service.GetItemByIdAsync(itemId);

            // Assert
            Assert.Null(result);
            MockItemRepository.Verify(r => r.GetByIdAsync(itemId, null), Times.Once);
        }

        [Fact]
        public async Task GetAllItemsAsync_WhenCalled_ReturnsAllItems()
        {
            // Arrange
            IEnumerable<ItemDbModel> itemDbModels = new[]
            {
                new ItemDbModel { Id = Guid.NewGuid(), Name = "Item 1", Description = null, LocationId = "location-1", PropertiesJson = "{}" },
                new ItemDbModel { Id = Guid.NewGuid(), Name = "Item 2", Description = null, LocationId = "location-2", PropertiesJson = "{}" }
            };

            MockItemRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(itemDbModels);

            // Act
            IEnumerable<Item> result = await Service.GetAllItemsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            MockItemRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetItemsByLocationIdAsync_WhenCalled_ReturnsItemsInLocation()
        {
            // Arrange
            string locationId = "test-location";
            IEnumerable<ItemDbModel> items = new[]
            {
                new ItemDbModel { Id = Guid.NewGuid(), Name = "Item 1", Description = null, LocationId = locationId, PropertiesJson = "{}" },
                new ItemDbModel { Id = Guid.NewGuid(), Name = "Item 2", Description = null, LocationId = locationId, PropertiesJson = "{}" }
            };

            MockItemRepository
                .Setup(r => r.GetByLocationIdAsync(locationId, null))
                .ReturnsAsync(items);

            // Act
            IEnumerable<Item> result = await Service.GetItemsByLocationIdAsync(locationId);

            // Assert
            Assert.Equal(2, result.Count());
            MockItemRepository.Verify(r => r.GetByLocationIdAsync(locationId, null), Times.Once);
        }

        [Fact]
        public async Task CreateItemAsync_WhenValidData_CreatesItem()
        {
            // Arrange
            string locationId = "test-location";
            string name = "New Item";
            string description = "New Description";
            Guid itemId = Guid.NewGuid();

            ItemDbModel createdItem = new ItemDbModel
            {
                Id = itemId,
                Name = name,
                Description = description,
                LocationId = locationId,
                PropertiesJson = "{}"
            };

            MockLocationRepository
                .Setup(r => r.ExistsAsync(locationId, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.CreateAsync(It.IsAny<Guid>(), name, description, locationId, null, null))
                .ReturnsAsync(createdItem);

            // Act
            Item result = await Service.CreateItemAsync(name, locationId, description);

            // Assert
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(locationId, result.LocationId);
            MockLocationRepository.Verify(r => r.ExistsAsync(locationId, null), Times.Once);
            MockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Guid>(), name, description, locationId, null, null), Times.Once);
        }

        [Fact]
        public async Task CreateItemAsync_WhenLocationDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            string locationId = "nonexistent-location";
            string name = "Item Name";

            MockLocationRepository
                .Setup(r => r.ExistsAsync(locationId, null))
                .ReturnsAsync(false);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => Service.CreateItemAsync(name, locationId));

            Assert.Contains("does not exist", exception.Message);
            MockLocationRepository.Verify(r => r.ExistsAsync(locationId, null), Times.Once);
            MockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task CreateItemAsync_WhenValidDataWithProperties_CreatesItemWithProperties()
        {
            // Arrange
            string locationId = "test-location";
            string name = "New Item";
            Dictionary<string, string> properties = new Dictionary<string, string> { { "key1", "value1" } };
            Guid itemId = Guid.NewGuid();

            ItemDbModel createdItem = new ItemDbModel
            {
                Id = itemId,
                Name = name,
                Description = null,
                LocationId = locationId,
                PropertiesJson = "{\"key1\":\"value1\"}"
            };

            MockLocationRepository
                .Setup(r => r.ExistsAsync(locationId, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.CreateAsync(It.IsAny<Guid>(), name, null, locationId, properties, null))
                .ReturnsAsync(createdItem);

            // Act
            Item result = await Service.CreateItemAsync(name, locationId, null, properties);

            // Assert
            Assert.Equal(name, result.Name);
            Assert.Equal(locationId, result.LocationId);
            Assert.Single(result.Properties);
            MockLocationRepository.Verify(r => r.ExistsAsync(locationId, null), Times.Once);
            MockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Guid>(), name, null, locationId, properties, null), Times.Once);
        }

        [Fact]
        public async Task CreateItemAsync_WhenValidDataWithoutProperties_CreatesItemWithEmptyProperties()
        {
            // Arrange
            string locationId = "test-location";
            string name = "New Item";
            Guid itemId = Guid.NewGuid();

            ItemDbModel createdItem = new ItemDbModel
            {
                Id = itemId,
                Name = name,
                Description = null,
                LocationId = locationId,
                PropertiesJson = "{}"
            };

            MockLocationRepository
                .Setup(r => r.ExistsAsync(locationId, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.CreateAsync(It.IsAny<Guid>(), name, null, locationId, null, null))
                .ReturnsAsync(createdItem);

            // Act
            Item result = await Service.CreateItemAsync(name, locationId);

            // Assert
            Assert.Equal(name, result.Name);
            Assert.Equal(locationId, result.LocationId);
            Assert.Empty(result.Properties);
            MockLocationRepository.Verify(r => r.ExistsAsync(locationId, null), Times.Once);
            MockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Guid>(), name, null, locationId, null, null), Times.Once);
        }

        [Fact]
        public async Task UpdateItemAsync_WhenValidData_UpdatesItem()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();
            string newName = "Updated Name";
            string newDescription = "Updated Description";

            ItemDbModel existingItem = new ItemDbModel
            {
                Id = itemId,
                Name = "Original Name",
                Description = "Original Description",
                LocationId = "test-location",
                PropertiesJson = "{}"
            };

            ItemDbModel updatedItem = new ItemDbModel
            {
                Id = itemId,
                Name = newName,
                Description = newDescription,
                LocationId = "test-location",
                PropertiesJson = "{}"
            };

            MockItemRepository
                .Setup(r => r.GetByIdAsync(itemId, null))
                .ReturnsAsync(existingItem);

            MockItemRepository
                .Setup(r => r.UpdateAsync(itemId, newName, newDescription, null, null))
                .ReturnsAsync(true);

            MockItemRepository
                .SetupSequence(r => r.GetByIdAsync(itemId, null))
                .ReturnsAsync(existingItem)
                .ReturnsAsync(updatedItem);

            // Act
            Item result = await Service.UpdateItemAsync(itemId, newName, newDescription);

            // Assert
            Assert.Equal(itemId, result.Id);
            Assert.Equal(newName, result.Name);
            Assert.Equal(newDescription, result.Description);
            MockItemRepository.Verify(r => r.GetByIdAsync(itemId, null), Times.Exactly(2));
            MockItemRepository.Verify(r => r.UpdateAsync(itemId, newName, newDescription, null, null), Times.Once);
        }

        [Fact]
        public async Task UpdateItemAsync_WhenItemDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();
            string name = "Item Name";

            MockItemRepository
                .Setup(r => r.GetByIdAsync(itemId, null))
                .ReturnsAsync((ItemDbModel?)null);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.UpdateItemAsync(itemId, name));

            Assert.Contains("does not exist", exception.Message);
            MockItemRepository.Verify(r => r.GetByIdAsync(itemId, null), Times.Once);
            MockItemRepository.Verify(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task UpdateItemAsync_WhenUpdatingProperties_UpdatesProperties()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();
            string name = "Item";
            Dictionary<string, string> newProperties = new Dictionary<string, string> { { "key1", "value1" } };

            ItemDbModel existingItem = new ItemDbModel
            {
                Id = itemId,
                Name = name,
                Description = null,
                LocationId = "test-location",
                PropertiesJson = "{}"
            };

            ItemDbModel updatedItem = new ItemDbModel
            {
                Id = itemId,
                Name = name,
                Description = null,
                LocationId = "test-location",
                PropertiesJson = "{\"key1\":\"value1\"}"
            };

            MockItemRepository
                .SetupSequence(r => r.GetByIdAsync(itemId, null))
                .ReturnsAsync(existingItem)
                .ReturnsAsync(updatedItem);

            MockItemRepository
                .Setup(r => r.UpdateAsync(itemId, name, null, newProperties, null))
                .ReturnsAsync(true);

            // Act
            Item result = await Service.UpdateItemAsync(itemId, name, null, newProperties);

            // Assert
            Assert.Equal(itemId, result.Id);
            Assert.Single(result.Properties);
            MockItemRepository.Verify(r => r.UpdateAsync(itemId, name, null, newProperties, null), Times.Once);
        }

        [Fact]
        public async Task DeleteItemAsync_WhenItemExists_ReturnsTrue()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();

            MockItemRepository
                .Setup(r => r.DeleteAsync(itemId, null))
                .ReturnsAsync(true);

            // Act
            bool result = await Service.DeleteItemAsync(itemId);

            // Assert
            Assert.True(result);
            MockItemRepository.Verify(r => r.DeleteAsync(itemId, null), Times.Once);
        }

        [Fact]
        public async Task DeleteItemAsync_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();

            MockItemRepository
                .Setup(r => r.DeleteAsync(itemId, null))
                .ReturnsAsync(false);

            // Act
            bool result = await Service.DeleteItemAsync(itemId);

            // Assert
            Assert.False(result);
            MockItemRepository.Verify(r => r.DeleteAsync(itemId, null), Times.Once);
        }

        [Fact]
        public async Task MoveItemsAsync_WhenSingleItem_MovesItem()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();
            string newLocationId = "new-location";

            MockLocationRepository
                .Setup(r => r.ExistsAsync(newLocationId, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.ExistsAsync(itemId, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.MoveItemsAsync(new[] { itemId }, newLocationId, null))
                .ReturnsAsync(1);

            // Act
            int result = await Service.MoveItemsAsync(new[] { itemId }, newLocationId);

            // Assert
            Assert.Equal(1, result);
            MockLocationRepository.Verify(r => r.ExistsAsync(newLocationId, null), Times.Once);
            MockItemRepository.Verify(r => r.ExistsAsync(itemId, null), Times.Once);
            MockItemRepository.Verify(r => r.MoveItemsAsync(new[] { itemId }, newLocationId, null), Times.Once);
        }

        [Fact]
        public async Task MoveItemsAsync_WhenMultipleItems_MovesAllItems()
        {
            // Arrange
            Guid itemId1 = Guid.NewGuid();
            Guid itemId2 = Guid.NewGuid();
            Guid itemId3 = Guid.NewGuid();
            string newLocationId = "new-location";

            MockLocationRepository
                .Setup(r => r.ExistsAsync(newLocationId, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.ExistsAsync(itemId1, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.ExistsAsync(itemId2, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.ExistsAsync(itemId3, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.MoveItemsAsync(new[] { itemId1, itemId2, itemId3 }, newLocationId, null))
                .ReturnsAsync(3);

            // Act
            int result = await Service.MoveItemsAsync(new[] { itemId1, itemId2, itemId3 }, newLocationId);

            // Assert
            Assert.Equal(3, result);
            MockLocationRepository.Verify(r => r.ExistsAsync(newLocationId, null), Times.Once);
            MockItemRepository.Verify(r => r.ExistsAsync(itemId1, null), Times.Once);
            MockItemRepository.Verify(r => r.ExistsAsync(itemId2, null), Times.Once);
            MockItemRepository.Verify(r => r.ExistsAsync(itemId3, null), Times.Once);
            MockItemRepository.Verify(r => r.MoveItemsAsync(new[] { itemId1, itemId2, itemId3 }, newLocationId, null), Times.Once);
        }

        [Fact]
        public async Task MoveItemsAsync_WhenLocationDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();
            string newLocationId = "nonexistent-location";

            MockLocationRepository
                .Setup(r => r.ExistsAsync(newLocationId, null))
                .ReturnsAsync(false);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => Service.MoveItemsAsync(new[] { itemId }, newLocationId));

            Assert.Contains("does not exist", exception.Message);
            MockLocationRepository.Verify(r => r.ExistsAsync(newLocationId, null), Times.Once);
            MockItemRepository.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<IDbSession?>()), Times.Never);
            MockItemRepository.Verify(r => r.MoveItemsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task MoveItemsAsync_WhenItemDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            Guid itemId = Guid.NewGuid();
            string newLocationId = "test-location";

            MockLocationRepository
                .Setup(r => r.ExistsAsync(newLocationId, null))
                .ReturnsAsync(true);

            MockItemRepository
                .Setup(r => r.ExistsAsync(itemId, null))
                .ReturnsAsync(false);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.MoveItemsAsync(new[] { itemId }, newLocationId));

            Assert.Contains("does not exist", exception.Message);
            MockLocationRepository.Verify(r => r.ExistsAsync(newLocationId, null), Times.Once);
            MockItemRepository.Verify(r => r.ExistsAsync(itemId, null), Times.Once);
            MockItemRepository.Verify(r => r.MoveItemsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task MoveItemsAsync_WhenEmptyList_ThrowsArgumentException()
        {
            // Arrange
            string newLocationId = "test-location";

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.MoveItemsAsync(Array.Empty<Guid>(), newLocationId));

            Assert.Contains("At least one item ID must be provided", exception.Message);
            MockLocationRepository.Verify(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<IDbSession?>()), Times.Never);
            MockItemRepository.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<IDbSession?>()), Times.Never);
            MockItemRepository.Verify(r => r.MoveItemsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>(), It.IsAny<IDbSession?>()), Times.Never);
        }
    }
}
