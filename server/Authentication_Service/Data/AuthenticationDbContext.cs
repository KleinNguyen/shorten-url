using Microsoft.EntityFrameworkCore;
using Authentication_Service.Models;
namespace Authentication_Service.Data
{
    public class AuthenticationDbContext : DbContext
    {
        public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
        { }

        public DbSet<User> Users { get; set; }
    }
}
