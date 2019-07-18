using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Linq;

namespace Crossplatform.EventsApp.Functions
{
    // TODO : Create binding for Azure Search
    public class LookupEventsFunction
    {
        private static string SearchServiceName = Environment.GetEnvironmentVariable("SearchServiceName");
        private static string ApiKey = Environment.GetEnvironmentVariable("SearchServiceApiKey");

        private static ISearchIndexClient _searchIndexClient;
        private const string IndexName = "eventstable-index";

        public LookupEventsFunction()
        {
            _searchIndexClient = CreateSearchIndexClient();
        }

        [FunctionName("LookupEventsFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "events/search={searchPhrase}")] HttpRequest req,
            ExecutionContext context,
            string searchPhrase,
            ILogger log)
        {
            log.LogInformation($"Start trigger {nameof(context.FunctionName)}");

            var parameters = new SearchParameters()
            {
                Select = new[] { "RowKey", "Title", "Address", "Place", "Country" }
            };
            var resultObject = await _searchIndexClient.Documents.SearchAsync(searchPhrase, parameters);
            if (resultObject.Results.Any())
                return new OkObjectResult(resultObject.Results);
            else
                return new BadRequestResult();
        }

        private SearchIndexClient CreateSearchIndexClient()
        {
            return new SearchIndexClient(SearchServiceName, IndexName, new SearchCredentials(ApiKey));
        }
    }
}
