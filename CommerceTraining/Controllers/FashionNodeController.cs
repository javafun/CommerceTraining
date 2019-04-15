using System.Web.Mvc;
using CommerceTraining.Models.Catalog;
using CommerceTraining.SupportingClasses;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Web.Routing;

namespace CommerceTraining.Controllers
{
    public class FashionNodeController : CatalogControllerBase<FashionNode>
    {
        public FashionNodeController(IContentLoader contentLoader,
                                     UrlResolver urlResolver,
                                     AssetUrlResolver assetUrlResolver,
                                     ThumbnailUrlResolver thumbnailUrlResolver) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
        }

        public ActionResult Index(FashionNode currentContent)
        {
            var model = new NodeEntryCombo
            {
                nodes = GetNodes(currentContent.ContentLink),
                entries = GetEntries(currentContent.ContentLink)
            };
            return View(model);
        }
    }
}