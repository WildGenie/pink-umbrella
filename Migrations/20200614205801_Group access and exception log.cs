using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations
{
    public partial class Groupaccessandexceptionlog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupAccessCodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    WhenExpires = table.Column<DateTime>(nullable: false),
                    WhenConsumed = table.Column<DateTime>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    CreatedByUserId = table.Column<int>(nullable: false),
                    ForUserId = table.Column<int>(nullable: false),
                    GroupName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAccessCodes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupAccessCodes");
        }
    }
}
