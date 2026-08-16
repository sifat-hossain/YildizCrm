using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Yildz.CRM.Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Policies_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), null, null, "Acme Corporation", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), null, null, "GlobalTech Industries", null, null }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Name", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000011"), null, null, "John Doe", new Guid("00000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("00000000-0000-0000-0000-000000000012"), null, null, "Jane Smith", new Guid("00000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("00000000-0000-0000-0000-000000000021"), null, null, "Bob Johnson", new Guid("00000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("00000000-0000-0000-0000-000000000022"), null, null, "Alice Williams", new Guid("00000000-0000-0000-0000-000000000002"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "CustomerId", "ExpirationDate", "PolicyNumber", "PremiumAmount", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000111"), null, null, new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2026, 8, 15, 0, 0, 0, 0, DateTimeKind.Utc), "POL-001", 1200.00m, null, null },
                    { new Guid("00000000-0000-0000-0000-000000000112"), null, null, new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), "POL-002", 850.00m, null, null },
                    { new Guid("00000000-0000-0000-0000-000000000113"), null, null, new Guid("00000000-0000-0000-0000-000000000012"), new DateTime(2026, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), "POL-003", 1500.00m, null, null },
                    { new Guid("00000000-0000-0000-0000-000000000211"), null, null, new Guid("00000000-0000-0000-0000-000000000021"), new DateTime(2026, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), "POL-004", 2000.00m, null, null },
                    { new Guid("00000000-0000-0000-0000-000000000212"), null, null, new Guid("00000000-0000-0000-0000-000000000022"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "POL-005", 950.00m, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedAt",
                table: "Customers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsActive",
                table: "Customers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CreatedAt",
                table: "Policies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CustomerId",
                table: "Policies",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ExpirationDate",
                table: "Policies",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_IsActive",
                table: "Policies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_IsDeleted",
                table: "Policies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CreatedAt",
                table: "Tenants",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_IsActive",
                table: "Tenants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_IsDeleted",
                table: "Tenants",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
