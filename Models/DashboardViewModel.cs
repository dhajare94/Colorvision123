namespace QRRewardPlatform.Models
{
    public class DashboardViewModel
    {
        public int TotalCampaigns { get; set; }
        public int TotalCodes { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal PendingPayouts { get; set; }
        public decimal CompletedPayouts { get; set; }
        public List<Redemption> RecentRedemptions { get; set; } = new();
        public List<Campaign> RecentCampaigns { get; set; } = new();
        public Dictionary<string, int> DailyRedemptions { get; set; } = new();
    }
}
