using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipmentService.Migrations
{
    /// <inheritdoc />
    public partial class AddedMoreIndexesForShipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Tenant_IsDeleted_CreatedAt",
                table: "Shipments",
                columns: new[] { "Tenant", "IsDeleted", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Tenant_IsDeleted_ID",
                table: "Shipments",
                columns: new[] { "Tenant", "IsDeleted", "ID" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Tenant_IsDeleted_Status",
                table: "Shipments",
                columns: new[] { "Tenant", "IsDeleted", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_Tenant_IsDeleted_CreatedAt",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_Tenant_IsDeleted_ID",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_Tenant_IsDeleted_Status",
                table: "Shipments");
        }
    }
}
