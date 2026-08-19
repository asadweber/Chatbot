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
