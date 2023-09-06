using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppMvc.Net.Migrations
{
    public partial class updatedb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_Orders_OrderId",
                table: "Voucher");

            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_Users_UserId",
                table: "Voucher");

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_AppUser",
                table: "Voucher",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_Order",
                table: "Voucher",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_AppUser",
                table: "Voucher");

            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_Order",
                table: "Voucher");

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_Orders_OrderId",
                table: "Voucher",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_Users_UserId",
                table: "Voucher",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
