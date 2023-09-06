using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppMvc.Net.Migrations
{
    public partial class productmodeledit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BackCamera",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatteryCapacity",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPU",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontCamera",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Memory",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNetWork",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OS",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ram",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScreenResolution",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ScreenSize",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SimQuantity",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechScreen",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Wifi",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "BackCamera",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "BatteryCapacity",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "CPU",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "FrontCamera",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Memory",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "MobileNetWork",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "OS",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Ram",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ScreenResolution",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ScreenSize",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "SimQuantity",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "TechScreen",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Wifi",
                table: "Product");
        }
    }
}
