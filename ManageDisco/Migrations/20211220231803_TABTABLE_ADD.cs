using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class TABTABLE_ADD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Table",
                columns: table => new
                {
                    TableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableAreaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscoEntityId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Table", x => x.TableId);
                    table.ForeignKey(
                        name: "FK_Table_DiscoEntity_DiscoEntityId",
                        column: x => x.DiscoEntityId,
                        principalTable: "DiscoEntity",
                        principalColumn: "DiscoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Table_DiscoEntityId",
                table: "Table",
                column: "DiscoEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Table");
        }
    }
}
