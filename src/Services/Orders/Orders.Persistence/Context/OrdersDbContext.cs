using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Persistence.Context
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                entity.Property(e => e.OrderNumber)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.CustomerId)
                    .IsRequired();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .IsRequired();
                
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.OrderDate)
                    .IsRequired();
                
                entity.Property(e => e.ShippingAddress)
                    .IsRequired()
                    .HasMaxLength(500);
                
                entity.Property(e => e.BillingAddress)
                    .IsRequired()
                    .HasMaxLength(500);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.ModifiedAt);

                // Configure relationship with OrderItems
                entity.HasMany(o => o.OrderItems)
                    .WithOne()
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.OrderDate);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                entity.Property(e => e.OrderId)
                    .IsRequired();
                
                entity.Property(e => e.ProductId)
                    .IsRequired();
                
                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Quantity)
                    .IsRequired();
                
                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.ModifiedAt);

                // Indexes
                entity.HasIndex(e => e.ProductId);
            });
        }
    }
}
