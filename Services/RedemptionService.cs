using QRRewardPlatform.Models;

namespace QRRewardPlatform.Services
{
    public class RedemptionService
    {
        private readonly FirebaseService _firebase;
        private readonly CodeService _codeService;
        private readonly CampaignService _campaignService;
        private readonly RewardSlabService _slabService;
        private const string Node = "redemptions";

        public RedemptionService(FirebaseService firebase, CodeService codeService,
            CampaignService campaignService, RewardSlabService slabService)
        {
            _firebase = firebase;
            _codeService = codeService;
            _campaignService = campaignService;
            _slabService = slabService;
        }

        public async Task<List<Redemption>> GetAllAsync()
        {
            var items = await _firebase.GetAllAsync<Redemption>(Node);
            return items.Select(i => { i.Value.Id = i.Key; return i.Value; }).ToList();
        }

        public async Task<List<Redemption>> GetFilteredAsync(string? campaignId, string? status)
        {
            var all = await GetAllAsync();
            if (!string.IsNullOrEmpty(campaignId))
                all = all.Where(r => r.CampaignId == campaignId).ToList();
            if (!string.IsNullOrEmpty(status))
                all = all.Where(r => r.PayoutStatus == status).ToList();
            return all;
        }

        public async Task<List<Redemption>> GetPendingAsync()
        {
            var all = await GetAllAsync();
            return all.Where(r => r.PayoutStatus == "Pending").ToList();
        }

        public async Task<(bool success, string message, decimal reward)> RedeemCodeAsync(
            string code, string userName, string mobileNumber, string upiId)
        {
            // Find the code
            var codeEntry = await _codeService.GetByCodeAsync(code);
            if (codeEntry == null)
                return (false, "Invalid code. This code does not exist.", 0);

            if (codeEntry.Status == "Redeemed")
                return (false, "This code has already been redeemed.", 0);

            // Get campaign
            var campaign = await _campaignService.GetByIdAsync(codeEntry.CampaignId);
            if (campaign == null)
                return (false, "Campaign not found for this code.", 0);

            if (campaign.Status != "Active")
                return (false, "This campaign is no longer active.", 0);

            // Get reward slab
            var slab = await _slabService.GetByIdAsync(campaign.RewardSlabId);
            if (slab == null)
                return (false, "Reward configuration not found.", 0);

            // Calculate reward
            decimal rewardAmount = _slabService.CalculateReward(slab);

            // Create redemption
            var redemption = new Redemption
            {
                CodeId = codeEntry.Id,
                CampaignId = codeEntry.CampaignId,
                UserName = userName,
                MobileNumber = mobileNumber,
                UpiId = upiId,
                RewardAmount = rewardAmount,
                RedemptionDate = DateTime.UtcNow.ToString("o"),
                PayoutStatus = "Pending"
            };

            var redemptionId = await _firebase.PushAsync(Node, redemption);

            // Mark code as redeemed
            await _codeService.MarkRedeemedAsync(codeEntry.Id, redemptionId);

            return (true, $"Congratulations! Your reward of ₹{rewardAmount:F2} has been recorded and will be credited within 24 hours.", rewardAmount);
        }

        public async Task MarkPaidAsync(string id, string batchId)
        {
            var redemption = await _firebase.GetByIdAsync<Redemption>(Node, id);
            if (redemption != null)
            {
                redemption.Id = id;
                redemption.PayoutStatus = "Paid";
                redemption.PayoutBatchId = batchId;
                await _firebase.SetAsync(Node, id, redemption);
            }
        }
    }
}
