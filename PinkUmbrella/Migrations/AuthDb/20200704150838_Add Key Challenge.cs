using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations.AuthDb
{
    public partial class AddKeyChallenge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyChallenges",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KeyId = table.Column<long>(nullable: false),
                    Challenge = table.Column<byte[]>(nullable: true),
                    Expires = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyChallenges", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyChallenges");
        }
    }
}
