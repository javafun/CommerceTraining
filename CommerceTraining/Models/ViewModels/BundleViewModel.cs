using System.Collections.Generic;
using CommerceTraining.SupportingClasses;
using EPiServer.Core;

namespace CommerceTraining.Models.ViewModels
{
    public class BundleViewModel
    {
        public string priceString { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public string url { get; set; }

        public XhtmlString MainBody { get; set; }

        public IEnumerable<NameAndUrls> Items { get; set; }
    }
}