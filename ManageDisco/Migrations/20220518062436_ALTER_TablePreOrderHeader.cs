using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ALTER_TablePreOrderHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "TableOrderHeader");

            migrationBuilder.CreateTable(
                name: "TablePreOrderHeader",
                columns: table => new
                {
                    TablePreOrderHeaderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TablePreOrderHeaderSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TablePreOrderHeaderExit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    TableOrderHeaderCouponCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TablePreOrderHeader", x => x.TablePreOrderHeaderId);
                    table.ForeignKey(
                        name: "FK_TablePreOrderHeader_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TablePreOrderHeader_ReservationId",
                table: "TablePreOrderHeader",
                column: "ReservationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TablePreOrderHeader");

          
            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "TableOrderHeader",
                type: "bit",
                nullable: true);
        }
    }
}
