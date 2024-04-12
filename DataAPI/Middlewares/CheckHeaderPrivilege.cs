namespace DataAPI.Middlewares
{
    public class CheckHeaderPrivilege
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public CheckHeaderPrivilege(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Headers[_configuration["SuperHeader:Name"]].Equals(_configuration["SuperHeader:Value"]))
            {
                await _next(httpContext);
            }
            else
            {
                await httpContext.Response.WriteAsync("Unauthorized");
            }
        }
    }
}
