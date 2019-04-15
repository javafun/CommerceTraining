using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using System.Collections.Generic;

namespace CommerceTraining.Models.ViewModels
{
    public class BlouseProductViewModel : CatalogViewModel<BlouseProduct, StartPage>
    {
        public BlouseProductViewModel(BlouseProduct currentContent,
            StartPage currentPage) : base(currentContent, currentPage)
        {
        }

        public IEnumerable<EntryContentBase> ProductVariations { get; set; }
        public ContentReference CampaignLink { get; set; }

    }
}
