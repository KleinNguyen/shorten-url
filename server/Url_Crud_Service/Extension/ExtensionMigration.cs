using Microsoft.EntityFrameworkCore;
using Url_Crud_Service.Data;

namespace Url_Crud_Service.Extension
{
    public static class ExtensionMigration
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<CrudDbContext>();
            db.Database.Migrate();
        }
    }
}
