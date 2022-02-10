using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class HOMEPHOTO_ALTER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HomePhotoDescription",
                table: "HomePhoto",
                newName: "HomePhotoPath");

            migrationBuilder.AddColumn<int>(
                name: "PhotoTypeId",
                table: "HomePhoto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HomePhoto_PhotoTypeId",
                table: "HomePhoto",
                column: "PhotoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_HomePhoto_PhotoType_PhotoTypeId",
                table: "HomePhoto",
                column: "PhotoTypeId",
                principalTable: "PhotoType",
                principalColumn: "PhotoTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomePhoto_PhotoType_PhotoTypeId",
                table: "HomePhoto");

            migrationBuilder.DropIndex(
                name: "IX_HomePhoto_PhotoTypeId",
                table: "HomePhoto");

            migrationBuilder.DropColumn(
                name: "PhotoTypeId",
                table: "HomePhoto");

            migrationBuilder.RenameColumn(
                name: "HomePhotoPath",
                table: "HomePhoto",
                newName: "HomePhotoDescription");
        }
    }
}
