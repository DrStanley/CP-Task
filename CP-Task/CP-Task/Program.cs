
using CP_Task.Infastructure.IServices;
using CP_Task.Infastructure.Services;
using Microsoft.Azure.Cosmos;

namespace CP_Task
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Register the Cosmos DB client and services.
            var cosmosDbConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
            var cosmosDbSettings = builder.Configuration.GetSection("CosmosDbSettings");
            string EndpointUri = cosmosDbSettings["EndpointUri"]!;
            string PrimaryKey = cosmosDbSettings["PrimaryKey"]!;
            string ApplicationName = cosmosDbSettings["ApplicationName"]!;

            builder.Services.AddSingleton(s => new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = ApplicationName }));
            builder.Services.AddTransient<IQuestionService, QuestionService>();
            builder.Services.AddTransient<IApplicationsService, ApplicationsService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
