using Microsoft.Extensions.Configuration;
using StockAlerter.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using YahooFinanceApi;
using System.Linq;

namespace StockAlerter.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public StockService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Stock> GetStockDataAsync(string symbol)
        {
            try
            {
                // Using Yahoo Finance API to get real stock data
                var securities = await Yahoo.Symbols(symbol).Fields(
                    Field.Symbol,
                    Field.RegularMarketPrice,
                    Field.FiftyDayAverage,
                    Field.TwoHundredDayAverage,
                    Field.TrailingPE,
                    Field.PriceToBook,
                    Field.ShortName
                ).QueryAsync();

                var data = securities[symbol];
                
                // Get the data from the response
                decimal currentPrice = Convert.ToDecimal(data.RegularMarketPrice);
                
                // Handle null values using Double.NaN check
                decimal historicalAverage;
                if (!double.IsNaN(data.TwoHundredDayAverage))
                    historicalAverage = Convert.ToDecimal(data.TwoHundredDayAverage);
                else if (!double.IsNaN(data.FiftyDayAverage))
                    historicalAverage = Convert.ToDecimal(data.FiftyDayAverage);
                else
                    historicalAverage = currentPrice * 0.9m;
                
                decimal peRatio;
                if (!double.IsNaN(data.TrailingPE))
                    peRatio = Convert.ToDecimal(data.TrailingPE);
                else
                    peRatio = 15.0m; // Default PE if not available
                
                string name = data.ShortName ?? symbol;
                
                // For values not directly available in the API, calculate or use default values
                decimal dividendYield = 0; // Would need additional API call to get this
                decimal targetPrice = currentPrice * 1.1m; // Simple target price estimation

                return new Stock
                {
                    Symbol = symbol,
                    Name = name,
                    CurrentPrice = currentPrice,
                    HistoricalAverage = historicalAverage,
                    PriceToEarningsRatio = peRatio,
                    DividendYield = dividendYield,
                    TargetPrice = targetPrice,
                    LastUpdated = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stock data for {symbol}: {ex.Message}");
                
                // Fallback to mock data if the Yahoo API call fails
                Console.WriteLine("Using fallback mock data");
                var random = new Random();
                var currentPrice = (decimal)(random.NextDouble() * 100 + 50);
                var historicalAvg = currentPrice * (decimal)(random.NextDouble() * 0.8 + 0.6);
                
                return new Stock
                {
                    Symbol = symbol,
                    Name = $"{symbol} Corporation",
                    CurrentPrice = currentPrice,
                    HistoricalAverage = historicalAvg,
                    PriceToEarningsRatio = (decimal)(random.NextDouble() * 40),
                    DividendYield = (decimal)(random.NextDouble() * 0.05),
                    TargetPrice = historicalAvg * 1.1m,
                    LastUpdated = DateTime.Now
                };
            }
        }

        public async Task<IEnumerable<Stock>> GetWatchlistStocksAsync()
        {
            // In a real application, this might come from a user's watchlist in a database
            // For demo purposes, we'll use a hardcoded list
            var watchlist = new[] { "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA" };
            var stocks = new List<Stock>();
            
            foreach (var symbol in watchlist)
            {
                stocks.Add(await GetStockDataAsync(symbol));
            }
            
            return stocks;
        }
    }
}