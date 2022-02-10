using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_DISCOENTITY_TIME : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscoClosingTime",
                table: "DiscoEntity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscoOpeningTime",
                table: "DiscoEntity",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscoClosingTime",
                table: "DiscoEntity");

            migrationBuilder.DropColumn(
                name: "DiscoOpeningTime",
                table: "DiscoEntity");
        }
    }
}
