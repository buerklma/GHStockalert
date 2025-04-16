using StockAlerter.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockAlerter.Services
{
    public interface IStockService
    {
        Task<Stock> GetStockDataAsync(string symbol);
        Task<IEnumerable<Stock>> GetWatchlistStocksAsync();
    }
}