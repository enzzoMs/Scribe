using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribe.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixingDocumentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTag_Documents_Document_id",
                table: "DocumentTag");

            migrationBuilder.RenameColumn(
                name: "Document_id",
                table: "DocumentTag",
                newName: "DocumentId");

            migrationBuilder.RenameColumn(
                name: "_id",
                table: "Documents",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTag_Documents_DocumentId",
                table: "DocumentTag",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTag_Documents_DocumentId",
                table: "DocumentTag");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "DocumentTag",
                newName: "Document_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Documents",
                newName: "_id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTag_Documents_Document_id",
                table: "DocumentTag",
                column: "Document_id",
                principalTable: "Documents",
                principalColumn: "_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
