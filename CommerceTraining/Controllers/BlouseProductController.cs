using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Catalog;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class BlouseProductController : CatalogControllerBase<BlouseProduct>
    {
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentRepository _contentRepository;
        private readonly IRelationRepository _relationRepository;

        public BlouseProductController(
            IContentLoader contentLoader,
            UrlResolver urlResolver,
            AssetUrlResolver assetUrlResolver,
            ThumbnailUrlResolver thumbnailUrlResolver,
            ReferenceConverter referenceConverter,
            IContentRepository contentRepository,
            IRelationRepository relationRepository) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
            _referenceConverter = referenceConverter;
            _contentRepository = contentRepository;
            _relationRepository = relationRepository;
        }

        public ActionResult Index(BlouseProduct currentContent, StartPage currentPage)
        {
            var model = new BlouseProductViewModel(currentContent, currentPage);
            model.ProductVariations = currentContent.GetVariants().Select(_ => _contentLoader.Get<EntryContentBase>(_));
            model.CampaignLink = currentPage.campaignLink;

            //CreateWithCode();
            return View(model);
        }

        public void CreateWithCode()
        {
            string nodeName = "myNode";
            string productName = "myProduct";
            string skuName = "mySku";

            ContentReference linkToParentNode = _referenceConverter.GetContentLink("Women_1");

            // Create node
            var newNode = _contentRepository.GetDefault<FashionNode>(linkToParentNode, new CultureInfo("en"));
            newNode.Code = nodeName;
            newNode.SeoUri = nodeName;
            newNode.Name = nodeName;
            newNode.DisplayName = nodeName;

            var newNodeRef = _contentRepository.Save(newNode, SaveAction.Publish, AccessLevel.NoAccess);


            // Create product
            var newProduct = _contentRepository.GetDefault<BlouseProduct>(newNodeRef, new CultureInfo("en"));

            // Set product properties
            newProduct.Code = productName;
            newProduct.SeoUri = productName;
            newProduct.Name = productName;
            newProduct.DisplayName = productName;

            newProduct.SeoInformation.Title = "SEO Title";
            newProduct.SeoInformation.Keywords = "Some keywords";
            newProduct.SeoInformation.Description = "A nice one";
            newProduct.MainBody = new XhtmlString("This new product is great");

            // Persist the Product
            ContentReference newProductReference = _contentRepository.Save
                (newProduct, SaveAction.Publish, AccessLevel.NoAccess);

            // Create SKU
            var newSku = _contentRepository.GetDefault<ShirtVariation>(newNodeRef, new CultureInfo("en"));

            newSku.Code = skuName;
            newSku.SeoUri = skuName;
            newSku.Name = skuName;
            newSku.DisplayName = skuName;

            ContentReference newSkuReference = _contentRepository.Save(newSku,
                SaveAction.Publish, AccessLevel.NoAccess);

            ProductVariation productVariation = new ProductVariation
            {
                Parent = newProductReference,
                Child = newSkuReference,
                SortOrder = 100
            };

            _relationRepository.UpdateRelation(productVariation);
        }
    }
}
