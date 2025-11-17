namespace TodoListApp.WebApi.Middleware
{
    /// <summary>
    /// Middleware for Bearer Token Authentication.
    /// </summary>
    public class BearerTokenAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _validToken;

        public BearerTokenAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            // Read the valid token from configuration
            _validToken = configuration["Authentication:BearerToken"];

            if (string.IsNullOrWhiteSpace(_validToken))
            {
                throw new InvalidOperationException(
                    "Authentication:BearerToken is not configured");
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get the Authorization header
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            // If no Authorization header, continue to the next middleware
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                await _next(context);
                return;
            }

            // Check if it starts with "Bearer "
            if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid authorization header format");
                return;
            }

            // Extract the token
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            // Check if the token is valid
            if (string.IsNullOrWhiteSpace(token) || token != _validToken)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid bearer token");
                return;
            }

            // Tocken is valid, continue to the next middleware
            await _next(context);
        }
    }
}