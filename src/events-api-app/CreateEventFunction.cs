using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;

namespace Crossplatform.Functions
{
    public static class CreateEventFunction
    {
        [FunctionName("CreateEventFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "events")] HttpRequest req,
            [Table("events")]CloudTable eventsTable,
            ILogger log)
        {
            var requestBody = await req.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult("Body of the request cannot be empty");
            }

            var command = JsonConvert.DeserializeObject<CreateEventCommand>(requestBody);
            var handler = new CreateEventCommandHandler(eventsTable);

            var resultObject = await handler.HandleAsync(command);
            if(resultObject.Success)
                return new CreatedResult(req.Path, resultObject.Result); // Return event guid
            else
                return new BadRequestObjectResult(resultObject.Message);
        }
    }

    internal class CreateEventCommandHandler
    {
        private readonly CloudTable _cloudTable;

        public CreateEventCommandHandler(CloudTable cloudTable)
        {
            _cloudTable = cloudTable;
        }

        internal async Task<ResultObject> HandleAsync(CreateEventCommand command)
        {
            if(!await _cloudTable.ExistsAsync())
                await _cloudTable.CreateAsync();

            // Validation logic goes here...

            var model = CreateEventModel(command);
            var operation = TableOperation.Insert(model);

            try
            {
                var tableResult = await _cloudTable.ExecuteAsync(operation);
                if(tableResult.HttpStatusCode == 204 && tableResult.Result is EventModel eventModel)
                    return new ResultObject { Success = true, Result = eventModel.RowKey, Message = string.Empty };
                else
                    return new ResultObject { Success = false, Result = null, Message = "Unable to create entity" };                
            }
            catch (StorageException)
            {
                throw;
            }
        }

        // TODO : This entity should be DTO.
        private EventModel CreateEventModel(CreateEventCommand command)
        {
            return new EventModel
            {
                PartitionKey = command.Client.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Title = command.Title,
                Description = command.Description,
                Address = command.Address,
                Place = command.Place,
                Country = command.Country
            };
        }
    }

    internal class EventModel : TableEntity
    {
        public string Title { get; internal set; }
        public string Description { get; internal set; }
        public string Address { get; internal set; }
        public string Place { get; internal set; }
        public string Country { get; internal set; }
    }

    internal class ResultObject
    {
        public bool Success { get; internal set; }
        public object Result { get; internal set; }
        public string Message { get; internal set; }
    }

    internal class CreateEventCommand
    {
        public Guid Client { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Place { get; set; }
        public string Country { get; set; }

        public IEnumerable<ArtistDto> Artists { get; set; }
    }

    public class ArtistDto
    {
        public string Fullname { get; set; }
        public string Description { get; set; }
    }
}
