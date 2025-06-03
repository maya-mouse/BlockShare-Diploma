using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockShare.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldToBlockhainRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentHash",
                table: "BlockchainRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentHash",
                table: "BlockchainRecords");
        }
    }
}
