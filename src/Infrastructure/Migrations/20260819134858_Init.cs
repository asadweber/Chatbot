using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    OrderQty = table.Column<long>(type: "bigint", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "City", "CreatedAt", "Industry", "Name", "State" },
                values: new object[,]
                {
                    { 1, "Chicago", new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Manufacturing", "Acme Corp", "IL" },
                    { 2, "Austin", new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Technology", "Globex Inc", "TX" },
                    { 3, "Houston", new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Software", "Initech", "TX" },
                    { 4, "Raleigh", new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Pharmaceuticals", "Umbrella LLC", "NC" },
                    { 5, "Denver", new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Food & Beverage", "Soylent Corp", "CO" },
                    { 6, "San Jose", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Technology", "Hooli", "CA" },
                    { 7, "New York", new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Defense", "Stark Industries", "NY" },
                    { 8, "Gotham", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Conglomerate", "Wayne Enterprises", "NJ" },
                    { 9, "Portland", new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Food & Beverage", "Wonka Industries", "OR" },
                    { 10, "Los Angeles", new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Robotics", "Cyberdyne Systems", "CA" },
                    { 11, "Boston", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Biotech", "Massive Dynamic", "MA" },
                    { 12, "Seattle", new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Research", "Aperture Science", "WA" },
                    { 13, "New York", new DateTime(2026, 1, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Chemicals", "Oscorp", "NY" },
                    { 14, "Los Angeles", new DateTime(2026, 1, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Biotechnology", "Tyrell Corp", "CA" },
                    { 15, "Phoenix", new DateTime(2026, 1, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Aerospace", "Weyland-Yutani", "AZ" },
                    { 16, "Charlotte", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Finance", "Gringotts Ltd", "NC" },
                    { 17, "Miami", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Media", "Prestige Worldwide", "FL" },
                    { 18, "New York", new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Import/Export", "Vandelay Industries", "NY" },
                    { 19, "Springfield", new DateTime(2026, 1, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Food & Beverage", "Duff Brewing", "OH" },
                    { 20, "Atlanta", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Consulting", "Monarch Solutions", "GA" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1L, "Laptop", 999.99m, 50L },
                    { 2L, "Wireless Mouse", 29.99m, 200L },
                    { 3L, "USB-C Hub", 49.99m, 150L },
                    { 4L, "Mechanical Keyboard", 89.99m, 75L },
                    { 5L, "Monitor 27\"", 349.99m, 30L }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CustomerId", "OrderDate", "Status", "TotalAmount" },
                values: new object[,]
                {
                    { 1L, 1, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 1099.98m },
                    { 2L, 2, new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 49.99m },
                    { 3L, 3, new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Pending", 89.99m },
                    { 4L, 4, new DateTime(2026, 2, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 349.99m },
                    { 5L, 5, new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Shipped", 999.99m },
                    { 6L, 6, new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Pending", 29.99m },
                    { 7L, 7, new DateTime(2026, 2, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 139.98m },
                    { 8L, 8, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Shipped", 999.99m },
                    { 9L, 9, new DateTime(2026, 2, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 49.99m },
                    { 10L, 10, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Pending", 349.99m },
                    { 11L, 11, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 89.99m },
                    { 12L, 12, new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Shipped", 999.99m },
                    { 13L, 13, new DateTime(2026, 2, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 29.99m },
                    { 14L, 14, new DateTime(2026, 2, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Pending", 349.99m },
                    { 15L, 15, new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 49.99m },
                    { 16L, 16, new DateTime(2026, 2, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Shipped", 89.99m },
                    { 17L, 17, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 999.99m },
                    { 18L, 18, new DateTime(2026, 2, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Pending", 29.99m },
                    { 19L, 19, new DateTime(2026, 2, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", 349.99m },
                    { 20L, 20, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Shipped", 89.99m }
                });

            migrationBuilder.InsertData(
                table: "OrderDetails",
                columns: new[] { "Id", "OrderId", "OrderQty", "ProductId", "Total", "UnitPrice" },
                values: new object[,]
                {
                    { 1L, 1L, 1L, 1L, 999.99m, 999.99m },
                    { 2L, 1L, 1L, 2L, 99.99m, 99.99m },
                    { 3L, 2L, 1L, 3L, 49.99m, 49.99m },
                    { 4L, 3L, 1L, 4L, 89.99m, 89.99m },
                    { 5L, 4L, 1L, 5L, 349.99m, 349.99m },
                    { 6L, 5L, 1L, 1L, 999.99m, 999.99m },
                    { 7L, 6L, 1L, 2L, 29.99m, 29.99m },
                    { 8L, 7L, 1L, 3L, 49.99m, 49.99m },
                    { 9L, 7L, 3L, 2L, 89.99m, 29.99m },
                    { 10L, 8L, 1L, 1L, 999.99m, 999.99m },
                    { 11L, 9L, 1L, 3L, 49.99m, 49.99m },
                    { 12L, 10L, 1L, 5L, 349.99m, 349.99m },
                    { 13L, 11L, 1L, 4L, 89.99m, 89.99m },
                    { 14L, 12L, 1L, 1L, 999.99m, 999.99m },
                    { 15L, 13L, 1L, 2L, 29.99m, 29.99m },
                    { 16L, 14L, 1L, 5L, 349.99m, 349.99m },
                    { 17L, 15L, 1L, 3L, 49.99m, 49.99m },
                    { 18L, 16L, 1L, 4L, 89.99m, 89.99m },
                    { 19L, 17L, 1L, 1L, 999.99m, 999.99m },
                    { 20L, 18L, 1L, 2L, 29.99m, 29.99m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
