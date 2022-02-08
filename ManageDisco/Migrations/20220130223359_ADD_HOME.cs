using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_HOME : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Home",
                columns: table => new
                {
                    HomeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscoEntityId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Home", x => x.HomeId);
                    table.ForeignKey(
                        name: "FK_Home_DiscoEntity_DiscoEntityId",
                        column: x => x.DiscoEntityId,
                        principalTable: "DiscoEntity",
                        principalColumn: "DiscoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Home_DiscoEntityId",
                table: "Home",
                column: "DiscoEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Home");
        }
    }
}
