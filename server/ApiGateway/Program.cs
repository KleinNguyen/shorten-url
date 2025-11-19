using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiGateway
{
    public class Program
    {
        public static async Task Main(string[] args)  
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            ProcessOcelotConfiguration();

            builder.Configuration.AddJsonFile(
                "ocelot.runtime.json",
                optional: false,
                reloadOnChange: true
            );

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
                        )
                    };
                });

            builder.Services.AddOcelot(builder.Configuration)
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });
            
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueFrontend", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:8080",
                        "https://shorten-url-client-7pz2.onrender.com", 
                        "https://short-url-api-utgu.onrender.com"
                    ) 
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); 
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowVueFrontend");
            app.UseAuthentication(); 
            app.UseAuthorization();
            app.MapControllers();

            await app.UseOcelot(); 

            app.Run();
        }

        private static void ProcessOcelotConfiguration()
        {
            var ocelotJsonPath = "ocelot.json";
            var runtimeJsonPath = "ocelot.runtime.json";

            if (!File.Exists(ocelotJsonPath))
            {
                throw new FileNotFoundException($"Ocelot configuration file not found: {ocelotJsonPath}");
            }

            var ocelotJson = File.ReadAllText(ocelotJsonPath);

            ocelotJson = ReplaceEnvironmentVariables(ocelotJson);

            File.WriteAllText(runtimeJsonPath, ocelotJson);

            Console.WriteLine("Ocelot configuration processed successfully!");
            Console.WriteLine($"BASE_URL: {Environment.GetEnvironmentVariable("BASE_URL")}");
            Console.WriteLine($"DOWNSTREAM_SCHEME: {Environment.GetEnvironmentVariable("DOWNSTREAM_SCHEME")}");
            Console.WriteLine($"AUTH_SERVICE_HOST: {Environment.GetEnvironmentVariable("AUTH_SERVICE_HOST")}");
            Console.WriteLine($"SHORTEN_SERVICE_HOST: {Environment.GetEnvironmentVariable("SHORTEN_SERVICE_HOST")}");
            Console.WriteLine($"CRUD_SERVICE_HOST: {Environment.GetEnvironmentVariable("CRUD_SERVICE_HOST")}");
        }

        private static string ReplaceEnvironmentVariables(string json)
        {
            var pattern = @"\$\{([^}]+)\}";
            
            return Regex.Replace(json, pattern, match =>
            {
                var variableName = match.Groups[1].Value;
                var value = Environment.GetEnvironmentVariable(variableName);
                
                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException(
                        $"Environment variable '{variableName}' is not set! " +
                        $"Please check your .env file or environment configuration."
                    );
                }
                
                Console.WriteLine($"Replacing ${{{variableName}}} with: {value}");

                if (variableName.EndsWith("_PORT"))
                {
                    if (int.TryParse(value, out int portNumber))
                    {
                        return portNumber.ToString(); 
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Port value '{value}' for '{variableName}' is not a valid number!"
                        );
                    }
                }
                return value;
            });
        }
    }
}
