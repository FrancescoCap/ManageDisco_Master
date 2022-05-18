using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_TablePreOrderRow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TablePreOrderRow",
                columns: table => new
                {
                    TablePreOrderRowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TablePreOrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TablePreOrderRowQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TablePreOrderRow", x => x.TablePreOrderRowId);
                    table.ForeignKey(
                        name: "FK_TablePreOrderRow_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TablePreOrderRow_TableOrderHeader_TablePreOrderHeaderId",
                        column: x => x.TablePreOrderHeaderId,
                        principalTable: "TableOrderHeader",
                        principalColumn: "TableOrderHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TablePreOrderRow_ProductId",
                table: "TablePreOrderRow",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TablePreOrderRow_TablePreOrderHeaderId",
                table: "TablePreOrderRow",
                column: "TablePreOrderHeaderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TablePreOrderRow");
        }
    }
}
