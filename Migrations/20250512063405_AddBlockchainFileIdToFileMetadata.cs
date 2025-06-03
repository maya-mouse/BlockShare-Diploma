using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockShare.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockchainFileIdToFileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlockchainFileIndex",
                table: "Files",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockchainFileIndex",
                table: "Files");
        }
    }
}
