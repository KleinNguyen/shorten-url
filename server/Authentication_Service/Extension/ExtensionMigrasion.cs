using Authentication_Service.Data;
using Microsoft.EntityFrameworkCore;

namespace Authentication_Service.Extension
{
    public  static class ExtensionMigrasion
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
            db.Database.Migrate();
        }
    }
}
