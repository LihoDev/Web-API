using Microsoft.EntityFrameworkCore;

namespace UsersApi.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<UserItem> UserItems { get; set; } = null!;
    }
}
