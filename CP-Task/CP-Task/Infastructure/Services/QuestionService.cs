using CP_Task.Data;
using CP_Task.Infastructure.IServices;
using CP_Task.Model;
using Microsoft.Azure.Cosmos;

namespace CP_Task.Infastructure.Services
{
    /// <summary>
    /// Service for managing questions in the application form.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly Container _container;
        private readonly CosmosClient _dbClient;
        private Database _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// </summary>
        /// <param name="dbClient">The Cosmos DB client.</param>
        public QuestionService(CosmosClient dbClient)
        {
            _dbClient = dbClient;
            //Todo: Find a better way to do the below.
            _database = _dbClient.CreateDatabaseIfNotExistsAsync("Questions").Result;
            _container = _database.CreateContainerIfNotExistsAsync("Items", "/id").Result;
            Console.WriteLine("Created Database: {0}\n", _database.Id);
            Console.WriteLine("Created Container: {0}\n", _container.Id);

        }

        /// <summary>
        /// Gets all questions from the Cosmos DB container.
        /// </summary>
        /// <returns>A list of questions.</returns>
        public async Task<IEnumerable<QuestionDto>> GetQuestionsAsync()
        {
            var query = _container.GetItemQueryIterator<Question>(new QueryDefinition("SELECT * FROM c"));
            List<QuestionDto> results = new List<QuestionDto>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.Select(q => (QuestionDto)q));
            }
            return results;
        }

        /// <summary>
        /// Gets a specific question by ID from the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the question to retrieve.</param>
        /// <returns>The requested question, or null if not found or CosmosException.</returns>
        public async Task<QuestionDto> GetQuestionAsync(string id)
        {
            try
            {
                ItemResponse<Question> response = await _container.ReadItemAsync<Question>(id, new PartitionKey(id));
                return (QuestionDto)response.Resource;
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                //Todo: handle later
                throw;
            }
        }

        /// <summary>
        /// Adds a new question to the Cosmos DB container.
        /// </summary>
        /// <param name="question">The question to add.</param>
        public async Task<string> AddQuestionAsync(QuestionDto questionDto)
        {
            var question = new Question
            {
                id = Guid.NewGuid().ToString(),
                Type = questionDto.Type,
                Text = questionDto.Text,
                Options = questionDto.Options
            };
            await _container.CreateItemAsync(question, new PartitionKey(question.id));
            return question.id;
        }

        /// <summary>
        /// Updates an existing question in the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the question to update.</param>
        /// <param name="question">The updated question.</param>
        public async Task UpdateQuestionAsync(string id, QuestionDto question)
        {
            await _container.UpsertItemAsync(question, new PartitionKey(id));
        }

        /// <summary>
        /// Deletes a question from the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the question to delete.</param>
        public async Task DeleteQuestionAsync(string id)
        {
            await _container.DeleteItemAsync<Question>(id, new PartitionKey(id));
        }

    }
}
