
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
                options.UseSqlServer(builder.Configuration.GetConnectionString("UrlCrudDbConnection"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,                     // số lần retry
                maxRetryDelay: TimeSpan.FromSeconds(10), // delay tối đa giữa các lần retry
                errorNumbersToAdd: null)));

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


            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<ReceiveUrlService>(); 

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("fuji.lmq.cloudamqp.com", "ioitvvgk", h =>
                    {
                        h.Username("ioitvvgk");
                        h.Password("VzJoVb6iTESpEXfATJ5oNh9PcjVw1Vmu");
                    });

                    cfg.ReceiveEndpoint("url-send-event", e =>
                    {
                        e.ConfigureConsumer<ReceiveUrlService>(context);
                    });
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:8080",
                        "https://shorten-url-client-7pz2.onrender.com",
                        "https://short-url-api-utgu.onrender.com") 
                            .AllowAnyMethod()
                            .AllowAnyHeader().
                            AllowCredentials();
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

            // app.UseHttpsRedirection();
            app.UseCors("AllowVueFrontend");
            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
