using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppMvc.Net.Migrations
{
    public partial class Order : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "CartsManage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CartsManage",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartsManage_UserId",
                table: "CartsManage",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartsManage_Users_UserId",
                table: "CartsManage",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartsManage_Users_UserId",
                table: "CartsManage");

            migrationBuilder.DropIndex(
                name: "IX_CartsManage_UserId",
                table: "CartsManage");

            migrationBuilder.DropColumn(
                name: "Completed",
                table: "CartsManage");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CartsManage");
        }
    }
}
