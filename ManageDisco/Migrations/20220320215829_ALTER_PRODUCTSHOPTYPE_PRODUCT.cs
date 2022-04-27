using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_PRODUCTSHOPTYPE_PRODUCT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductShopTypeId",
                table: "ProductShop",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductShop_ProductShopTypeId",
                table: "ProductShop",
                column: "ProductShopTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductShop_ProductShopType_ProductShopTypeId",
                table: "ProductShop",
                column: "ProductShopTypeId",
                principalTable: "ProductShopType",
                principalColumn: "ProductShopTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductShop_ProductShopType_ProductShopTypeId",
                table: "ProductShop");

            migrationBuilder.DropIndex(
                name: "IX_ProductShop_ProductShopTypeId",
                table: "ProductShop");

            migrationBuilder.DropColumn(
                name: "ProductShopTypeId",
                table: "ProductShop");
        }
    }
}
