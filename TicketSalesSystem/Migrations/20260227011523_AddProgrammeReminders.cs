using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSalesSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddProgrammeReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollectionReminder",
                table: "Programme",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notice",
                table: "Programme",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PurchaseReminder",
                table: "Programme",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefundPolicy",
                table: "Programme",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectionReminder",
                table: "Programme");

            migrationBuilder.DropColumn(
                name: "Notice",
                table: "Programme");

            migrationBuilder.DropColumn(
                name: "PurchaseReminder",
                table: "Programme");

            migrationBuilder.DropColumn(
                name: "RefundPolicy",
                table: "Programme");
        }
    }
}
