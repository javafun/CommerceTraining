using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommerceTraining.Infrastructure.Pricing;
using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.Security;

namespace CommerceTraining.Controllers
{
    public class VariationsController : CatalogControllerBase<ShirtVariation>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPromotionEngine _promotionEngine;
        private readonly MyPriceCalculator _myPriceCalculator;

        public VariationsController(
            IContentLoader contentLoader,
            UrlResolver urlResolver,
            AssetUrlResolver assetUrlResolver,
            ThumbnailUrlResolver thumbnailUrlResolver,
            IOrderRepository orderRepository,
            IWarehouseRepository warehouseRepository,
            IInventoryService inventoryService,
            IOrderGroupFactory orderGroupFactory,
            ILineItemValidator lineItemValidator,
            IPromotionEngine promotionEngine,
            MyPriceCalculator myPriceCalculator) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
            _orderRepository = orderRepository;
            _warehouseRepository = warehouseRepository;
            _inventoryService = inventoryService;
            _orderGroupFactory = orderGroupFactory;
            _lineItemValidator = lineItemValidator;
            _promotionEngine = promotionEngine;
            _myPriceCalculator = myPriceCalculator;
        }

        public ActionResult Index(ShirtVariation currentContent)
        {
            decimal savedMoney = 0;
            string rewardDescription = string.Empty;

            List<RewardDescription> rewardDescriptions = _promotionEngine.Evaluate(currentContent.ContentLink).ToList();

            if(rewardDescriptions.Count == 0) // No promos
            {
                var d = new RewardDescription(FulfillmentStatus.NotFulfilled, null, null, 0, 0, RewardType.None, "No promo");
                rewardDescriptions.Add(d);
                rewardDescription = rewardDescriptions.FirstOrDefault()?.Description;
            }
            else
            {
                foreach (var item in rewardDescriptions)
                {
                    rewardDescription += item.Description;
                }
            }

            IPriceValue salePrice = _myPriceCalculator.GetSalePrice(currentContent, 1);

            // previous way
            if (rewardDescriptions.Count() >= 1)
            {
                savedMoney = rewardDescriptions.First().Percentage * salePrice.UnitPrice.Amount / 100;
                //rewardDescription = descr.First().Description;
                Session["SavedMoney"] = savedMoney;
            }
            else
            {
                savedMoney = 0;
                //rewardDescription = "No discount";
            }


            ShirtVariationViewModel model = new ShirtVariationViewModel();
            model.MainBody = currentContent.MainBody;
            model.priceString = currentContent.GetDefaultPrice().UnitPrice.ToString("C");
            model.image = GetDefaultAsset(currentContent);
            model.CanBeMonogrammed = currentContent.CanBeMonogrammed;
            model.PromoString = rewardDescription;
            model.DiscountPrice = savedMoney;
            return View(model);
        }

        public ActionResult AddToCart(ShirtVariation currentContent, decimal Quantity, string Monogram)
        {
            // ToDo: (lab D1) add a LineItem to the Cart
            var cart = _orderRepository.LoadOrCreateCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");


            IWarehouse wh = _warehouseRepository.GetDefaultWarehouse();
            InventoryRecord rec = _inventoryService.Get(currentContent.Code, wh.Code);

            // Retrieve the shirt variation is already in the cart (if cart was loaded from the db)
            var lineItem = cart.GetAllLineItems().FirstOrDefault(x => x.Code == currentContent.Code);

            if(lineItem == null)
            {
                lineItem = _orderGroupFactory.CreateLineItem(currentContent.Code, cart);
                lineItem.Quantity = Quantity;
                cart.AddLineItem(lineItem);
            }
            else
            {
                lineItem.Quantity += Quantity;
            }

            // To hold any validation issues
            var validationIssues = new Dictionary<ILineItem, ValidationIssue>();

            var validLineItem = _lineItemValidator.Validate(lineItem, cart.MarketId, (item, issue) =>
            {
                validationIssues.Add(item, issue);
            });

            if (validLineItem)
            {
                lineItem.Properties["Monogram"] = Monogram;
                _orderRepository.Save(cart);
            }

            // if we want to redirect
            ContentReference cartRef = _contentLoader.Get<StartPage>(ContentReference.StartPage).Settings.cartPage;
            CartPage cartPage = _contentLoader.Get<CartPage>(cartRef);
            var name = cartPage.Name;
            var lang = ContentLanguage.PreferredCulture;
            string passingValue = cart.Name;

            // go to the cart page, if needed
            return RedirectToAction("Index", lang + "/" + name, new { passedAlong = passingValue });
        }

        public void AddToWishList(ShirtVariation currentContent)
        {
            ICart myWishList = _orderRepository.LoadOrCreateCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), "WishList");
            ILineItem lineItem = _orderGroupFactory.CreateLineItem(currentContent.Code, myWishList);
            myWishList.AddLineItem(lineItem);
            _orderRepository.Save(myWishList);
        }
    }
}