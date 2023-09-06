using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppMvc.Net.Migrations
{
    public partial class Order4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Discount",
                table: "Voucher",
                newName: "PercentageDiscount");

            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "Voucher",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enable",
                table: "Voucher");

            migrationBuilder.RenameColumn(
                name: "PercentageDiscount",
                table: "Voucher",
                newName: "Discount");
        }
    }
}
