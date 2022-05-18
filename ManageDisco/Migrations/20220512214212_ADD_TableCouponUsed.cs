using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_TableCouponUsed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableCouponUsed",
                columns: table => new
                {
                    TableCouponId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableCouponTableId = table.Column<int>(type: "int", nullable: false),
                    TableCouponCouponCode = table.Column<int>(type: "int", nullable: false),
                    TableCouponEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableCouponUsed", x => x.TableCouponId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableCouponUsed");
        }
    }
}
