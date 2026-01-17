using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.ServerRunner.Migrations
{
    /// <inheritdoc />
    public partial class UserProfileFkToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""Id"", ""Created"")
                SELECT p.""Id"", NOW() AT TIME ZONE 'UTC'
                FROM ""UserProfiles"" p
                WHERE NOT EXISTS (SELECT 1 FROM ""Users"" u WHERE u.""Id"" = p.""Id"");
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Users_Id",
                table: "UserProfiles",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Users_Id",
                table: "UserProfiles");
        }
    }
}
