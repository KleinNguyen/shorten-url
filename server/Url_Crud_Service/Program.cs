
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Url_Crud_Service.Data;
using Url_Crud_Service.Extension;
using Url_Crud_Service.Services;

namespace Url_Crud_Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<CrudDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("UrlCrudDbConnection"),
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,                     // số lần retry
            maxRetryDelay: TimeSpan.FromSeconds(10), // delay tối đa giữa các lần retry
            errorCodesToAdd: null              // các SQL error code muốn retry thêm
        )
            ));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
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
            builder.Services.AddAuthorization();


            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<ReceiveUrlService>(); 

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest");
                        h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest");
                    });

                    cfg.ReceiveEndpoint("url-send-event", e =>
                    {
                        e.ConfigureConsumer<ReceiveUrlService>(context);
                    });
                });
            });
            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:8080") // frontend dev server
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.ApplyMigrations();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowVueFrontend");
            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
