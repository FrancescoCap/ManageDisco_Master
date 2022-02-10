using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_CONTACT_CONTACT_TYPE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactTypeId",
                table: "Contact",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Contact_ContactTypeId",
                table: "Contact",
                column: "ContactTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contact_ContactType_ContactTypeId",
                table: "Contact",
                column: "ContactTypeId",
                principalTable: "ContactType",
                principalColumn: "ContactTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contact_ContactType_ContactTypeId",
                table: "Contact");

            migrationBuilder.DropIndex(
                name: "IX_Contact_ContactTypeId",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "ContactTypeId",
                table: "Contact");
        }
    }
}
