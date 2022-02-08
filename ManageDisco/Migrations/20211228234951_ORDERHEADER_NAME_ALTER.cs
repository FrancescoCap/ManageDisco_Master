using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ORDERHEADER_NAME_ALTER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TableOrderSpending",
                table: "TableOrderHeader",
                newName: "TableOrderHeaderSpending");

            migrationBuilder.RenameColumn(
                name: "TableOrderExit",
                table: "TableOrderHeader",
                newName: "TableOrderHeaderExit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TableOrderHeaderSpending",
                table: "TableOrderHeader",
                newName: "TableOrderSpending");

            migrationBuilder.RenameColumn(
                name: "TableOrderHeaderExit",
                table: "TableOrderHeader",
                newName: "TableOrderExit");
        }
    }
}
