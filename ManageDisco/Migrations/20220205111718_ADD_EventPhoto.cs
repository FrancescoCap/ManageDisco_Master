using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_EventPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventPhoto",
                columns: table => new
                {
                    EventPhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventPhotoImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventPhotoEventId = table.Column<int>(type: "int", nullable: false),
                    EventPhotoTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPhoto", x => x.EventPhotoId);
                    table.ForeignKey(
                        name: "FK_EventPhoto_EventPhotoType_EventPhotoTypeId",
                        column: x => x.EventPhotoTypeId,
                        principalTable: "EventPhotoType",
                        principalColumn: "EventPhotoTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventPhoto_EventPhotoTypeId",
                table: "EventPhoto",
                column: "EventPhotoTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventPhoto");
        }
    }
}
