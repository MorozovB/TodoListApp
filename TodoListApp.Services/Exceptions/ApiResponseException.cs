namespace TodoListApp.Services.Exceptions
{
    public class ApiResponseException : ApiException
    {
        public new int? StatusCode { get; }

        public ApiResponseException() : base()
        {
        }

        public ApiResponseException(string message) : base(message)
        {
        }

        public ApiResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ApiResponseException(string message, int statusCode)
            : base(message, statusCode)
        {
        }

        public ApiResponseException(string message, int statusCode, Exception innerException)
            : base(message, statusCode, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
