using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_ANONYMUSALLOWED : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnonymusPath",
                table: "AnonymusAllowed",
                newName: "RedirectedPath");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "AnonymusAllowed",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "AnonymusAllowed");

            migrationBuilder.RenameColumn(
                name: "RedirectedPath",
                table: "AnonymusAllowed",
                newName: "AnonymusPath");
        }
    }
}
