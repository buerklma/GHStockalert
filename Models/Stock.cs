using System;

namespace StockAlerter.Models
{
    public class Stock
    {
        public required string Symbol { get; set; }
        public required string Name { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal HistoricalAverage { get; set; }
        public decimal PriceToEarningsRatio { get; set; }
        public decimal DividendYield { get; set; }
        public decimal TargetPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public bool IsOvervalued()
        {
            // Simple overvaluation logic - can be enhanced with more sophisticated models
            // A stock might be overvalued if:
            // 1. Current price is significantly higher than historical average
            // 2. P/E ratio is above industry average (assuming high P/E ratio implies overvaluation)
            // 3. Dividend yield is too low compared to market average
            
            // For this example, we'll use a simple threshold
            bool priceOvervalued = CurrentPrice > HistoricalAverage * 1.5m;
            bool peRatioHigh = PriceToEarningsRatio > 25; // Industry average varies, using 25 for example
            
            return priceOvervalued && peRatioHigh;
        }
    }
}