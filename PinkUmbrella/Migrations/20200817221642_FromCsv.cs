using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations
{
    public partial class FromCsv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromCSV",
                table: "ObjectContentModel",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromCSV",
                table: "ObjectContentModel");
        }
    }
}
