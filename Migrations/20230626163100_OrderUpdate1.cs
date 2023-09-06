using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppMvc.Net.Migrations
{
    public partial class OrderUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CartId",
                table: "Orders",
                newName: "OrderId");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Voucher",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "orderProduct",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderProduct", x => new { x.ProductId, x.OrderId });
                    table.ForeignKey(
                        name: "FK_orderProduct_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orderProduct_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Voucher_OrderId",
                table: "Voucher",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_orderProduct_OrderId",
                table: "orderProduct",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_Orders_OrderId",
                table: "Voucher",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_Orders_OrderId",
                table: "Voucher");

            migrationBuilder.DropTable(
                name: "orderProduct");

            migrationBuilder.DropIndex(
                name: "IX_Voucher_OrderId",
                table: "Voucher");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Voucher");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Orders",
                newName: "CartId");

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
