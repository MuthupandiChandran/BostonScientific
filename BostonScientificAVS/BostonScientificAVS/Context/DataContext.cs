using BostonScientificAVS.Entity;
using BostonScientificAVS.Models;
using Entity;
using Microsoft.EntityFrameworkCore;

namespace Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<ItemMaster> ItemMaster { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.EmpID)
                .IsUnique();
        }

    }
}
