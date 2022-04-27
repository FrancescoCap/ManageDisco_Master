﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace ManageDisco.Migrations
{
    public partial class ADD_PAYEMENT_DESCRIPTION : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentOverviewDescription",
                table: "PaymentOverview",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentOverviewDescription",
                table: "PaymentOverview");
        }
    }
}
