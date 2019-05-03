using CommerceTraining.Models.Catalog;
using Mediachase.Commerce;
using System.Collections.Generic;

namespace CommerceTraining.Models.ViewModels
{
    public class DemoMarketsViewModel
    {
        public IMarket SelectedMarket { get; set; }
        public IEnumerable<IMarket> MarketList { get; set; }
        public ShirtVariation Shirt { get; set; }

    }
}
