using Microsoft.EntityFrameworkCore;
using MiniEquipmentMarketplace.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace MiniEquipmentMarketplace.Data
{
    public class AppDbContext : IdentityDbContext
    
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Equipment> Equipment { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Equipment>()
            .Property(e => e.Price)
            .HasPrecision(18, 2);  // makes SQL column decimal(18,2)
    }

    }
}
