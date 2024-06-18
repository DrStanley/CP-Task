using CP_Task.Data;
using CP_Task.Infastructure.IServices;
using CP_Task.Infastructure.Services;
using CP_Task.Model;
using Microsoft.Azure.Cosmos;
using Moq;

namespace cp_task_test.Services
{
    public class QuestionServiceTest
    {
        private readonly Mock<Container> _containerMock;
        private readonly IQuestionService _questionService;

        public QuestionServiceTest()
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

            _questionService = new QuestionService(dbClientMock.Object);
        }
        [Fact]
        public async Task GetQuestionsAsync_ShouldReturnAllQuestions()
        {
            // Arrange
            var questions = new List<Question>
        {
            new Question { id = "1", Type = "Paragraph", Text = "Question 1", Options = new List<string> { "Option1" } },
            new Question { id = "2", Type = "YesNo", Text = "Question 2", Options = new List<string>() }
        };
            var feedIteratorMock = new Mock<FeedIterator<Question>>();
            feedIteratorMock.SetupSequence(f => f.HasMoreResults)
                            .Returns(true);

            var feedResponseMock = new Mock<FeedResponse<Question>>();
            feedResponseMock.Setup(r => r.Resource)
                            .Returns(questions);

            feedIteratorMock.Setup(f => f.ReadNextAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(feedResponseMock.Object);

            _containerMock.Setup(c => c.GetItemQueryIterator<Question>(It.IsAny<QueryDefinition>(), null, null))
                          .Returns(feedIteratorMock.Object);

            // Act
            var result = await _questionService.GetQuestionsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Question 1", result.First().Text);
            Assert.Equal("Question 2", result.Last().Text);
        }

        [Fact]
        public async Task GetQuestionAsync_ShouldReturnQuestion_WhenQuestionExists()
        {
            // Arrange
            var question = new Question { id = "1", Type = "Paragraph", Text = "Question 1", Options = new List<string> { "Option1" } };
            _containerMock.Setup(c => c.ReadItemAsync<Question>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(Mock.Of<ItemResponse<Question>>(r => r.Resource == question));

            // Act
            var result = await _questionService.GetQuestionAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Question 1", result.Text);
        }

        [Fact]
        public async Task GetQuestionAsync_ShouldReturnNull_WhenQuestionDoesNotExist()
        {
            // Arrange
            _containerMock.Setup(c => c.ReadItemAsync<Question>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

            // Act
            var result = await _questionService.GetQuestionAsync("1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldAddQuestion()
        {
            // Arrange
            var questionDto = new QuestionDto { Type = "Paragraph", Text = "New Question", Options = new List<string> { "Option1" } };
            _containerMock.Setup(c => c.CreateItemAsync(It.IsAny<Question>(), It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(Mock.Of<ItemResponse<Question>>());

            // Act
            var result = await _questionService.AddQuestionAsync(questionDto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateQuestionAsync_ShouldUpdateExistingQuestion()
        {
            // Arrange
            var questionId = "1";
            var originalQuestion = new Question { id = questionId, Type = "Paragraph", Text = "Original Question", Options = new List<string> { "Option1" } };
            var updatedQuestionDto = new QuestionDto { id = questionId, Type = "YesNo", Text = "Updated Question", Options = new List<string> { "Yes", "No" } };

            var itemResponseMock = new Mock<ItemResponse<Question>>();
            itemResponseMock.SetupGet(r => r.Resource).Returns(originalQuestion);

            _containerMock.Setup(c => c.ReadItemAsync<Question>(questionId, It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(itemResponseMock.Object);

            // Act
            await _questionService.UpdateQuestionAsync(questionId, updatedQuestionDto);

            // Assert
            _containerMock.Verify(c => c.UpsertItemAsync(
                It.Is<Question>(q => q.id == questionId && q.Text == "Updated Question" && q.Type == "YesNo" && q.Options.SequenceEqual(new List<string> { "Yes", "No" })),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task DeleteQuestionAsync_ShouldDeleteQuestion()
        {
            // Arrange
            var question = new Question { id = "1", Type = "Paragraph", Text = "Question 1", Options = new List<string> { "Option1" } };
            _containerMock.Setup(c => c.DeleteItemAsync<Question>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(Mock.Of<ItemResponse<Question>>());

            // Act
            await _questionService.DeleteQuestionAsync("1");

            // Assert
            _containerMock.Verify(c => c.DeleteItemAsync<Question>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
