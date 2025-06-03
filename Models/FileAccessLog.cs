namespace BlockShare.Models
{
    public class FileAccessLog
    {
        public int Id { get; set; }
        public int FileId { get; set; }
		public string Filename { get; set; }
		public string IpfsHash { get; set; }
		public string WalletAddress { get; set; }
        public DateTime AccessedAt { get; set; }
    }

}
