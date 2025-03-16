using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopicSources",
                columns: table => new
                {
                    TopicSourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 10240, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSources", x => new { x.TopicSourceId, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    SourceTopicSourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => new { x.TopicId, x.Name });
                    table.ForeignKey(
                        name: "FK_Topics_TopicSources_SourceTopicSourceId_SourceName",
                        columns: x => new { x.SourceTopicSourceId, x.SourceName },
                        principalTable: "TopicSources",
                        principalColumns: new[] { "TopicSourceId", "Name" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.ArticleId);
                    table.ForeignKey(
                        name: "FK_Articles_Topics_TopicId_TopicName",
                        columns: x => new { x.TopicId, x.TopicName },
                        principalTable: "Topics",
                        principalColumns: new[] { "TopicId", "Name" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_Sections_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_TopicId_TopicName",
                table: "Articles",
                columns: new[] { "TopicId", "TopicName" });

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ArticleId",
                table: "Sections",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SourceTopicSourceId_SourceName",
                table: "Topics",
                columns: new[] { "SourceTopicSourceId", "SourceName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "TopicSources");
        }
    }
}
