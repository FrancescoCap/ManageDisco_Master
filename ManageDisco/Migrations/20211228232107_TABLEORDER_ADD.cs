using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class TABLEORDER_ADD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableOrderHeader",
                columns: table => new
                {
                    TableOrderHeaderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableOrderSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TableOrderExit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableOrderHeader", x => x.TableOrderHeaderId);
                });

            migrationBuilder.CreateTable(
                name: "TableOrderRow",
                columns: table => new
                {
                    TableOrderRowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    Productid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableOrderRow", x => x.TableOrderRowId);
                    table.ForeignKey(
                        name: "FK_TableOrderRow_Product_Productid",
                        column: x => x.Productid,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableOrderRow_Table_TableId",
                        column: x => x.TableId,
                        principalTable: "Table",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableOrderRow_Productid",
                table: "TableOrderRow",
                column: "Productid");

            migrationBuilder.CreateIndex(
                name: "IX_TableOrderRow_TableId",
                table: "TableOrderRow",
                column: "TableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableOrderHeader");

            migrationBuilder.DropTable(
                name: "TableOrderRow");
        }
    }
}
