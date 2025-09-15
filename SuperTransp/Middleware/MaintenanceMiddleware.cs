namespace SuperTransp.Middleware
{
	public class MaintenanceMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IConfiguration _configuration;

		public MaintenanceMiddleware(RequestDelegate next, IConfiguration configuration)
		{
			_next = next;
			_configuration = configuration;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var isMaintenanceEnabled = _configuration.GetValue<bool>("AppSettings:EnableMaintenanceMode");

			var redirectPath = $"{context.Request.PathBase}/Home/Maintenance";
			var path = context.Request.Path.Value?.ToLower();

			if (isMaintenanceEnabled && !path.ToLower().Contains("maintenance"))
			{
				context.Response.Redirect(redirectPath);
				return;
			}

			await _next(context);
		}
	}
}
