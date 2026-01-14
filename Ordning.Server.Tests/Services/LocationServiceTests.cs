using EasyReasy.Database;
using Moq;
using Ordning.Server.Locations.Models;
using Ordning.Server.Locations.Repositories;
using Ordning.Server.Locations.Services;

namespace Ordning.Server.Tests.Services
{
    /// <summary>
    /// Unit tests for LocationService.
    /// </summary>
    public class LocationServiceTests
    {
        private Mock<ILocationRepository> MockRepository { get; set; } = null!;
        private LocationService Service { get; set; } = null!;

        public LocationServiceTests()
        {
            MockRepository = new Mock<ILocationRepository>();
            Service = new LocationService(MockRepository.Object);
        }

        [Fact]
        public async Task GetLocationByIdAsync_WhenLocationExists_ReturnsLocation()
        {
            // Arrange
            string id = "test-location";
            DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddMinutes(-5);
            DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
            LocationDbModel locationDbModel = new LocationDbModel
            {
                Id = id,
                Name = "Test Location",
                Description = "Test Description",
                ParentLocationId = null,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            MockRepository
                .Setup(r => r.GetByIdAsync(id, null))
                .ReturnsAsync(locationDbModel);

            // Act
            Location? result = await Service.GetLocationByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(locationDbModel.Name, result.Name);
            Assert.Equal(locationDbModel.Description, result.Description);
            Assert.Equal(createdAt, result.CreatedAt);
            Assert.Equal(updatedAt, result.UpdatedAt);
            MockRepository.Verify(r => r.GetByIdAsync(id, null), Times.Once);
        }

        [Fact]
        public async Task GetLocationByIdAsync_WhenLocationDoesNotExist_ReturnsNull()
        {
            // Arrange
            string id = "nonexistent-location";

            MockRepository
                .Setup(r => r.GetByIdAsync(id, null))
                .ReturnsAsync((LocationDbModel?)null);

            // Act
            Location? result = await Service.GetLocationByIdAsync(id);

            // Assert
            Assert.Null(result);
            MockRepository.Verify(r => r.GetByIdAsync(id, null), Times.Once);
        }

        [Fact]
        public async Task GetAllLocationsAsync_WhenCalled_ReturnsAllLocations()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locationDbModels = new[]
            {
                new LocationDbModel { Id = "location-1", Name = "Location 1", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "location-2", Name = "Location 2", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(locationDbModels);

            // Act
            IEnumerable<Location> result = await Service.GetAllLocationsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetChildrenAsync_WhenCalled_ReturnsChildren()
        {
            // Arrange
            string parentId = "parent-location";
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> children = new[]
            {
                new LocationDbModel { Id = "child-1", Name = "Child 1", Description = null, ParentLocationId = parentId, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-2", Name = "Child 2", Description = null, ParentLocationId = parentId, CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetChildrenAsync(parentId, null))
                .ReturnsAsync(children);

            // Act
            IEnumerable<Location> result = await Service.GetChildrenAsync(parentId);

            // Assert
            Assert.Equal(2, result.Count());
            MockRepository.Verify(r => r.GetChildrenAsync(parentId, null), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_WhenValidData_CreatesLocation()
        {
            // Arrange
            string id = "new-location";
            string name = "New Location";
            string description = "New Description";

            DateTimeOffset createdAt = DateTimeOffset.UtcNow;
            DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
            LocationDbModel createdLocation = new LocationDbModel
            {
                Id = id,
                Name = name,
                Description = description,
                ParentLocationId = null,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            MockRepository
                .Setup(r => r.ExistsAsync(id, null))
                .ReturnsAsync(false);

            MockRepository
                .Setup(r => r.CreateAsync(id, name, description, null, null))
                .ReturnsAsync(createdLocation);

            // Act
            Location result = await Service.CreateLocationAsync(id, name, description);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(createdAt, result.CreatedAt);
            Assert.Equal(updatedAt, result.UpdatedAt);
            MockRepository.Verify(r => r.ExistsAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.CreateAsync(id, name, description, null, null), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_WhenLocationIdExists_ThrowsArgumentException()
        {
            // Arrange
            string id = "existing-location";
            string name = "Location Name";

            MockRepository
                .Setup(r => r.ExistsAsync(id, null))
                .ReturnsAsync(true);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.CreateLocationAsync(id, name));

            Assert.Contains("already exists", exception.Message);
            MockRepository.Verify(r => r.ExistsAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task CreateLocationAsync_WhenParentDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            string id = "new-location";
            string name = "Location Name";
            string parentId = "nonexistent-parent";

            MockRepository
                .Setup(r => r.ExistsAsync(id, null))
                .ReturnsAsync(false);

            MockRepository
                .Setup(r => r.ExistsAsync(parentId, null))
                .ReturnsAsync(false);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => Service.CreateLocationAsync(id, name, null, parentId));

            Assert.Contains("does not exist", exception.Message);
            MockRepository.Verify(r => r.ExistsAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.ExistsAsync(parentId, null), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_WhenParentExists_CreatesLocationWithParent()
        {
            // Arrange
            string id = "child-location";
            string name = "Child Location";
            string parentId = "parent-location";

            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            LocationDbModel createdLocation = new LocationDbModel
            {
                Id = id,
                Name = name,
                Description = null,
                ParentLocationId = parentId,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            MockRepository
                .Setup(r => r.ExistsAsync(id, null))
                .ReturnsAsync(false);

            MockRepository
                .Setup(r => r.ExistsAsync(parentId, null))
                .ReturnsAsync(true);

            DateTimeOffset parentTimestamp = DateTimeOffset.UtcNow;
            MockRepository
                .Setup(r => r.GetByIdAsync(parentId, null))
                .ReturnsAsync(new LocationDbModel { Id = parentId, Name = "Parent", Description = null, ParentLocationId = null, CreatedAt = parentTimestamp, UpdatedAt = parentTimestamp });

            MockRepository
                .Setup(r => r.CreateAsync(id, name, null, parentId, null))
                .ReturnsAsync(createdLocation);

            // Act
            Location result = await Service.CreateLocationAsync(id, name, null, parentId);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal(parentId, result.ParentLocationId);
            MockRepository.Verify(r => r.ExistsAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.ExistsAsync(parentId, null), Times.Once);
            MockRepository.Verify(r => r.CreateAsync(id, name, null, parentId, null), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_WhenCircularReferenceWouldBeCreated_ThrowsArgumentException()
        {
            // Arrange
            string id = "location-a";
            string name = "Location A";
            string parentId = "location-b";

            LocationDbModel parent = new LocationDbModel
            {
                Id = parentId,
                Name = "Location B",
                Description = null,
                ParentLocationId = id
            };

            MockRepository
                .Setup(r => r.ExistsAsync(id, null))
                .ReturnsAsync(false);

            MockRepository
                .Setup(r => r.ExistsAsync(parentId, null))
                .ReturnsAsync(true);

            MockRepository
                .Setup(r => r.GetByIdAsync(parentId, null))
                .ReturnsAsync(parent);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.CreateLocationAsync(id, name, null, parentId));

            Assert.Contains("circular reference", exception.Message);
            MockRepository.Verify(r => r.ExistsAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.ExistsAsync(parentId, null), Times.Once);
            MockRepository.Verify(r => r.GetByIdAsync(parentId, null), Times.Once);
            MockRepository.Verify(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task UpdateLocationAsync_WhenValidData_UpdatesLocation()
        {
            // Arrange
            string id = "update-location";
            string newName = "Updated Name";
            string newDescription = "Updated Description";

            DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddMinutes(-10);
            DateTimeOffset originalUpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
            DateTimeOffset newUpdatedAt = DateTimeOffset.UtcNow;

            LocationDbModel existingLocation = new LocationDbModel
            {
                Id = id,
                Name = "Original Name",
                Description = "Original Description",
                ParentLocationId = null,
                CreatedAt = createdAt,
                UpdatedAt = originalUpdatedAt
            };

            LocationDbModel updatedLocation = new LocationDbModel
            {
                Id = id,
                Name = newName,
                Description = newDescription,
                ParentLocationId = null,
                CreatedAt = createdAt,
                UpdatedAt = newUpdatedAt
            };

            MockRepository
                .Setup(r => r.GetByIdAsync(id, null))
                .ReturnsAsync(existingLocation);

            MockRepository
                .Setup(r => r.UpdateAsync(id, newName, newDescription, null, null))
                .ReturnsAsync(true);

            MockRepository
                .Setup(r => r.GetByIdAsync(id, null))
                .ReturnsAsync(updatedLocation);

            // Act
            Location result = await Service.UpdateLocationAsync(id, newName, newDescription);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal(newName, result.Name);
            Assert.Equal(newDescription, result.Description);
            Assert.Equal(createdAt, result.CreatedAt);
            Assert.Equal(newUpdatedAt, result.UpdatedAt);
            MockRepository.Verify(r => r.GetByIdAsync(id, null), Times.Exactly(2));
            MockRepository.Verify(r => r.UpdateAsync(id, newName, newDescription, null, null), Times.Once);
        }

        [Fact]
        public async Task UpdateLocationAsync_WhenLocationDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            string id = "nonexistent-location";
            string name = "Location Name";

            MockRepository
                .Setup(r => r.GetByIdAsync(id, null))
                .ReturnsAsync((LocationDbModel?)null);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.UpdateLocationAsync(id, name));

            Assert.Contains("does not exist", exception.Message);
            MockRepository.Verify(r => r.GetByIdAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task UpdateLocationAsync_WhenSettingSelfAsParent_ThrowsArgumentException()
        {
            // Arrange
            string id = "location-a";
            string name = "Location A";

            LocationDbModel existingLocation = new LocationDbModel
            {
                Id = id,
                Name = name,
                Description = null,
                ParentLocationId = null
            };

            MockRepository
                .Setup(r => r.GetByIdAsync(id, null))
                .ReturnsAsync(existingLocation);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.UpdateLocationAsync(id, name, null, id));

            Assert.Contains("cannot be its own parent", exception.Message);
            MockRepository.Verify(r => r.GetByIdAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task UpdateLocationAsync_WhenCircularReferenceWouldBeCreated_ThrowsArgumentException()
        {
            // Arrange
            string id = "location-a";
            string name = "Location A";
            string parentId = "location-b";

            LocationDbModel existingLocation = new LocationDbModel
            {
                Id = id,
                Name = name,
                Description = null,
                ParentLocationId = null
            };

            LocationDbModel parent = new LocationDbModel
            {
                Id = parentId,
                Name = "Location B",
                Description = null,
                ParentLocationId = id
            };

            MockRepository
                .Setup(r => r.GetByIdAsync(id, null))
                .ReturnsAsync(existingLocation);

            MockRepository
                .Setup(r => r.ExistsAsync(parentId, null))
                .ReturnsAsync(true);

            MockRepository
                .Setup(r => r.GetByIdAsync(parentId, null))
                .ReturnsAsync(parent);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.UpdateLocationAsync(id, name, null, parentId));

            Assert.Contains("circular reference", exception.Message);
            MockRepository.Verify(r => r.GetByIdAsync(id, null), Times.Once);
            MockRepository.Verify(r => r.ExistsAsync(parentId, null), Times.Once);
            MockRepository.Verify(r => r.GetByIdAsync(parentId, null), Times.Once);
            MockRepository.Verify(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task DeleteLocationAsync_WhenLocationExists_ReturnsTrue()
        {
            // Arrange
            string id = "delete-location";

            MockRepository
                .Setup(r => r.DeleteAsync(id, null))
                .ReturnsAsync(true);

            // Act
            bool result = await Service.DeleteLocationAsync(id);

            // Assert
            Assert.True(result);
            MockRepository.Verify(r => r.DeleteAsync(id, null), Times.Once);
        }

        [Fact]
        public async Task DeleteLocationAsync_WhenLocationDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string id = "nonexistent-location";

            MockRepository
                .Setup(r => r.DeleteAsync(id, null))
                .ReturnsAsync(false);

            // Act
            bool result = await Service.DeleteLocationAsync(id);

            // Assert
            Assert.False(result);
            MockRepository.Verify(r => r.DeleteAsync(id, null), Times.Once);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenValidSearchTerm_CallsRepository()
        {
            // Arrange
            string searchTerm = "garage";
            int offset = 0;
            int limit = 20;

            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locationDbModels = new[]
            {
                new LocationDbModel { Id = "garage-1", Name = "Garage", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "garage-2", Name = "Garage Attic", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.SearchAsync(searchTerm, offset, limit, null))
                .ReturnsAsync((locationDbModels, 2));

            // Act
            (IEnumerable<Location> results, int totalCount) = await Service.SearchLocationsAsync(searchTerm, offset, limit);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(2, totalCount);
            MockRepository.Verify(r => r.SearchAsync(searchTerm, offset, limit, null), Times.Once);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenSearchTermIsEmpty_ThrowsArgumentException()
        {
            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.SearchLocationsAsync(string.Empty, 0, 20));

            Assert.Contains("cannot be null, empty, or whitespace-only", exception.Message);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenSearchTermIsWhitespace_ThrowsArgumentException()
        {
            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.SearchLocationsAsync("   ", 0, 20));

            Assert.Contains("cannot be null, empty, or whitespace-only", exception.Message);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenSearchTermIsNull_ThrowsArgumentException()
        {
            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.SearchLocationsAsync(null!, 0, 20));

            Assert.Contains("cannot be null, empty, or whitespace-only", exception.Message);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenOffsetIsNegative_ThrowsArgumentException()
        {
            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.SearchLocationsAsync("test", -1, 20));

            Assert.Contains("must be greater than or equal to zero", exception.Message);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenLimitIsZero_ThrowsArgumentException()
        {
            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.SearchLocationsAsync("test", 0, 0));

            Assert.Contains("must be greater than zero", exception.Message);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenLimitExceedsMax_ThrowsArgumentException()
        {
            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => Service.SearchLocationsAsync("test", 0, 101));

            Assert.Contains("cannot exceed 100", exception.Message);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenResultsExist_ReturnsLocationsAndTotalCount()
        {
            // Arrange
            string locationId1 = "garage-1";
            string locationId2 = "garage-2";

            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locationDbModels = new[]
            {
                new LocationDbModel { Id = locationId1, Name = "Garage", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = locationId2, Name = "Garage Attic", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.SearchAsync("garage", 0, 20, null))
                .ReturnsAsync((locationDbModels, 2));

            // Act
            (IEnumerable<Location> results, int totalCount) = await Service.SearchLocationsAsync("garage", 0, 20);

            // Assert
            List<Location> resultsList = results.ToList();
            Assert.Equal(2, resultsList.Count);
            Assert.Equal(2, totalCount);
            Assert.Contains(resultsList, l => l.Id == locationId1);
            Assert.Contains(resultsList, l => l.Id == locationId2);
        }

        [Fact]
        public async Task SearchLocationsAsync_ConvertsDbModelsToDomainModels()
        {
            // Arrange
            string id = "test-location";
            string name = "Test Location";
            string description = "Test Description";
            string parentId = "parent-location";

            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            LocationDbModel locationDbModel = new LocationDbModel
            {
                Id = id,
                Name = name,
                Description = description,
                ParentLocationId = parentId,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            MockRepository
                .Setup(r => r.SearchAsync("test", 0, 20, null))
                .ReturnsAsync((new[] { locationDbModel }, 1));

            // Act
            (IEnumerable<Location> results, int totalCount) = await Service.SearchLocationsAsync("test", 0, 20);

            // Assert
            Location result = results.Single();
            Assert.Equal(id, result.Id);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(parentId, result.ParentLocationId);
            Assert.Equal(timestamp, result.CreatedAt);
            Assert.Equal(timestamp, result.UpdatedAt);
        }
    }
}
