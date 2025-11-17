namespace TodoListApp.Services.Exceptions
{
    public class ApiConnectionException : ApiException
    {
        public ApiConnectionException() : base()
        {
        }

        public ApiConnectionException(string message) : base(message)
        {
        }

        public ApiConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
