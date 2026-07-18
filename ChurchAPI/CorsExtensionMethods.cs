namespace ChurchAPI
{
    public static class CorsExtensionMethods
    {
        public static void AddCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? [];
            if (allowedOrigins.Length == 0) return;

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
        }

    }
}
