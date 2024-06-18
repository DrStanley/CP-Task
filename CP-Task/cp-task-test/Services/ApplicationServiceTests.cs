
using CP_Task.Data;
using CP_Task.Infastructure.IServices;
using CP_Task.Infastructure.Services;
using CP_Task.Model;
using Microsoft.Azure.Cosmos;
using Moq;
namespace cp_task_test.Services
{


    public class ApplicationServiceTests
    {
        private readonly IApplicationsService _applicationsService;
        private readonly Mock<Container> _containerMock;

        public ApplicationServiceTests()
        {
            _containerMock = new Mock<Container>();
            var dbClientMock = new Mock<CosmosClient>();
            var databaseMock = new Mock<Database>();
            var containerResponseMock = new Mock<ContainerResponse>();

            dbClientMock.Setup(c => c.CreateDatabaseIfNotExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Mock.Of<DatabaseResponse>(r => r.Database == databaseMock.Object));

            databaseMock.Setup(d => d.CreateContainerIfNotExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(containerResponseMock.Object);

            containerResponseMock.Setup(c => c.Container)
                                 .Returns(_containerMock.Object);

            _applicationsService = new ApplicationsService(dbClientMock.Object);
        }


        [Fact]
        public async Task GetApplicationsAsync_ShouldReturnAllApplications()
        {
            // Arrange
            var applications = new List<Application>
        {
            new Application { id = "1", FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new Application { id = "2", FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" }
        };
            var feedResponseMock = new Mock<FeedResponse<Application>>();
            feedResponseMock.Setup(r => r.GetEnumerator()).Returns(applications.GetEnumerator());
            var queryMock = new Mock<FeedIterator<Application>>();
            queryMock.Setup(q => q.HasMoreResults).Returns(true);
            queryMock.SetupSequence(q => q.ReadNextAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(feedResponseMock.Object)
                     .ReturnsAsync(Mock.Of<FeedResponse<Application>>(r => r.Count == 0));

            _containerMock.Setup(c => c.GetItemQueryIterator<Application>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                          .Returns(queryMock.Object);

            // Act
            var result = await _applicationsService.GetApplicationsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.id == "1");
            Assert.Contains(result, a => a.id == "2");
        }

        [Fact]
        public async Task GetApplicationAsync_ShouldReturnApplicationById()
        {
            // Arrange
            var applicationId = "1";
            var application = new Application { id = applicationId, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
            var itemResponseMock = new Mock<ItemResponse<Application>>();
            itemResponseMock.SetupGet(r => r.Resource).Returns(application);

            _containerMock.Setup(c => c.ReadItemAsync<Application>(applicationId, It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(itemResponseMock.Object);

            // Act
            var result = await _applicationsService.GetApplicationAsync(applicationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationId, result.id);
        }

        [Fact]
        public async Task AddApplicationAsync_ShouldAddApplication()
        {
            // Arrange
            var applicationDto = new ApplicationDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Answers = new List<AnswerDto>
            {
                new AnswerDto { QuestionId = "q1", Response = "Answer1" },
                new AnswerDto { QuestionId = "q2", Response = "Answer2" }
            }
            };

            _containerMock.Setup(c => c.CreateItemAsync(It.IsAny<Application>(), It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new Mock<ItemResponse<Application>>().Object);

            // Act
            var result = await _applicationsService.AddApplicationAsync(applicationDto);

            // Assert
            Assert.NotNull(result);
            _containerMock.Verify(c => c.CreateItemAsync(
                It.Is<Application>(a => a.FirstName == "John" && a.LastName == "Doe" && a.Email == "john@example.com" && a.Answers.Count == 2),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateApplicationAsync_ShouldUpdateExistingApplication()
        {
            // Arrange
            var applicationId = "1";
            var originalApplication = new Application { id = applicationId, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
            var updatedApplicationDto = new ApplicationDto
            {
                id = applicationId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Answers = new List<AnswerDto>
            {
                new AnswerDto { id = "a1", QuestionId = "q1", Response = "Updated Answer1" }
            }
            };

            var itemResponseMock = new Mock<ItemResponse<Application>>();
            itemResponseMock.SetupGet(r => r.Resource).Returns(originalApplication);

            _containerMock.Setup(c => c.ReadItemAsync<Application>(applicationId, It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(itemResponseMock.Object);

            // Act
            var result = await _applicationsService.UpdateApplicationAsync(applicationId, updatedApplicationDto);

            // Assert
            Assert.NotNull(result);
            _containerMock.Verify(c => c.UpsertItemAsync(
                It.Is<Application>(a => a.id == applicationId && a.FirstName == "Jane" && a.LastName == "Smith" && a.Email == "jane@example.com" && a.Answers.Count == 1),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteApplicationAsync_ShouldDeleteApplication()
        {
            // Arrange
            var applicationId = "1";

            _containerMock.Setup(c => c.DeleteItemAsync<Application>(applicationId, It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new Mock<ItemResponse<Application>>().Object);

            // Act
            await _applicationsService.DeleteApplicationAsync(applicationId);

            // Assert
            _containerMock.Verify(c => c.DeleteItemAsync<Application>(
                applicationId,
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
