using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_EVENTSTATUS_EVENT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventStatusId",
                table: "Events",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventStatusId",
                table: "Events",
                column: "EventStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventStatus_EventStatusId",
                table: "Events",
                column: "EventStatusId",
                principalTable: "EventStatus",
                principalColumn: "EventStatusId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventStatus_EventStatusId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventStatusId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventStatusId",
                table: "Events");
        }
    }
}
