using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommerceTraining.Models.ViewModels
{
    public class ShirtVariationViewModel
    {
        public string priceString { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public bool CanBeMonogrammed { get; set; }
        public XhtmlString MainBody { get; set; }

        public string PromoString { get; set; }
        public decimal DiscountPrice { get; set; }
    }
}
