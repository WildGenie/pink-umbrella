using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations.AuthDb
{
    public partial class Addauth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FIDOCredentials",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    PublicKey = table.Column<byte[]>(nullable: true),
                    AaGuid = table.Column<Guid>(nullable: false),
                    CredType = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    SignatureCounter = table.Column<uint>(nullable: false),
                    TransportTypes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIDOCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IPBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WhenBlocked = table.Column<DateTime>(nullable: false),
                    ByUserId = table.Column<int>(nullable: false),
                    IPId = table.Column<long>(nullable: false),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicKeys",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WhenAdded = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Format = table.Column<int>(nullable: false),
                    FingerPrint = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SitePermissions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuthId = table.Column<long>(nullable: false),
                    Permission = table.Column<int>(nullable: false),
                    OverrideValue = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IPs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    PublicKeyId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IPs_PublicKeys_PublicKeyId",
                        column: x => x.PublicKeyId,
                        principalTable: "PublicKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivateKeys",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(nullable: false),
                    Format = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    WhenAdded = table.Column<DateTime>(nullable: false),
                    PublicKeyId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateKeys_PublicKeys_PublicKeyId",
                        column: x => x.PublicKeyId,
                        principalTable: "PublicKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAuthKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    PublicKeyId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuthKeys_PublicKeys_PublicKeyId",
                        column: x => x.PublicKeyId,
                        principalTable: "PublicKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IPs_PublicKeyId",
                table: "IPs",
                column: "PublicKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateKeys_PublicKeyId",
                table: "PrivateKeys",
                column: "PublicKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthKeys_PublicKeyId",
                table: "UserAuthKeys",
                column: "PublicKeyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FIDOCredentials");

            migrationBuilder.DropTable(
                name: "IPBlocks");

            migrationBuilder.DropTable(
                name: "IPs");

            migrationBuilder.DropTable(
                name: "PrivateKeys");

            migrationBuilder.DropTable(
                name: "SitePermissions");

            migrationBuilder.DropTable(
                name: "UserAuthKeys");

            migrationBuilder.DropTable(
                name: "PublicKeys");
        }
    }
}
