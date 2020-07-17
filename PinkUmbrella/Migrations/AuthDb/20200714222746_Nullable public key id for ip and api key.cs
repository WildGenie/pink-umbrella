using Microsoft.EntityFrameworkCore.Migrations;

namespace PinkUmbrella.Migrations.AuthDb
{
    public partial class Nullablepublickeyidforipandapikey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiAuthKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientPublicKeyId = table.Column<long>(nullable: false),
                    ServerPublicKeyId = table.Column<long>(nullable: false),
                    ServerPrivateKeyId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiAuthKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiAuthKeys_PublicKeys_ClientPublicKeyId",
                        column: x => x.ClientPublicKeyId,
                        principalTable: "PublicKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiAuthKeys_PublicKeys_ServerPublicKeyId",
                        column: x => x.ServerPublicKeyId,
                        principalTable: "PublicKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiAuthKeys_PrivateKeys_ServerPrivateKeyId",
                        column: x => x.ServerPrivateKeyId,
                        principalTable: "PrivateKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiAuthKeys_ClientPublicKeyId",
                table: "ApiAuthKeys",
                column: "ClientPublicKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiAuthKeys_ServerPublicKeyId",
                table: "ApiAuthKeys",
                column: "ServerPublicKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiAuthKeys_ServerPrivateKeyId",
                table: "ApiAuthKeys",
                column: "ServerPrivateKeyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiAuthKeys");
        }
    }
}
