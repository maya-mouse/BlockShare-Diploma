namespace BlockShare.Models
{
    public class BlockchainRecord
    {
        public int Id { get; set; }
        public string IpfsHash { get; set; }
        public string BlockHash { get; set; }
        public long BlockNumber { get; set; }
        public string TransactionHash { get; set; }
        public DateTime Timestamp { get; set; }
        public string UploaderAddress { get; set; }

        public string ParentHash { get; set; }
    }

}
