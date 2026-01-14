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
            LocationDbModel locationDbModel = new LocationDbModel
            {
                Id = id,
                Name = "Test Location",
                Description = "Test Description",
                ParentLocationId = null
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
            IEnumerable<LocationDbModel> locationDbModels = new[]
            {
                new LocationDbModel { Id = "location-1", Name = "Location 1", Description = null, ParentLocationId = null },
                new LocationDbModel { Id = "location-2", Name = "Location 2", Description = null, ParentLocationId = null }
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
            IEnumerable<LocationDbModel> children = new[]
            {
                new LocationDbModel { Id = "child-1", Name = "Child 1", Description = null, ParentLocationId = parentId },
                new LocationDbModel { Id = "child-2", Name = "Child 2", Description = null, ParentLocationId = parentId }
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

            LocationDbModel createdLocation = new LocationDbModel
            {
                Id = id,
                Name = name,
                Description = description,
                ParentLocationId = null
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

            LocationDbModel createdLocation = new LocationDbModel
            {
                Id = id,
                Name = name,
                Description = null,
                ParentLocationId = parentId
            };

            MockRepository
                .Setup(r => r.ExistsAsync(id, null))
                .ReturnsAsync(false);

            MockRepository
                .Setup(r => r.ExistsAsync(parentId, null))
                .ReturnsAsync(true);

            MockRepository
                .Setup(r => r.GetByIdAsync(parentId, null))
                .ReturnsAsync(new LocationDbModel { Id = parentId, Name = "Parent", Description = null, ParentLocationId = null });

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

            LocationDbModel existingLocation = new LocationDbModel
            {
                Id = id,
                Name = "Original Name",
                Description = "Original Description",
                ParentLocationId = null
            };

            LocationDbModel updatedLocation = new LocationDbModel
            {
                Id = id,
                Name = newName,
                Description = newDescription,
                ParentLocationId = null
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
    }
}
