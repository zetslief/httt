using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gen.Migrations
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

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SourceTopicSourceId_SourceName",
                table: "Topics",
                columns: new[] { "SourceTopicSourceId", "SourceName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "TopicSources");
        }
    }
}
