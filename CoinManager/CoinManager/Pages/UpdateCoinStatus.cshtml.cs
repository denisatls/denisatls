using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoinManager.Data;
using CoinManager.Models;

namespace CoinManager.Pages
{
    public class UpdateCoinStatus : PageModel
    {
        private readonly Procedures _procedures;
        private readonly ILogger<UpdateCoinStatus> _logger;

        public UpdateCoinStatus(Procedures procedures, ILogger<UpdateCoinStatus> logger)
        {
            _procedures = procedures;
            _logger = logger;
            Exchanges = new List<Exchange>();
            Coins = new List<TopCoin>();
        }

        public List<Exchange> Exchanges { get; private set; }
        public List<TopCoin> Coins { get; private set; }
        [TempData]
        public string StatusMessage { get; set; } // Use TempData attribute

        public async Task OnGetAsync(int selectedExchangeId = 0)
        {
            var exchanges = await _procedures.GetAllExchangesAsync();
            Exchanges = exchanges != null ? new List<Exchange>(exchanges) : new List<Exchange>();

            if (selectedExchangeId > 0)
            {
                var coins = await _procedures.GetTopCoinsByExchangeAsync(selectedExchangeId);
                Coins = coins != null ? new List<TopCoin>(coins) : new List<TopCoin>();
            }
            else
            {
                Coins = new List<TopCoin>();
            }
        }

        public async Task<JsonResult> OnGetCoinsAsync(int exchangeId, string statusFilter = "all")
        {
            IEnumerable<TopCoin> coins;
            switch (statusFilter)
            {
                case "active":
                    coins = await _procedures.GetTopCoinsByExchangeAndStatusAsync(exchangeId, true);
                    break;
                case "disabled":
                    coins = await _procedures.GetTopCoinsByExchangeAndStatusAsync(exchangeId, false);
                    break;
                default:
                    coins = await _procedures.GetTopCoinsByExchangeAsync(exchangeId);
                    break;
            }
            return new JsonResult(coins);
        }

        public async Task<IActionResult> OnPostUpdateCoinStatusAsync(int exchangeId, string coinName, bool isActive)
        {
            _logger.LogInformation($"Updating coin status: ExchangeId={exchangeId}, CoinName={coinName}, IsActive={isActive}");
            bool updateResult = await _procedures.UpdateCoinActiveStatusAsync(exchangeId, coinName, isActive);

            if (updateResult)
            {
                 await _procedures.InsertChangeAsync();
                TempData["StatusMessage"] = "Coin status updated successfully.";
                
                _logger.LogInformation("  await _procedures.InsertChangeAsync(); executed ");
            }
            else
            {
                TempData["StatusMessage"] = "Error: Unable to update the coin status.";
            }

            return RedirectToPage(new { selectedExchangeId = exchangeId });
        }
        
    }
}
