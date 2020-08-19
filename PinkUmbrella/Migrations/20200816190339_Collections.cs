using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations
{
    public partial class Collections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttributedToCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudienceCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BccCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BtoCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CcCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InReplyToCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepliesCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagCSV",
                table: "ObjectContentModel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToCSV",
                table: "ObjectContentModel",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "AttributedToCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "AudienceCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "BccCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "BtoCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "CcCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "ContextCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "IconCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "ImageCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "InReplyToCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "LocationCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "RepliesCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "TagCSV",
                table: "ObjectContentModel");

            migrationBuilder.DropColumn(
                name: "ToCSV",
                table: "ObjectContentModel");
        }
    }
}
