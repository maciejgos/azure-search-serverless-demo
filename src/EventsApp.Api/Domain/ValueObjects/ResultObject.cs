namespace Crossplatform.EventsApp.Domain.ValueObjects
{
    public class ResultObject
    {
        public bool Success { get; set; }
        public object Result { get; set; }
        public string Message { get; set; }
    }
}
