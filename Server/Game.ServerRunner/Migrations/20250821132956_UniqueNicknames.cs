using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.ServerRunner.Migrations
{
    /// <inheritdoc />
    public partial class UniqueNicknames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "UserProfiles",
                type: "citext",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_NickName",
                table: "UserProfiles",
                column: "NickName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_NickName",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "UserProfiles");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");
        }
    }
}
