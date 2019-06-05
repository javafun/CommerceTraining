using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.ViewModels;
using CommerceTraining.SupportingClasses;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    /// <summary>
    /// Bundle example 
    /// </summary>
    /// <remarks>
    /// More Apis can be found in the following links
    /// https://world.episerver.com/documentation/developer-guides/commerce/catalogs/catalog-content/Bundles-and-packages/
    /// </remarks>
    public class BundleController : CatalogControllerBase<ShirtBundle>
    {
        private readonly IRelationRepository _relationRepository;
        private readonly ICurrentMarket _currentMarket;

        public BundleController(
            IContentLoader contentLoader,
            UrlResolver urlResolver,
            AssetUrlResolver assetUrlResolver,
            ThumbnailUrlResolver thumbnailUrlResolver,
            IRelationRepository relationRepository,
            ICurrentMarket currentMarket) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
            _relationRepository = relationRepository;
            _currentMarket = currentMarket;
        }

        public ActionResult Index(ShirtBundle currentContent)
        {
            var vm = new BundleViewModel();

            vm.MainBody = currentContent.MainBody;
            vm.name = $"{currentContent.Name} - {currentContent.Code}";
            vm.image = GetDefaultAsset(currentContent);


            var childItems = _relationRepository.GetChildren<BundleEntry>(currentContent.ContentLink);

            // Alternative way to get the entry links from bundle 
            // https://world.episerver.com/documentation/developer-guides/commerce/catalogs/catalog-content/Bundles-and-packages/
            // currentContent.GetEntries();

            Money total = new Money(0,_currentMarket.GetCurrentMarket().DefaultCurrency);

            List<NameAndUrls> items = new List<NameAndUrls>(childItems.Count());

            foreach (var item in childItems)
            {

                var v = _contentLoader.Get<VariationContent>(item.Child);

                items.Add(new NameAndUrls
                {
                    name = v.Name,
                    url = _urlResolver.GetUrl(v),
                    imageUrl = _assetUrlResolver.GetAssetUrl(v),
                    imageThumbUrl = _thumbnailUrlResolver.GetThumbnailUrl(v, "Thumbnail")
                });

                total += v.GetDefaultPrice().UnitPrice;
            }

            vm.Items = items;
            vm.priceString = total.ToString("C");

            return View(vm);
        }
    }
}