
using Authentication_Service.Data;
using Authentication_Service.Extension;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Authentication_Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AuthenticationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDbConnection"),
            sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5, 
                maxRetryDelay: TimeSpan.FromSeconds(10), 
                errorNumbersToAdd: null)));

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
            builder.Services.AddScoped<Services.EmailService>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:8080",
                        "https://shorten-url-client-7pz2.onrender.com"
                    ) 
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            }); 

            var app = builder.Build();
            app.ApplyMigrations();

            app.MapGet("/health", () => Results.Ok(new 
            { 
                status = "Authentication healthy"
            }));

           
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

            }

            // app.UseHttpsRedirection();
            app.UseCors("AllowVueFrontend");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
