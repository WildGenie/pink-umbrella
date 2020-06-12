using Microsoft.EntityFrameworkCore.Migrations;

namespace seattle.Migrations
{
    public partial class Displaynameanddescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Inventories",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Inventories",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Inventories");
        }
    }
}
