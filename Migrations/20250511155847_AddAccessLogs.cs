using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockShare.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileAccessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpfsHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WalletAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAccessLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileAccessLogs");
        }
    }
}
