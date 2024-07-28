using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option)
        {

        }


        // Create UserTable in Sqlite Db
        public virtual DbSet<UserModel> Users {get; set;}

        // Create MessageTable in Sqlite Db
        public DbSet<MessageModel> Messages { get; set;}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
               .HasMany(u => u.Messages) // One-to-Many relationship
               .WithOne(m => m.User) // Back reference
               .HasForeignKey(m => m.UserId) // Foreign key in MessageModel
               .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
