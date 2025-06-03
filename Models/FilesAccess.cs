namespace BlockShare.Models
{
    public class FilesAccess
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string WalletAddress { get; set; }
        public FileMetadata File { get; set; }
    }
}
