using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations.AhPushItDb
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MethodSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Method = table.Column<int>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    FromUserId = table.Column<int>(nullable: false),
                    Subject = table.Column<int>(nullable: false),
                    SubjectId = table.Column<int>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    DataJson = table.Column<string>(nullable: true),
                    RecipientCount = table.Column<int>(nullable: false),
                    DeliveryCount = table.Column<int>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false),
                    DismissCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ToUserId = table.Column<int>(nullable: false),
                    NotificationId = table.Column<int>(nullable: false),
                    Method = table.Column<int>(nullable: false),
                    WhenDelivered = table.Column<DateTime>(nullable: true),
                    WhenViewed = table.Column<DateTime>(nullable: true),
                    WhenDismissed = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipients", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MethodSettings");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Recipients");
        }
    }
}
