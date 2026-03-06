using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRRewardPlatform.Models;
using QRRewardPlatform.Services;

namespace QRRewardPlatform.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly CampaignService _campaignService;
        private readonly CodeService _codeService;
        private readonly RedemptionService _redemptionService;
        private readonly PayoutService _payoutService;

        public DashboardController(CampaignService campaignService, CodeService codeService,
            RedemptionService redemptionService, PayoutService payoutService)
        {
            _campaignService = campaignService;
            _codeService = codeService;
            _redemptionService = redemptionService;
            _payoutService = payoutService;
        }

        public async Task<IActionResult> Index()
        {
            var campaigns = await _campaignService.GetAllAsync();
            var codes = await _codeService.GetAllAsync();
            var redemptions = await _redemptionService.GetAllAsync();
            var batches = await _payoutService.GetAllAsync();

            var model = new DashboardViewModel
            {
                TotalCampaigns = campaigns.Count,
                TotalCodes = codes.Count,
                TotalRedemptions = redemptions.Count,
                PendingPayouts = batches.Where(b => b.Status == "Pending").Sum(b => b.TotalAmount),
                CompletedPayouts = batches.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount),
                RecentRedemptions = redemptions.OrderByDescending(r => r.RedemptionDate).Take(10).ToList(),
                RecentCampaigns = campaigns.OrderByDescending(c => c.CreatedAt).Take(10).ToList()
            };

            // Daily redemptions for last 30 days
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd"))
                .Reverse().ToList();

            foreach (var day in last30Days)
            {
                model.DailyRedemptions[day] = redemptions
                    .Count(r => r.RedemptionDate.StartsWith(day));
            }

            return View(model);
        }
    }
}
