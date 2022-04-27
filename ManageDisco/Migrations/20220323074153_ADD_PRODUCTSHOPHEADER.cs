using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_PRODUCTSHOPHEADER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductShopHeader",
                columns: table => new
                {
                    ProductShopHeaderIdId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductShopHeaderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductShopHeaderDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductShopHeaderPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductShopTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductShopHeader", x => x.ProductShopHeaderIdId);
                    table.ForeignKey(
                        name: "FK_ProductShopHeader_ProductShopType_ProductShopTypeId",
                        column: x => x.ProductShopTypeId,
                        principalTable: "ProductShopType",
                        principalColumn: "ProductShopTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductShopHeader_ProductShopTypeId",
                table: "ProductShopHeader",
                column: "ProductShopTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductShopHeader");
        }
    }
}
