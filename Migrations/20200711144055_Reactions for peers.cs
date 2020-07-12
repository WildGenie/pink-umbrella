using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations
{
    public partial class Reactionsforpeers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliverToPeerTryCount",
                table: "ReactionModel",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ToPeerId",
                table: "ReactionModel",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhenDeliveredToPeer",
                table: "ReactionModel",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliverToPeerTryCount",
                table: "ReactionModel");

            migrationBuilder.DropColumn(
                name: "ToPeerId",
                table: "ReactionModel");

            migrationBuilder.DropColumn(
                name: "WhenDeliveredToPeer",
                table: "ReactionModel");
        }
    }
}
