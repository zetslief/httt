using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class ArticleIndexes_CreatedOn_ViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Articles_CreatedOn",
                table: "Articles",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ViewCount",
                table: "Articles",
                column: "ViewCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Articles_CreatedOn",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_ViewCount",
                table: "Articles");
        }
    }
}
