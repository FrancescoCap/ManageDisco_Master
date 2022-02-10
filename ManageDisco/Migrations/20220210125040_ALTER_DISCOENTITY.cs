using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_DISCOENTITY : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscoAddress",
                table: "DiscoEntity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscoCity",
                table: "DiscoEntity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscoProvince",
                table: "DiscoEntity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscoVatCode",
                table: "DiscoEntity",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscoAddress",
                table: "DiscoEntity");

            migrationBuilder.DropColumn(
                name: "DiscoCity",
                table: "DiscoEntity");

            migrationBuilder.DropColumn(
                name: "DiscoProvince",
                table: "DiscoEntity");

            migrationBuilder.DropColumn(
                name: "DiscoVatCode",
                table: "DiscoEntity");
        }
    }
}
