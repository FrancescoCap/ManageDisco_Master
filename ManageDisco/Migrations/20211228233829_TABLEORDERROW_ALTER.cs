using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class TABLEORDERROW_ALTER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableOrderRow_Table_TableId",
                table: "TableOrderRow");

            migrationBuilder.DropIndex(
                name: "IX_TableOrderRow_TableId",
                table: "TableOrderRow");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "TableOrderRow");

            migrationBuilder.AddColumn<int>(
                name: "TableId",
                table: "TableOrderHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TableOrderHeader_TableId",
                table: "TableOrderHeader",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableOrderHeader_Table_TableId",
                table: "TableOrderHeader",
                column: "TableId",
                principalTable: "Table",
                principalColumn: "TableId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableOrderHeader_Table_TableId",
                table: "TableOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_TableOrderHeader_TableId",
                table: "TableOrderHeader");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "TableOrderHeader");

            migrationBuilder.AddColumn<int>(
                name: "TableId",
                table: "TableOrderRow",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TableOrderRow_TableId",
                table: "TableOrderRow",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableOrderRow_Table_TableId",
                table: "TableOrderRow",
                column: "TableId",
                principalTable: "Table",
                principalColumn: "TableId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
