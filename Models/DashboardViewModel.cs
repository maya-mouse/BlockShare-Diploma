namespace BlockShare.Models
{
    public class DashboardViewModel
    {
        public List<FileMetadata> RecentFiles { get; set; }
        public int TotalFiles { get; set; }
        public int PublicFiles { get; set; }
        public int AccessibleFiles { get; set; }

        public List<string> WalletsIAccessed { get; set; } = new();

        public List<string> UsernamesIAccessed { get; set; } = new();
        public List<string> WalletsWithAccessToMine { get; set; } = new();

        public List<string> UsernamesWithAccessToMine { get; set; } = new();
    }
}
