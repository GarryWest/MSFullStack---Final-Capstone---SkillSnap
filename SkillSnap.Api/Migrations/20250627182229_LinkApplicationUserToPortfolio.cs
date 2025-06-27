using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkApplicationUserToPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "PortfolioUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUsers_ApplicationUserId",
                table: "PortfolioUsers",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId",
                table: "PortfolioUsers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId",
                table: "PortfolioUsers");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUsers_ApplicationUserId",
                table: "PortfolioUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "PortfolioUsers");
        }
    }
}
