using System.Web.Mvc;
using CommerceTraining.Models.Catalog;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Web.Routing;

namespace CommerceTraining.Controllers
{
    public class ProductsController : CatalogControllerBase<ShirtProduct>
    {
        public ProductsController(IContentLoader contentLoader,
                                  UrlResolver urlResolver,
                                  AssetUrlResolver assetUrlResolver,
                                  ThumbnailUrlResolver thumbnailUrlResolver) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
        }

        public ActionResult Index(ShirtProduct currentContent)
        {
            return View(currentContent);
        }
    }
}