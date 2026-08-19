using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.Property(d => d.UnitPrice).HasPrecision(18, 2);
        builder.Property(d => d.Total).HasPrecision(18, 2);

        builder.HasOne(d => d.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new OrderDetail { Id = 1, OrderId = 1, ProductId = 1, OrderQty = 1, UnitPrice = 999.99m, Total = 999.99m },
            new OrderDetail { Id = 2, OrderId = 1, ProductId = 2, OrderQty = 1, UnitPrice = 99.99m, Total = 99.99m },
            new OrderDetail { Id = 3, OrderId = 2, ProductId = 3, OrderQty = 1, UnitPrice = 49.99m, Total = 49.99m },
            new OrderDetail { Id = 4, OrderId = 3, ProductId = 4, OrderQty = 1, UnitPrice = 89.99m, Total = 89.99m },
            new OrderDetail { Id = 5, OrderId = 4, ProductId = 5, OrderQty = 1, UnitPrice = 349.99m, Total = 349.99m },
            new OrderDetail { Id = 6, OrderId = 5, ProductId = 1, OrderQty = 1, UnitPrice = 999.99m, Total = 999.99m },
            new OrderDetail { Id = 7, OrderId = 6, ProductId = 2, OrderQty = 1, UnitPrice = 29.99m, Total = 29.99m },
            new OrderDetail { Id = 8, OrderId = 7, ProductId = 3, OrderQty = 1, UnitPrice = 49.99m, Total = 49.99m },
            new OrderDetail { Id = 9, OrderId = 7, ProductId = 2, OrderQty = 3, UnitPrice = 29.99m, Total = 89.99m },
            new OrderDetail { Id = 10, OrderId = 8, ProductId = 1, OrderQty = 1, UnitPrice = 999.99m, Total = 999.99m },
            new OrderDetail { Id = 11, OrderId = 9, ProductId = 3, OrderQty = 1, UnitPrice = 49.99m, Total = 49.99m },
            new OrderDetail { Id = 12, OrderId = 10, ProductId = 5, OrderQty = 1, UnitPrice = 349.99m, Total = 349.99m },
            new OrderDetail { Id = 13, OrderId = 11, ProductId = 4, OrderQty = 1, UnitPrice = 89.99m, Total = 89.99m },
            new OrderDetail { Id = 14, OrderId = 12, ProductId = 1, OrderQty = 1, UnitPrice = 999.99m, Total = 999.99m },
            new OrderDetail { Id = 15, OrderId = 13, ProductId = 2, OrderQty = 1, UnitPrice = 29.99m, Total = 29.99m },
            new OrderDetail { Id = 16, OrderId = 14, ProductId = 5, OrderQty = 1, UnitPrice = 349.99m, Total = 349.99m },
            new OrderDetail { Id = 17, OrderId = 15, ProductId = 3, OrderQty = 1, UnitPrice = 49.99m, Total = 49.99m },
            new OrderDetail { Id = 18, OrderId = 16, ProductId = 4, OrderQty = 1, UnitPrice = 89.99m, Total = 89.99m },
            new OrderDetail { Id = 19, OrderId = 17, ProductId = 1, OrderQty = 1, UnitPrice = 999.99m, Total = 999.99m },
            new OrderDetail { Id = 20, OrderId = 18, ProductId = 2, OrderQty = 1, UnitPrice = 29.99m, Total = 29.99m }
        );
    }
}
