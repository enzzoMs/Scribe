using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribe.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class ModifyingFolderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NavigationPosition",
                table: "Folders",
                newName: "NavigationIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NavigationIndex",
                table: "Folders",
                newName: "NavigationPosition");
        }
    }
}
