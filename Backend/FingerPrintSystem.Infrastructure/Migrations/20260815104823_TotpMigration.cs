using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FingerPrintSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TotpMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TotpEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TotpSecret",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotpEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotpSecret",
                table: "AspNetUsers");
        }
    }
}
