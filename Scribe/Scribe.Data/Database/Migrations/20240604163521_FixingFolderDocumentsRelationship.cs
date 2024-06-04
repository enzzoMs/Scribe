using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribe.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixingFolderDocumentsRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Documents_DocumentId",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "Tags",
                newName: "Document_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Tags",
                newName: "_id");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_DocumentId",
                table: "Tags",
                newName: "IX_Tags_Document_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Documents",
                newName: "_id");

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "Documents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Documents_Document_id",
                table: "Tags",
                column: "Document_id",
                principalTable: "Documents",
                principalColumn: "_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Documents_Document_id",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "Document_id",
                table: "Tags",
                newName: "DocumentId");

            migrationBuilder.RenameColumn(
                name: "_id",
                table: "Tags",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_Document_id",
                table: "Tags",
                newName: "IX_Tags_DocumentId");

            migrationBuilder.RenameColumn(
                name: "_id",
                table: "Documents",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "Documents",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Documents_DocumentId",
                table: "Tags",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }
    }
}
