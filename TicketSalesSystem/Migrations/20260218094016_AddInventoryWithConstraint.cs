using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSalesSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryWithConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "TicketsArea",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Remaining",
                table: "TicketsArea",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // B. 🚩 手動加上 SQL 約束 (這是關鍵)
            // 這樣在建立欄位的當下，資料庫就會立刻擁有這層物理保護
            migrationBuilder.Sql("ALTER TABLE TicketsArea ADD CONSTRAINT CHK_Inventory_Logic CHECK (Remaining >= 0 AND Remaining <= Capacity);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //同樣在還原時，也要記得先移除約束，再移除欄位
            migrationBuilder.Sql("ALTER TABLE TicketsArea DROP CONSTRAINT CHK_Inventory_Logic;");

            migrationBuilder.DropColumn(name: "Capacity", table: "TicketsArea");
            migrationBuilder.DropColumn(name: "Remaining", table: "TicketsArea");
        }
    }
}
