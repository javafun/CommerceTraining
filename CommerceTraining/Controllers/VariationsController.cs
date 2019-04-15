using System.Web.Mvc;
using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Routing;

namespace CommerceTraining.Controllers
{
    public class VariationsController : CatalogControllerBase<ShirtVariation>
    {
        public VariationsController(
            IContentLoader contentLoader, UrlResolver urlResolver, AssetUrlResolver assetUrlResolver,
            ThumbnailUrlResolver thumbnailUrlResolver) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
        }

        public ActionResult Index(ShirtVariation currentContent)
        {
            ShirtVariationViewModel model = new ShirtVariationViewModel();
            model.MainBody = currentContent.MainBody;
            model.priceString = currentContent.GetDefaultPrice().UnitPrice.ToString("C");
            model.image = GetDefaultAsset(currentContent);
            model.CanBeMonogrammed = currentContent.CanBeMonogrammed;

            return View(model);
        }
    }
}