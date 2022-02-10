using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_COLUMN_NAME_EVENTPHOTOTYPE_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPhoto_PhotoType_EventPhotoTypeId",
                table: "EventPhoto");

            migrationBuilder.RenameColumn(
                name: "EventPhotoTypeId",
                table: "EventPhoto",
                newName: "PhotoTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_EventPhoto_EventPhotoTypeId",
                table: "EventPhoto",
                newName: "IX_EventPhoto_PhotoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPhoto_PhotoType_PhotoTypeId",
                table: "EventPhoto",
                column: "PhotoTypeId",
                principalTable: "PhotoType",
                principalColumn: "PhotoTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPhoto_PhotoType_PhotoTypeId",
                table: "EventPhoto");

            migrationBuilder.RenameColumn(
                name: "PhotoTypeId",
                table: "EventPhoto",
                newName: "EventPhotoTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_EventPhoto_PhotoTypeId",
                table: "EventPhoto",
                newName: "IX_EventPhoto_EventPhotoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPhoto_PhotoType_EventPhotoTypeId",
                table: "EventPhoto",
                column: "EventPhotoTypeId",
                principalTable: "PhotoType",
                principalColumn: "PhotoTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
