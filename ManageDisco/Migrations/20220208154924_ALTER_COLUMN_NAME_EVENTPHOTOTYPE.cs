using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_COLUMN_NAME_EVENTPHOTOTYPE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventPhotoDescription",
                table: "EventPhotoType",
                newName: "PhotoTypeDescription");

            migrationBuilder.RenameColumn(
                name: "EventPhotoTypeId",
                table: "EventPhotoType",
                newName: "PhotoTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoTypeDescription",
                table: "EventPhotoType",
                newName: "EventPhotoDescription");

            migrationBuilder.RenameColumn(
                name: "PhotoTypeId",
                table: "EventPhotoType",
                newName: "EventPhotoTypeId");
        }
    }
}
