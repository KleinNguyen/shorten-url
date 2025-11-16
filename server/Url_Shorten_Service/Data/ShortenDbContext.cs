using Microsoft.EntityFrameworkCore;
using Url_Shorten_Service.Models;

namespace Url_Shorten_Service.Data
{
    public class ShortenDbContext : DbContext
    {
        public ShortenDbContext(DbContextOptions<ShortenDbContext> options) : base(options)
        { }

        public DbSet<UrlShorten> UrlShortenes { get; set; }
    }
}
