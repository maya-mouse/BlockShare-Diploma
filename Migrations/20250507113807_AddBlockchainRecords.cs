using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockShare.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockchainRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockchainRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpfsHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlockHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploaderAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainRecords");
        }
    }
}
