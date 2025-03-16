using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gen.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleToSectionsAndRequiredAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Articles_ArticleId",
                table: "Sections");

            migrationBuilder.AlterColumn<int>(
                name: "ArticleId",
                table: "Sections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Articles_ArticleId",
                table: "Sections",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Articles_ArticleId",
                table: "Sections");

            migrationBuilder.AlterColumn<int>(
                name: "ArticleId",
                table: "Sections",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Articles_ArticleId",
                table: "Sections",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "ArticleId");
        }
    }
}
