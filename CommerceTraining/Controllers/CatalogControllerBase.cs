using CommerceTraining.SupportingClasses;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommerceTraining.Controllers
{
    public class CatalogControllerBase<T> :
        ContentController<T> where T : CatalogContentBase
    {
        protected readonly IContentLoader _contentLoader;
        protected readonly UrlResolver _urlResolver;
        protected readonly AssetUrlResolver _assetUrlResolver;
        protected readonly ThumbnailUrlResolver _thumbnailUrlResolver;

        public CatalogControllerBase(IContentLoader contentLoader,
            UrlResolver urlResolver,
            AssetUrlResolver assetUrlResolver,
            ThumbnailUrlResolver thumbnailUrlResolver)
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _assetUrlResolver = assetUrlResolver;
            _thumbnailUrlResolver = thumbnailUrlResolver;
        }

        protected string GetDefaultAsset(IAssetContainer assetContainer)
        {
            return _assetUrlResolver.GetAssetUrl(assetContainer);
        }

        protected string GetNamedAsset(IAssetContainer assetContainer, string groupName = "Thumbnail")
        {
            return _thumbnailUrlResolver.GetThumbnailUrl(assetContainer, groupName);
        }

        protected string GetUrl(ContentReference contentReference)
        {
            return _urlResolver.GetUrl(contentReference);
        }

        protected IEnumerable<NameAndUrls> GetNodes(ContentReference contentReference)
        {
            return 
                FilterForVisitor.Filter(
                    _contentLoader.GetChildren<NodeContent>(contentReference, new LoaderOptions()))
                .OfType<NodeContent>()
                .Select(x => new NameAndUrls
                {
                    name = x.Name,
                    url = GetUrl(x.ContentLink),
                    imageUrl = GetDefaultAsset(x),
                    imageThumbUrl = GetNamedAsset(x)
                });
        }

        protected IEnumerable<NameAndUrls> GetEntries(ContentReference contentReference)
        {
            return
                FilterForVisitor.Filter(
                    _contentLoader.GetChildren<EntryContentBase>(contentReference, new LoaderOptions()))
                    .OfType<EntryContentBase>()
                    .Select(_ => new NameAndUrls
                    {
                        name = _.Name,
                        url = GetUrl(_.ContentLink),
                        imageUrl = GetDefaultAsset(_),
                        imageThumbUrl = GetNamedAsset(_)
                    });
        }


    }
}