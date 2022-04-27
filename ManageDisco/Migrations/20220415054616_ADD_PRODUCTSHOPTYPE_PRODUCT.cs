using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_PRODUCTSHOPTYPE_PRODUCT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductShopTypeId",
                table: "Product",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductShopTypeId",
                table: "Product",
                column: "ProductShopTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ProductShopType_ProductShopTypeId",
                table: "Product",
                column: "ProductShopTypeId",
                principalTable: "ProductShopType",
                principalColumn: "ProductShopTypeId",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ProductShopType_ProductShopTypeId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ProductShopTypeId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductShopTypeId",
                table: "Product");
        }
    }
}
