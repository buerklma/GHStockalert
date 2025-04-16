using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using StockAlerter.Models;
using StockAlerter.Services;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StockAlerter
{
    class Program
    {
        // Using null! tells the compiler we'll initialize these fields before use
        private static IConfiguration _configuration = null!;
        private static IServiceProvider _serviceProvider = null!;

        static async Task Main(string[] args)
        {
            // Set up Configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Set up Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("Stock Alerter Application Started");
            Console.WriteLine("=================================");
            Console.WriteLine("Monitoring stocks for overvaluation and target price alerts");
            Console.WriteLine();

            // Run the monitoring process
            await MonitorStocksAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_configuration);
            services.AddHttpClient();
            services.AddTransient<IStockService, StockService>();
            services.AddTransient<IEmailService, EmailService>();
        }

        private static async Task MonitorStocksAsync()
        {
            var stockService = _serviceProvider.GetRequiredService<IStockService>();
            var emailService = _serviceProvider.GetRequiredService<IEmailService>();
            var recipientEmail = _configuration["AlertSettings:RecipientEmail"] ?? 
                throw new InvalidOperationException("Recipient email configuration is missing");
            var checkInterval = int.Parse(_configuration["AlertSettings:CheckIntervalMinutes"] ?? "30");

            // In a real application, this would run continuously
            // For this demo, we'll run it once
            
            Console.WriteLine("Fetching stock data...");
            var stocks = await stockService.GetWatchlistStocksAsync();
            
            foreach (var stock in stocks)
            {
                Console.WriteLine($"Analyzing {stock.Symbol} ({stock.Name}):");
                Console.WriteLine($"  Current Price: ${stock.CurrentPrice:F2}");
                Console.WriteLine($"  Historical Avg: ${stock.HistoricalAverage:F2}");
                Console.WriteLine($"  P/E Ratio: {stock.PriceToEarningsRatio:F2}");
                Console.WriteLine($"  Target Price: ${stock.TargetPrice:F2}");
                
                // Check for overvalued condition
                if (stock.IsOvervalued())
                {
                    Console.WriteLine($"  ALERT: {stock.Symbol} appears to be overvalued!");
                    await SendOvervaluedAlertAsync(emailService, stock, recipientEmail);
                }
                
                // Check for price target reached
                if (stock.CurrentPrice >= stock.TargetPrice)
                {
                    Console.WriteLine($"  ALERT: {stock.Symbol} has reached the target price!");
                    await SendPriceTargetAlertAsync(emailService, stock, recipientEmail);
                }
                
                Console.WriteLine();
            }
            
            Console.WriteLine($"Stock check complete. Next check in {checkInterval} minutes.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            
            // In a real application, you would implement a timer or background service
            // to continuously monitor stocks at regular intervals
            // Example:
            //
            // while (true)
            // {
            //     await CheckStocksAsync();
            //     await Task.Delay(TimeSpan.FromMinutes(checkInterval));
            // }
        }

        private static async Task SendOvervaluedAlertAsync(IEmailService emailService, Stock stock, string recipient)
        {
            string subject = $"ALERT: {stock.Symbol} appears to be overvalued";
            string body = $@"
                <h2>Stock Overvaluation Alert</h2>
                <p>Our analysis indicates that <strong>{stock.Symbol} ({stock.Name})</strong> may be overvalued.</p>
                <ul>
                    <li>Current Price: ${stock.CurrentPrice:F2}</li>
                    <li>Historical Average: ${stock.HistoricalAverage:F2}</li>
                    <li>P/E Ratio: {stock.PriceToEarningsRatio:F2}</li>
                </ul>
                <p>This alert is based on the current price being significantly above the historical average 
                and a high P/E ratio.</p>
                <p>Please consider reviewing your position in this stock.</p>
            ";
            
            try
            {
                await emailService.SendAlertAsync(recipient, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send overvaluation alert: {ex.Message}");
            }
        }

        private static async Task SendPriceTargetAlertAsync(IEmailService emailService, Stock stock, string recipient)
        {
            string subject = $"ALERT: {stock.Symbol} has reached target price";
            string body = $@"
                <h2>Stock Price Target Alert</h2>
                <p><strong>{stock.Symbol} ({stock.Name})</strong> has reached or exceeded the target price.</p>
                <ul>
                    <li>Current Price: ${stock.CurrentPrice:F2}</li>
                    <li>Target Price: ${stock.TargetPrice:F2}</li>
                </ul>
                <p>This may be a good time to review your investment strategy for this stock.</p>
            ";
            
            try
            {
                await emailService.SendAlertAsync(recipient, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send price target alert: {ex.Message}");
            }
        }
    }
}
