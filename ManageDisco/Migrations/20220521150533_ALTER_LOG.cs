using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_LOG : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorDate",
                table: "Log",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorDate",
                table: "Log");
        }
    }
}
