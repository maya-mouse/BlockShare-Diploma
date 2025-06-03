using System.ComponentModel.DataAnnotations;

namespace BlockShare.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int? BlockchainFileIndex { get; set; }
        public string IpfsHash { get; set; }
        public string OwnerId { get; set; }
        public string OwnerWalletAddress { get; set; }
        public string EncryptionKey { get; set; }
        public bool IsPublic { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
