using Microsoft.EntityFrameworkCore.Migrations;

namespace seattle.Migrations
{
    public partial class Nextinchain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NextInChain",
                table: "Posts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextInChain",
                table: "Posts");
        }
    }
}
