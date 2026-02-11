using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipmentService.Migrations
{
    /// <inheritdoc />
    public partial class AddedShipmentValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_Tenant_IsDeleted",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Tenant_Status_IsDeleted",
                table: "Shipments",
                columns: new[] { "Tenant", "Status", "IsDeleted" },
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_Status",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_Tenant_Status_IsDeleted",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Tenant_IsDeleted",
                table: "Shipments",
                columns: new[] { "Tenant", "IsDeleted" },
                filter: "[IsDeleted] = 0");
        }
    }
}
