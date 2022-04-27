using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class DELETE_PAYMENTDESCRIPTION : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentOverviewDescription",
                table: "PaymentOverview");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentOverviewDescription",
                table: "PaymentOverview",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
