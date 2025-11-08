namespace ChatService.Settings.JWT
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtUtils _jwtUtils;

        public TokenValidationMiddleware(RequestDelegate next, JwtUtils jwtUtils)
        {
            _next = next;
            _jwtUtils = jwtUtils;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ IMPORTANT: Allow CORS preflight requests to pass immediately
            if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                // Let CORS middleware handle this cleanly
                await _next(context);
                return;
            }

            // Extract the token from the Authorization header.
            var token = context.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

            // If a token is present, validate it
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var claimsPrincipal = _jwtUtils.ValidateToken(token);

                    // If validation is successful, log it
                    Console.WriteLine("Token is valid.");

                    // Optionally, you can add claimsPrincipal to the context if needed
                    context.User = claimsPrincipal;
                }
                catch (Exception ex)
                {
                    // If token validation fails, log the error
                    Console.WriteLine("Token is invalid: " + ex.Message);
                }
            }

            // Continue processing the request (move to the next middleware)
            await _next(context);
        }
    }
}
