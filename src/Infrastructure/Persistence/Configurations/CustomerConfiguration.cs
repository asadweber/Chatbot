using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Industry).HasMaxLength(100);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.State).HasMaxLength(50);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasData(
            new Customer { Id = 1, Name = "Acme Corp", Industry = "Manufacturing", City = "Chicago", State = "IL", CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 2, Name = "Globex Inc", Industry = "Technology", City = "Austin", State = "TX", CreatedAt = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 3, Name = "Initech", Industry = "Software", City = "Houston", State = "TX", CreatedAt = new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 4, Name = "Umbrella LLC", Industry = "Pharmaceuticals", City = "Raleigh", State = "NC", CreatedAt = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 5, Name = "Soylent Corp", Industry = "Food & Beverage", City = "Denver", State = "CO", CreatedAt = new DateTime(2026, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 6, Name = "Hooli", Industry = "Technology", City = "San Jose", State = "CA", CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 7, Name = "Stark Industries", Industry = "Defense", City = "New York", State = "NY", CreatedAt = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 8, Name = "Wayne Enterprises", Industry = "Conglomerate", City = "Gotham", State = "NJ", CreatedAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 9, Name = "Wonka Industries", Industry = "Food & Beverage", City = "Portland", State = "OR", CreatedAt = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 10, Name = "Cyberdyne Systems", Industry = "Robotics", City = "Los Angeles", State = "CA", CreatedAt = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 11, Name = "Massive Dynamic", Industry = "Biotech", City = "Boston", State = "MA", CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 12, Name = "Aperture Science", Industry = "Research", City = "Seattle", State = "WA", CreatedAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 13, Name = "Oscorp", Industry = "Chemicals", City = "New York", State = "NY", CreatedAt = new DateTime(2026, 1, 17, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 14, Name = "Tyrell Corp", Industry = "Biotechnology", City = "Los Angeles", State = "CA", CreatedAt = new DateTime(2026, 1, 18, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 15, Name = "Weyland-Yutani", Industry = "Aerospace", City = "Phoenix", State = "AZ", CreatedAt = new DateTime(2026, 1, 19, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 16, Name = "Gringotts Ltd", Industry = "Finance", City = "Charlotte", State = "NC", CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 17, Name = "Prestige Worldwide", Industry = "Media", City = "Miami", State = "FL", CreatedAt = new DateTime(2026, 1, 21, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 18, Name = "Vandelay Industries", Industry = "Import/Export", City = "New York", State = "NY", CreatedAt = new DateTime(2026, 1, 22, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 19, Name = "Duff Brewing", Industry = "Food & Beverage", City = "Springfield", State = "OH", CreatedAt = new DateTime(2026, 1, 23, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = 20, Name = "Monarch Solutions", Industry = "Consulting", City = "Atlanta", State = "GA", CreatedAt = new DateTime(2026, 1, 24, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
