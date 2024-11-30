using Api.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class AppDbContext:IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { 
        }

        public DbSet<Order> orders =>Set<Order>();
        public DbSet<AppFile> files => Set<AppFile>();
    }
}
