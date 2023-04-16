using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace ServerApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<AccountModel> Accounts { get; set; }
    }
}
