using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace seattle.Migrations
{
    public partial class WhenCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "WhenCreated",
                table: "Inventories",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhenCreated",
                table: "Inventories");
        }
    }
}
