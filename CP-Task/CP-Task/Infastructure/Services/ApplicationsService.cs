using CP_Task.Data;
using CP_Task.Infastructure.IServices;
using CP_Task.Model;
using Microsoft.Azure.Cosmos;

namespace CP_Task.Infastructure.Services
{
    /// <summary>
    /// Service for managing applications submitted by candidates.
    /// </summary>
    public class ApplicationsService : IApplicationsService
    {
        private readonly Container _container;
        private readonly CosmosClient _dbClient;
        private Database _database;
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationsService"/> class.
        /// </summary>
        /// <param name="dbClient">The Cosmos DB client.</param>
        public ApplicationsService(CosmosClient dbClient)
        {

            _dbClient = dbClient;
            //Todo: Find a better way to do the below.
            _database = _dbClient.CreateDatabaseIfNotExistsAsync("cp_task").Result;
            _container = _database.CreateContainerIfNotExistsAsync("Applications", "/id").Result;
            Console.WriteLine("Created Database: {0}\n", _database.Id);
            Console.WriteLine("Created Container: {0}\n", _container.Id);
        }

        /// <summary>
        /// Gets all applications from the Cosmos DB container.
        /// </summary>
        /// <returns>A list of applications.</returns>
        public async Task<IEnumerable<Application>> GetApplicationsAsync()
        {
            var query = _container.GetItemQueryIterator<Application>(new QueryDefinition("SELECT * FROM c"));
            List<Application> results = new List<Application>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }

        /// <summary>
        /// Gets a specific application by ID from the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the application to retrieve.</param>
        /// <returns>The requested application, or null if not found.</returns>
        public async Task<Application> GetApplicationAsync(string id)
        {
            try
            {
                ItemResponse<Application> response = await _container.ReadItemAsync<Application>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException e)
            {

                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                //Todo: handle later //
                throw;
            }
        }

        /// <summary>
        /// Adds a new application to the Cosmos DB container.
        /// </summary>
        /// <param name="application">The application to add.</param>
        public async Task<string> AddApplicationAsync(ApplicationDto applicationDto)
        {
            var application = new Application
            {
                id = Guid.NewGuid().ToString(),
                FirstName = applicationDto.FirstName,
                LastName = applicationDto.LastName,
                Email = applicationDto.Email,
                Answers = applicationDto.Answers.Select(a => new Answer
                {
                    id = Guid.NewGuid().ToString(),
                    QuestionId = a.QuestionId,
                    Response = a.Response
                }).ToList()
            };
            await _container.CreateItemAsync(application, new PartitionKey(application.id));
            return application.id;
        }

        /// <summary>
        /// Updates an existing application in the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the application to update.</param>
        /// <param name="application">The updated application.</param>
        public async Task<string> UpdateApplicationAsync(string id, ApplicationDto applicationDto)
        {
            var application = await GetApplicationAsync(id);
            if (application == null)
            {
                return null;
            }
            application.FirstName = applicationDto.FirstName;
            application.LastName = applicationDto.LastName;
            application.Email = applicationDto.Email;
            application.Answers = applicationDto.Answers.Select(a => new Answer
            {
                id = a.id ?? Guid.NewGuid().ToString(),
                QuestionId = a.QuestionId,
                Response = a.Response
            }).ToList();
            await _container.UpsertItemAsync(application, new PartitionKey(id));
            return id;
        }

        /// <summary>
        /// Deletes an application from the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the application to delete.</param>
        public async Task DeleteApplicationAsync(string id)
        {
            await _container.DeleteItemAsync<Application>(id, new PartitionKey(id));
        }
    }
}
