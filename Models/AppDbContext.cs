using Microsoft.EntityFrameworkCore;

namespace API_CLIENT.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ApiRequest> ApiRequests { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Environment> Environments { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
