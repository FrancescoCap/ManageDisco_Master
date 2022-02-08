using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class TABLEORDER_ADD_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableOrderRow_Product_Productid",
                table: "TableOrderRow");

            migrationBuilder.RenameColumn(
                name: "Productid",
                table: "TableOrderRow",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_TableOrderRow_Productid",
                table: "TableOrderRow",
                newName: "IX_TableOrderRow_ProductId");

            migrationBuilder.AddColumn<int>(
                name: "TableOrderHeaderId",
                table: "TableOrderRow",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TableOrderRow_TableOrderHeaderId",
                table: "TableOrderRow",
                column: "TableOrderHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableOrderRow_Product_ProductId",
                table: "TableOrderRow",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TableOrderRow_TableOrderHeader_TableOrderHeaderId",
                table: "TableOrderRow",
                column: "TableOrderHeaderId",
                principalTable: "TableOrderHeader",
                principalColumn: "TableOrderHeaderId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableOrderRow_Product_ProductId",
                table: "TableOrderRow");

            migrationBuilder.DropForeignKey(
                name: "FK_TableOrderRow_TableOrderHeader_TableOrderHeaderId",
                table: "TableOrderRow");

            migrationBuilder.DropIndex(
                name: "IX_TableOrderRow_TableOrderHeaderId",
                table: "TableOrderRow");

            migrationBuilder.DropColumn(
                name: "TableOrderHeaderId",
                table: "TableOrderRow");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "TableOrderRow",
                newName: "Productid");

            migrationBuilder.RenameIndex(
                name: "IX_TableOrderRow_ProductId",
                table: "TableOrderRow",
                newName: "IX_TableOrderRow_Productid");

            migrationBuilder.AddForeignKey(
                name: "FK_TableOrderRow_Product_Productid",
                table: "TableOrderRow",
                column: "Productid",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
