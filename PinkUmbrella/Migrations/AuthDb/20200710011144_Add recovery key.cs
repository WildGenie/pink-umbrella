using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations.AuthDb
{
    public partial class Addrecoverykey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecoveryKeys",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    Label = table.Column<string>(maxLength: 100, nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    WhenShown = table.Column<DateTime>(nullable: true),
                    WhenUsed = table.Column<DateTime>(nullable: true),
                    Code = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecoveryKeys", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecoveryKeys");
        }
    }
}
