using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockShare.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerAddressToFileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerWalletAddress",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerWalletAddress",
                table: "Files");
        }
    }
}
