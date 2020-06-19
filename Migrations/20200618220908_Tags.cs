using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations
{
    public partial class Tags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostTags");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Shops",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalUsernamesJson",
                table: "Shops",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuLink",
                table: "Shops",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "Shops",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WebsiteLink",
                table: "Shops",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "WhenDeleted",
                table: "Shops",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AllTags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(maxLength: 100, nullable: true),
                    ContainsProfanity = table.Column<bool>(nullable: false),
                    LikeCount = table.Column<int>(nullable: false),
                    DislikeCount = table.Column<int>(nullable: false),
                    BlockCount = table.Column<int>(nullable: false),
                    CreatedByUserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaggedModel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ToId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaggedModel", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllTags");

            migrationBuilder.DropTable(
                name: "TaggedModel");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ExternalUsernamesJson",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "MenuLink",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "WebsiteLink",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "WhenDeleted",
                table: "Shops");

            migrationBuilder.CreateTable(
                name: "PostTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTags", x => x.Id);
                });
        }
    }
}
