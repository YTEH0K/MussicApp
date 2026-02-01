using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MussicApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackGenresDbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackGenre_Genre_GenreId",
                table: "TrackGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackGenre_Tracks_TrackId",
                table: "TrackGenre");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackGenre",
                table: "TrackGenre");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Genre",
                table: "Genre");

            migrationBuilder.RenameTable(
                name: "TrackGenre",
                newName: "TrackGenres");

            migrationBuilder.RenameTable(
                name: "Genre",
                newName: "Genres");

            migrationBuilder.RenameIndex(
                name: "IX_TrackGenre_GenreId",
                table: "TrackGenres",
                newName: "IX_TrackGenres_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_Genre_Slug",
                table: "Genres",
                newName: "IX_Genres_Slug");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackGenres",
                table: "TrackGenres",
                columns: new[] { "TrackId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Genres",
                table: "Genres",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserListeningHistories",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlayedDuration = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserListeningHistories", x => new { x.UserId, x.TrackId, x.PlayedAt });
                    table.ForeignKey(
                        name: "FK_UserListeningHistories_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserListeningHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserListeningHistories_TrackId",
                table: "UserListeningHistories",
                column: "TrackId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackGenres_Genres_GenreId",
                table: "TrackGenres",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackGenres_Tracks_TrackId",
                table: "TrackGenres",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackGenres_Genres_GenreId",
                table: "TrackGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackGenres_Tracks_TrackId",
                table: "TrackGenres");

            migrationBuilder.DropTable(
                name: "UserListeningHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackGenres",
                table: "TrackGenres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Genres",
                table: "Genres");

            migrationBuilder.RenameTable(
                name: "TrackGenres",
                newName: "TrackGenre");

            migrationBuilder.RenameTable(
                name: "Genres",
                newName: "Genre");

            migrationBuilder.RenameIndex(
                name: "IX_TrackGenres_GenreId",
                table: "TrackGenre",
                newName: "IX_TrackGenre_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_Genres_Slug",
                table: "Genre",
                newName: "IX_Genre_Slug");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackGenre",
                table: "TrackGenre",
                columns: new[] { "TrackId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Genre",
                table: "Genre",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackGenre_Genre_GenreId",
                table: "TrackGenre",
                column: "GenreId",
                principalTable: "Genre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackGenre_Tracks_TrackId",
                table: "TrackGenre",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
