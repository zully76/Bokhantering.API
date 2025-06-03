using Microsoft.EntityFrameworkCore;
using Bokhantering.API.Models;


namespace Bokhantering.API.Data
{
    public class BokhanteringDbContext:DbContext
    {
        public BokhanteringDbContext(DbContextOptions<BokhanteringDbContext> options) : base(options)
        {
        }

    public DbSet<Bok> Boks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}




