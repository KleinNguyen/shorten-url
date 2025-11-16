using Microsoft.EntityFrameworkCore;
using Url_Shorten_Service.Data;

namespace Url_Shorten_Service.Extension
{
    public static class MIgrationExtensions
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ShortenDbContext>();
            db.Database.Migrate();
        }
    }
}
