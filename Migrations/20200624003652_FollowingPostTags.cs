using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations
{
    public partial class FollowingPostTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowingPostTags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowingPostTags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shops_Handle",
                table: "Shops",
                column: "Handle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Handle",
                table: "AspNetUsers",
                column: "Handle",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowingPostTags");

            migrationBuilder.DropIndex(
                name: "IX_Shops_Handle",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Handle",
                table: "AspNetUsers");
        }
    }
}
