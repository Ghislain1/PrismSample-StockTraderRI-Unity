using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTraderRI.Infrastructure.Models;

namespace StockTraderRI.Infrastructure.Interfaces
{
    public interface IAccountPositionService
    {
        event EventHandler<AccountPositionModelEventArgs> Updated;
        IList<AccountPosition> GetAccountPositions();
        Task<IList<AccountPosition>> GetAccountPositionsAsync();
    }
}
