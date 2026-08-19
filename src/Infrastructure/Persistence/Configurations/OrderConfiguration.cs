using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Order { Id = 1, CustomerId = 1, OrderDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 1099.98m, Status = "Completed" },
            new Order { Id = 2, CustomerId = 2, OrderDate = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 49.99m, Status = "Completed" },
            new Order { Id = 3, CustomerId = 3, OrderDate = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 89.99m, Status = "Pending" },
            new Order { Id = 4, CustomerId = 4, OrderDate = new DateTime(2026, 2, 4, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 349.99m, Status = "Completed" },
            new Order { Id = 5, CustomerId = 5, OrderDate = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 999.99m, Status = "Shipped" },
            new Order { Id = 6, CustomerId = 6, OrderDate = new DateTime(2026, 2, 6, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 29.99m, Status = "Pending" },
            new Order { Id = 7, CustomerId = 7, OrderDate = new DateTime(2026, 2, 7, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 139.98m, Status = "Completed" },
            new Order { Id = 8, CustomerId = 8, OrderDate = new DateTime(2026, 2, 8, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 999.99m, Status = "Shipped" },
            new Order { Id = 9, CustomerId = 9, OrderDate = new DateTime(2026, 2, 9, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 49.99m, Status = "Completed" },
            new Order { Id = 10, CustomerId = 10, OrderDate = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 349.99m, Status = "Pending" },
            new Order { Id = 11, CustomerId = 11, OrderDate = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 89.99m, Status = "Completed" },
            new Order { Id = 12, CustomerId = 12, OrderDate = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 999.99m, Status = "Shipped" },
            new Order { Id = 13, CustomerId = 13, OrderDate = new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 29.99m, Status = "Completed" },
            new Order { Id = 14, CustomerId = 14, OrderDate = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 349.99m, Status = "Pending" },
            new Order { Id = 15, CustomerId = 15, OrderDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 49.99m, Status = "Completed" },
            new Order { Id = 16, CustomerId = 16, OrderDate = new DateTime(2026, 2, 16, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 89.99m, Status = "Shipped" },
            new Order { Id = 17, CustomerId = 17, OrderDate = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 999.99m, Status = "Completed" },
            new Order { Id = 18, CustomerId = 18, OrderDate = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 29.99m, Status = "Pending" },
            new Order { Id = 19, CustomerId = 19, OrderDate = new DateTime(2026, 2, 19, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 349.99m, Status = "Completed" },
            new Order { Id = 20, CustomerId = 20, OrderDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), TotalAmount = 89.99m, Status = "Shipped" }
        );
    }
}
