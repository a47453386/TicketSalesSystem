using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSalesSystem.Migrations
{
    /// <inheritdoc />
    public partial class SyncOrderRelationFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AddColumn<string>(
                name: "OrderID",
                table: "Question",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Question_OrderID",
                table: "Question",
                column: "OrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Order_OrderID",
                table: "Question",
                column: "OrderID",
                principalTable: "Order",
                principalColumn: "OrderID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_Order_OrderID",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_Question_OrderID",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "Question");

            
        }
    }
}
