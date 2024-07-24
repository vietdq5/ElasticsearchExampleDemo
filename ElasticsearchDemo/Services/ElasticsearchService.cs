using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticsearchDemo.Entities;
using ElasticsearchDemo.Settings;
using Microsoft.Extensions.Options;

namespace ElasticsearchDemo.Services;

public interface IElasticsearchService
{
    Task<IndexResponse> IndexProduct(Product product);

    Task<IReadOnlyCollection<Product>> SearchProducts(string searchTerm, int from = 0, int size = 10);
}

public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly ILogger<ElasticsearchService> _logger;

    public ElasticsearchService(ElasticsearchClient client, IOptions<ElasticsearchSettings> options, ILogger<ElasticsearchService> logger)
    {
        _client = client;
        _indexName = options.Value.DefaultIndex;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Product>> SearchProducts(string searchTerm, int from = 0, int size = 2)
    {
        try
        {
            var response = await _client.SearchAsync<Product>(s => s
            .Index(_indexName)
            .From(from)
            .Size(size)
            .Query(q => q
            .MultiMatch(mm => mm
            .Fields(Fields.FromExpressions<Product>([p => p.Name, p => p.Description]))
            .Query(searchTerm)
            .Type(TextQueryType.BestFields)
            .Fuzziness(new Fuzziness("AUTO")))));

            return response.Documents;

        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message}.\n{ex.StackTrace}");
            throw;
        }
    }

    public async Task<IndexResponse> IndexProduct(Product product)
    {
        try
        {
            var response = await _client.IndexAsync(product, i => i
                .Index(_indexName)
                .Id(product.Id)
            );

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to index product. Status code: {response.ApiCallDetails.HttpStatusCode}, Debug info: {response.DebugInformation}");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message}.\n{ex.StackTrace}");
            throw;
        }
    }
}