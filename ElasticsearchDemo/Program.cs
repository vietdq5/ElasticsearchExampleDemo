using Elastic.Clients.Elasticsearch;
using ElasticsearchDemo.Entities;
using ElasticsearchDemo.Services;
using ElasticsearchDemo.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ElasticsearchSettings>(builder.Configuration.GetSection("ElasticsearchSettings"));

builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var elasticSettings = sp.GetRequiredService<IOptions<ElasticsearchSettings>>();
    var settings = new ElasticsearchClientSettings(new Uri(elasticSettings.Value.Uri))
        .DefaultIndex(elasticSettings.Value.DefaultIndex)
        .DefaultMappingFor<Product>(m => m
            .IndexName(elasticSettings.Value.DefaultIndex)
        );
    return new ElasticsearchClient(settings);
});
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

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
