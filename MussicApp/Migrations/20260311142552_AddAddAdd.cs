using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MussicApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAddAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioFileId",
                table: "Advertisements",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioFileId",
                table: "Advertisements");
        }
    }
}
