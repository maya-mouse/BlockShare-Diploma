using BlockShare.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BlockShare.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<FileMetadata> Files { get; set; }
        public DbSet<FilesAccess> AccessEntries { get; set; }
        public DbSet<BlockchainRecord> BlockchainRecords { get; set; }

        public DbSet<FileAccessLog> FileAccessLogs { get; set; }
    }
}
