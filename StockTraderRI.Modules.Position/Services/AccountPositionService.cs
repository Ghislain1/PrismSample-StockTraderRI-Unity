using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using StockTraderRI.Infrastructure;
using StockTraderRI.Infrastructure.Interfaces;
using StockTraderRI.Infrastructure.Models;
 

namespace StockTraderRI.Modules.Position.Services
{
    public class AccountPositionService : IAccountPositionService
    {
        List<AccountPosition> _positions = new List<AccountPosition>();

        public AccountPositionService()
        {
            InitializePositions();
        }

        #region IAccountPositionService Members

        public event EventHandler<AccountPositionModelEventArgs> Updated = delegate { };

        public IList<AccountPosition> GetAccountPositions()
        {
            return _positions;
        }
        #endregion

        private void InitializePositions()
        {
            var  resourcesAccountPositions =XDocument.Load(DataLocationNames.AccountPositionsXml);
            using (var sr = new StringReader(resourcesAccountPositions.ToString()))
            {
                XDocument document = XDocument.Load(sr);
                _positions = document.Descendants("AccountPosition")
                    .Select(
                    x => new AccountPosition(x.Element("TickerSymbol").Value,
                                             decimal.Parse(x.Element("CostBasis").Value, CultureInfo.InvariantCulture),
                                             long.Parse(x.Element("Shares").Value, CultureInfo.InvariantCulture)))
                    .ToList();
            }
        }

    }
}
