using Microsoft.EntityFrameworkCore.Migrations;

namespace seattle.Migrations
{
    public partial class followandblock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlockCount",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowCount",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FollowCount",
                table: "AspNetUsers");
        }
    }
}
