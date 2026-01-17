using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Game.ServerRunner.Migrations
{
    /// <inheritdoc />
    public partial class Friedship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserA = table.Column<Guid>(type: "uuid", nullable: false, computedColumnSql: "CASE WHEN \"RequesterId\" < \"AddresseeId\" THEN \"RequesterId\" ELSE \"AddresseeId\" END", stored: true),
                    UserB = table.Column<Guid>(type: "uuid", nullable: false, computedColumnSql: "CASE WHEN \"RequesterId\" > \"AddresseeId\" THEN \"RequesterId\" ELSE \"AddresseeId\" END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                    table.CheckConstraint("CK_Friendship_NotSelf", "\"RequesterId\" <> \"AddresseeId\"");
                    table.ForeignKey(
                        name: "FK_Friendships_Users_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_AddresseeId_Status",
                table: "Friendships",
                columns: new[] { "AddresseeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_Status",
                table: "Friendships",
                columns: new[] { "RequesterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserA_UserB",
                table: "Friendships",
                columns: new[] { "UserA", "UserB" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friendships");
        }
    }
}
