using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_PRODUCTSHOPROW : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductShopRow",
                columns: table => new
                {
                    ProductShopRowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductShopRowQuantity = table.Column<int>(type: "int", nullable: false),
                    ProductShopHeaderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductShopRow", x => x.ProductShopRowId);
                    table.ForeignKey(
                        name: "FK_ProductShopRow_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductShopRow_ProductShopHeader_ProductShopHeaderId",
                        column: x => x.ProductShopHeaderId,
                        principalTable: "ProductShopHeader",
                        principalColumn: "ProductShopHeaderIdId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductShopRow_ProductId",
                table: "ProductShopRow",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductShopRow_ProductShopHeaderId",
                table: "ProductShopRow",
                column: "ProductShopHeaderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductShopRow");
        }
    }
}
