using Microsoft.WindowsAzure.Storage.Table;

namespace Crossplatform.EventsApp.Domain.Event
{
    internal class EventModel : TableEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Place { get; set; }
        public string Country { get; set; }
    }
}
