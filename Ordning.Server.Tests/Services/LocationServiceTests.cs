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
        public async Task SearchLocationsAsync_WhenSearchTermIsEmpty_ReturnsAllLocations()
        {
            // Arrange
            List<LocationDbModel> allLocations = new List<LocationDbModel>
            {
                new LocationDbModel { Id = "loc1", Name = "Location 1", Description = null, ParentLocationId = null, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
                new LocationDbModel { Id = "loc2", Name = "Location 2", Description = null, ParentLocationId = null, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
                new LocationDbModel { Id = "loc3", Name = "Location 3", Description = null, ParentLocationId = null, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(allLocations);

            // Act
            (IEnumerable<Location> results, int totalCount) = await Service.SearchLocationsAsync(string.Empty, 0, 20);

            // Assert
            Assert.Equal(3, totalCount);
            Assert.Equal(3, results.Count());
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenSearchTermIsWhitespace_ReturnsAllLocations()
        {
            // Arrange
            List<LocationDbModel> allLocations = new List<LocationDbModel>
            {
                new LocationDbModel { Id = "loc1", Name = "Location 1", Description = null, ParentLocationId = null, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(allLocations);

            // Act
            (IEnumerable<Location> results, int totalCount) = await Service.SearchLocationsAsync("   ", 0, 20);

            // Assert
            Assert.Equal(1, totalCount);
            Assert.Single(results);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenSearchTermIsNull_ReturnsAllLocations()
        {
            // Arrange
            List<LocationDbModel> allLocations = new List<LocationDbModel>
            {
                new LocationDbModel { Id = "loc1", Name = "Location 1", Description = null, ParentLocationId = null, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(allLocations);

            // Act
            (IEnumerable<Location> results, int totalCount) = await Service.SearchLocationsAsync(null!, 0, 20);

            // Assert
            Assert.Equal(1, totalCount);
            Assert.Single(results);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
            MockRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbSession?>()), Times.Never);
        }

        [Fact]
        public async Task SearchLocationsAsync_WhenSearchTermIsEmpty_AppliesPagination()
        {
            // Arrange
            List<LocationDbModel> allLocations = new List<LocationDbModel>();
            for (int i = 0; i < 10; i++)
            {
                allLocations.Add(new LocationDbModel { Id = $"loc{i}", Name = $"Location {i}", Description = null, ParentLocationId = null, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            }

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(allLocations);

            // Act
            (IEnumerable<Location> results, int totalCount) = await Service.SearchLocationsAsync(string.Empty, 2, 3);

            // Assert
            Assert.Equal(10, totalCount);
            Assert.Equal(3, results.Count());
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
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

        [Fact]
        public async Task GetLocationTreeAsync_WhenNoLocations_ReturnsEmptyCollection()
        {
            // Arrange
            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(Array.Empty<LocationDbModel>());

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            Assert.Empty(result);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetLocationTreeAsync_WhenSingleRootLocation_ReturnsSingleNode()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            LocationDbModel location = new LocationDbModel
            {
                Id = "root-1",
                Name = "Root Location",
                Description = "Root Description",
                ParentLocationId = null,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(new[] { location });

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            List<LocationTreeNode> nodes = result.ToList();
            Assert.Single(nodes);
            Assert.Equal("root-1", nodes[0].Location.Id);
            Assert.Equal("Root Location", nodes[0].Location.Name);
            Assert.Empty(nodes[0].Children);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetLocationTreeAsync_WhenMultipleRootLocations_ReturnsMultipleRootNodes()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locations = new[]
            {
                new LocationDbModel { Id = "root-2", Name = "Root 2", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "root-1", Name = "Root 1", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "root-3", Name = "Root 3", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(locations);

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            List<LocationTreeNode> nodes = result.ToList();
            Assert.Equal(3, nodes.Count);
            Assert.Equal("Root 1", nodes[0].Location.Name);
            Assert.Equal("Root 2", nodes[1].Location.Name);
            Assert.Equal("Root 3", nodes[2].Location.Name);
            Assert.All(nodes, n => Assert.Empty(n.Children));
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetLocationTreeAsync_WhenParentAndChild_ReturnsHierarchicalStructure()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locations = new[]
            {
                new LocationDbModel { Id = "parent-1", Name = "Parent", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-1", Name = "Child", Description = null, ParentLocationId = "parent-1", CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(locations);

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            List<LocationTreeNode> nodes = result.ToList();
            Assert.Single(nodes);
            Assert.Equal("parent-1", nodes[0].Location.Id);
            Assert.Single(nodes[0].Children);
            Assert.Equal("child-1", nodes[0].Children[0].Location.Id);
            Assert.Equal("parent-1", nodes[0].Children[0].Location.ParentLocationId);
            Assert.Empty(nodes[0].Children[0].Children);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetLocationTreeAsync_WhenMultipleChildren_ReturnsAllChildren()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locations = new[]
            {
                new LocationDbModel { Id = "parent-1", Name = "Parent", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-2", Name = "Child 2", Description = null, ParentLocationId = "parent-1", CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-1", Name = "Child 1", Description = null, ParentLocationId = "parent-1", CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-3", Name = "Child 3", Description = null, ParentLocationId = "parent-1", CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(locations);

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            List<LocationTreeNode> nodes = result.ToList();
            Assert.Single(nodes);
            Assert.Equal(3, nodes[0].Children.Count);
            Assert.Equal("Child 1", nodes[0].Children[0].Location.Name);
            Assert.Equal("Child 2", nodes[0].Children[1].Location.Name);
            Assert.Equal("Child 3", nodes[0].Children[2].Location.Name);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetLocationTreeAsync_WhenMultiLevelHierarchy_ReturnsNestedStructure()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locations = new[]
            {
                new LocationDbModel { Id = "root", Name = "Root", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "level1", Name = "Level 1", Description = null, ParentLocationId = "root", CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "level2", Name = "Level 2", Description = null, ParentLocationId = "level1", CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "level3", Name = "Level 3", Description = null, ParentLocationId = "level2", CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(locations);

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            List<LocationTreeNode> nodes = result.ToList();
            Assert.Single(nodes);
            Assert.Equal("root", nodes[0].Location.Id);
            Assert.Single(nodes[0].Children);
            Assert.Equal("level1", nodes[0].Children[0].Location.Id);
            Assert.Single(nodes[0].Children[0].Children);
            Assert.Equal("level2", nodes[0].Children[0].Children[0].Location.Id);
            Assert.Single(nodes[0].Children[0].Children[0].Children);
            Assert.Equal("level3", nodes[0].Children[0].Children[0].Children[0].Location.Id);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetLocationTreeAsync_WhenOrphanedLocation_ExcludesFromTree()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locations = new[]
            {
                new LocationDbModel { Id = "root", Name = "Root", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "orphan", Name = "Orphan", Description = null, ParentLocationId = "nonexistent", CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(locations);

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            List<LocationTreeNode> nodes = result.ToList();
            Assert.Single(nodes);
            Assert.Equal("root", nodes[0].Location.Id);
            Assert.Empty(nodes[0].Children);
            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetLocationTreeAsync_WhenComplexHierarchy_ReturnsCorrectStructure()
        {
            // Arrange
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            IEnumerable<LocationDbModel> locations = new[]
            {
                new LocationDbModel { Id = "root-1", Name = "Root 1", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "root-2", Name = "Root 2", Description = null, ParentLocationId = null, CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-1-1", Name = "Child 1-1", Description = null, ParentLocationId = "root-1", CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-1-2", Name = "Child 1-2", Description = null, ParentLocationId = "root-1", CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "child-2-1", Name = "Child 2-1", Description = null, ParentLocationId = "root-2", CreatedAt = timestamp, UpdatedAt = timestamp },
                new LocationDbModel { Id = "grandchild-1-1-1", Name = "Grandchild 1-1-1", Description = null, ParentLocationId = "child-1-1", CreatedAt = timestamp, UpdatedAt = timestamp }
            };

            MockRepository
                .Setup(r => r.GetAllAsync(null))
                .ReturnsAsync(locations);

            // Act
            IEnumerable<LocationTreeNode> result = await Service.GetLocationTreeAsync();

            // Assert
            List<LocationTreeNode> nodes = result.ToList();
            Assert.Equal(2, nodes.Count);
            
            LocationTreeNode root1 = nodes.First(n => n.Location.Id == "root-1");
            Assert.Equal(2, root1.Children.Count);
            Assert.Equal("Child 1-1", root1.Children[0].Location.Name);
            Assert.Equal("Child 1-2", root1.Children[1].Location.Name);
            Assert.Single(root1.Children[0].Children);
            Assert.Equal("Grandchild 1-1-1", root1.Children[0].Children[0].Location.Name);

            LocationTreeNode root2 = nodes.First(n => n.Location.Id == "root-2");
            Assert.Single(root2.Children);
            Assert.Equal("Child 2-1", root2.Children[0].Location.Name);
            Assert.Empty(root2.Children[0].Children);

            MockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
        }
    }
}
