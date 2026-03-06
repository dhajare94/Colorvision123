using Microsoft.AspNetCore.Mvc;
using QRRewardPlatform.Services;

namespace QRRewardPlatform.Controllers
{
    public class RedeemController : Controller
    {
        private readonly RedemptionService _redemptionService;
        private readonly CodeService _codeService;
        private readonly SettingsService _settingsService;

        public RedeemController(RedemptionService redemptionService, CodeService codeService, SettingsService settingsService)
        {
            _redemptionService = redemptionService;
            _codeService = codeService;
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                ViewBag.Status = "invalid";
                ViewBag.Message = "No code provided.";
                return View();
            }

            var codeEntry = await _codeService.GetByCodeAsync(code);
            if (codeEntry == null)
            {
                ViewBag.Status = "invalid";
                ViewBag.Message = "Invalid code. This code does not exist.";
                return View();
            }

            if (codeEntry.Status == "Redeemed")
            {
                ViewBag.Status = "redeemed";
                ViewBag.Message = "This code has already been redeemed.";
                return View();
            }

            var settings = await _settingsService.GetSettingsAsync();

            ViewBag.Status = "valid";
            ViewBag.Code = code;
            ViewBag.InstagramUrl = settings.InstagramUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(string code, string userName, string mobileNumber,
            string upiId)
        {
            var (success, message, reward) = await _redemptionService.RedeemCodeAsync(
                code, userName, mobileNumber, upiId);

            ViewBag.Status = success ? "success" : "error";
            ViewBag.Message = message;
            ViewBag.Reward = reward;
            ViewBag.Code = code;

            return View("Index");
        }
    }
}
