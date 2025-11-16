using Microsoft.EntityFrameworkCore;
using Url_Crud_Service.Models;

namespace Url_Crud_Service.Data
{
    public class CrudDbContext : DbContext
    {
        public CrudDbContext(DbContextOptions<CrudDbContext> options) : base(options)
        { }

        public DbSet<UrlCrud> UrlCruds { get; set; }
    }
}
