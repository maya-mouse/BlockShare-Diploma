namespace BlockShare.Models
{
    public class AccessUpdateViewModel
    {
        public int FileId { get; set; }
        public bool IsPublic { get; set; }
        public List<string>? AllowedWallets { get; set; }
    }
}
