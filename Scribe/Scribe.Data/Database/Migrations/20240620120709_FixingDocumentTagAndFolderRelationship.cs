using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribe.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixingDocumentTagAndFolderRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Documents_Document_id",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Document_id",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Document_id",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "_id",
                table: "Tags",
                newName: "FolderId");

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "Tags",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                columns: new[] { "Name", "FolderId" });

            migrationBuilder.CreateTable(
                name: "DocumentTag",
                columns: table => new
                {
                    Document_id = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsName = table.Column<string>(type: "TEXT", nullable: false),
                    TagsFolderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTag", x => new { x.Document_id, x.TagsName, x.TagsFolderId });
                    table.ForeignKey(
                        name: "FK_DocumentTag_Documents_Document_id",
                        column: x => x.Document_id,
                        principalTable: "Documents",
                        principalColumn: "_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentTag_Tags_TagsName_TagsFolderId",
                        columns: x => new { x.TagsName, x.TagsFolderId },
                        principalTable: "Tags",
                        principalColumns: new[] { "Name", "FolderId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_FolderId",
                table: "Tags",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTag_TagsName_TagsFolderId",
                table: "DocumentTag",
                columns: new[] { "TagsName", "TagsFolderId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Folders_FolderId",
                table: "Tags",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Folders_FolderId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "DocumentTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_FolderId",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "FolderId",
                table: "Tags",
                newName: "_id");

            migrationBuilder.AlterColumn<int>(
                name: "_id",
                table: "Tags",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "Document_id",
                table: "Tags",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Document_id",
                table: "Tags",
                column: "Document_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Documents_Document_id",
                table: "Tags",
                column: "Document_id",
                principalTable: "Documents",
                principalColumn: "_id");
        }
    }
}
