using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class RequestsAddResponseStatusCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTimeOffset",
                table: "Requests",
                newName: "RequestedOn");

            migrationBuilder.AddColumn<int>(
                name: "ResponseStatusCode",
                table: "Requests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseStatusCode",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "RequestedOn",
                table: "Requests",
                newName: "DateTimeOffset");
        }
    }
}
