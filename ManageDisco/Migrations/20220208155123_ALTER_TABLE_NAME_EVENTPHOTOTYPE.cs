using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_TABLE_NAME_EVENTPHOTOTYPE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPhoto_EventPhotoType_EventPhotoTypeId",
                table: "EventPhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPhotoType",
                table: "EventPhotoType");

            migrationBuilder.RenameTable(
                name: "EventPhotoType",
                newName: "PhotoType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotoType",
                table: "PhotoType",
                column: "PhotoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPhoto_PhotoType_EventPhotoTypeId",
                table: "EventPhoto",
                column: "EventPhotoTypeId",
                principalTable: "PhotoType",
                principalColumn: "PhotoTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPhoto_PhotoType_EventPhotoTypeId",
                table: "EventPhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotoType",
                table: "PhotoType");

            migrationBuilder.RenameTable(
                name: "PhotoType",
                newName: "EventPhotoType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPhotoType",
                table: "EventPhotoType",
                column: "PhotoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPhoto_EventPhotoType_EventPhotoTypeId",
                table: "EventPhoto",
                column: "EventPhotoTypeId",
                principalTable: "EventPhotoType",
                principalColumn: "PhotoTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
