using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class RESERVATION_TABLE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TableId",
                table: "Reservation",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_TableId",
                table: "Reservation",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Table_TableId",
                table: "Reservation",
                column: "TableId",
                principalTable: "Table",
                principalColumn: "TableId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Table_TableId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_TableId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "Reservation");
        }
    }
}
