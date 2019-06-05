using CommerceTraining.Models.Pages;
using EPiServer;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog.Events;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
// for the extension-method
using Mediachase.Commerce.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{    
    public class CheckOutController : PageController<CheckOutPage>
    {

        private const string DefaultCart = "Default";

        private readonly IContentLoader _contentLoader; // To get the StartPage --> Settings-links
        private readonly ICurrentMarket _currentMarket; // not in fund... yet
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IPromotionEngine _promotionEngine;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;

        public CheckOutController(IContentLoader contentLoader
    , ICurrentMarket currentMarket
    , IOrderRepository orderRepository
    , IPlacedPriceProcessor placedPriceProcessor
    , IInventoryProcessor inventoryProcessor
    , ILineItemValidator lineItemValidator
    , IOrderGroupCalculator orderGroupCalculator
    , ILineItemCalculator lineItemCalculator
    , IOrderGroupFactory orderGroupFactory
    , IPaymentProcessor paymentProcessor
    , IPromotionEngine promotionEngine)
        {
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
            _orderRepository = orderRepository;
            _orderGroupCalculator = orderGroupCalculator;
            _orderGroupFactory = orderGroupFactory;
            _paymentProcessor = paymentProcessor;
            _promotionEngine = promotionEngine;
            _lineItemCalculator = lineItemCalculator;
            _inventoryProcessor = inventoryProcessor;
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
        }

        // ToDo: in the first exercise (E1) Ship & Pay
        public ActionResult Index(CheckOutPage currentPage)
        {
            // Try to load the cart  
            var cart = _orderRepository.LoadOrCreateCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), DefaultCart);

            if (cart == null)
            {
                throw new InvalidOperationException("No cart found");
            }


            var model = new CheckOutViewModel(currentPage)
            {
                PaymentMethods = GetPaymentMethods(),
                ShippingRates = GetShippingRates(),
                ShippingMethods = GetShipmentMethods()

            };

            return View(model);
        }


        // Exercise (E1) creation of GetPaymentMethods(), GetShipmentMethods() and GetShippingRates() goes below
        // ToDo: Get IEnumerables of Shipping and Payment methods along with ShippingRates

        public IEnumerable<PaymentMethodDto.PaymentMethodRow> GetPaymentMethods()
        {
            return new List<PaymentMethodDto.PaymentMethodRow>(
                PaymentManager.GetPaymentMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value).PaymentMethod);
        }

        public IEnumerable<ShippingMethodDto.ShippingMethodRow> GetShipmentMethods()
        {
            return new List<ShippingMethodDto.ShippingMethodRow>(
                ShippingManager.GetShippingMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value, false).ShippingMethod);
        }

        public IEnumerable<ShippingRate> GetShippingRates()
        {
            List<ShippingRate> shippingRates = new List<ShippingRate>();

            IEnumerable<ShippingMethodDto.ShippingMethodRow> shippingMethods = GetShipmentMethods();

            foreach (var item in shippingMethods)
            {
                shippingRates.Add(new ShippingRate(item.ShippingMethodId, item.DisplayName, new Money(item.BasePrice, item.Currency)));
            }

            return shippingRates;
        }

        //Exercise (E2) Do CheckOut
        public ActionResult CheckOut(CheckOutViewModel model)
        {
            // ToDo: Load the cart
            var cart = _orderRepository.Load<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), DefaultCart).FirstOrDefault();

            if (cart == null)
            {
                throw new InvalidOperationException("No cart found");
            }
            // ToDo: Add an OrderAddress
            IOrderAddress theAddress = AddAddressToOrder(cart);

            // ToDo: Define/update Shipping
            AdjustFirstShipmentInOrder(cart, theAddress, model.SelectedShipId);

            var rewards = cart.ApplyDiscounts();

            // ToDo: Add a Payment to the Order 
            AddPaymentToOrder(cart, model.SelectedPayId);

            // ToDo: Add a transaction scope and convert the cart to PO
            IPurchaseOrder purchaseOrder;
            OrderReference orderReference;

            using (var scope = new TransactionScope())
            {
                var validationIssues = new Dictionary<ILineItem, ValidationIssue>();

                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment(), OrderStatus.InProgress,
                    (item, issue) =>
                  {
                      validationIssues.Add(item, issue);
                  });

                if (validationIssues.Count >= 1)
                {
                    throw new Exception("Not possible right now.");
                }

                IEnumerable<PaymentProcessingResult> paymentProcessingResults = cart.ProcessPayments();

                var cartTotal = cart.GetTotal();
                var handling = cart.GetHandlingTotal();
                var form = cart.GetFirstForm();
                var formHandling = form.HandlingTotal;

                var totalProcessAmount = cart.GetFirstForm().Payments
                    .Where(x => x.Status.Equals(PaymentStatus.Processed.ToString()))
                    .Sum(x => x.Amount);

                if (totalProcessAmount != cart.GetTotal(_orderGroupCalculator).Amount)
                {
                    _inventoryProcessor.AdjustInventoryOrRemoveLineItem(
                        cart.GetFirstShipment(),
                        OrderStatus.Cancelled,
                        (item, issue) => validationIssues.Add(item, issue));

                    throw new InvalidOperationException("Wrong amount");
                }

                cart.GetFirstShipment().OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;

                // Decrement inventory and let it go
                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment(), OrderStatus.Completed,
                    (item, issue) => validationIssues.Add(item, issue));

                orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
                _orderRepository.Delete(cart.OrderLink);
                scope.Complete();
            }

            // ToDo: Housekeeping (Statuses for Shipping and PO, OrderNotes and save the order)
            purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);

            // Retrieve purchase order by order number
            //ServiceLocator.Current.GetInstance<IPurchaseOrderRepository>().Load("OrderNumer")

            OrderStatus poStatus;

            var shipment = purchaseOrder.GetFirstShipment();
            var status = shipment.OrderShipmentStatus;

            var notes = purchaseOrder.Notes;

            OrderNote otherNote = new OrderNote
            {
                CustomerId = Guid.NewGuid(),
                Detail = "Order Tostring(): " + ToString() + " - Shipment tracking number: " + shipment.ShipmentTrackingNumber,
                Title = "Some title",
                Type = OrderNoteTypes.Custom.ToString()
            };

            purchaseOrder.Notes.Add(otherNote);
            purchaseOrder.ExpirationDate = DateTime.Now.AddMonths(1);

            _orderRepository.Save(purchaseOrder);


            // Final steps, navigate to the order confirmation page
            StartPage home = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            ContentReference orderPageReference = home.Settings.orderPage;

            // the below is a dummy, change to "PO".OrderNumber when done
            string passingValue = purchaseOrder.OrderNumber;

            return RedirectToAction("Index", new { node = orderPageReference, passedAlong = passingValue });
        }


        // Prewritten 
        private string ValidateCart(ICart cart)
        {
            var validationMessages = string.Empty;

            cart.ValidateOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _lineItemValidator);

            cart.UpdatePlacedPriceOrRemoveLineItems(GetContact(), (item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _placedPriceProcessor);

            cart.UpdateInventoryOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _inventoryProcessor);

            return validationMessages;
        }

        private static string CreateValidationMessages(ILineItem item, ValidationIssue issue)
        {
            return string.Format("Line item with code {0} had the validation issue {1}.", item.Code, issue);
        }

        private void AdjustFirstShipmentInOrder(ICart cart, IOrderAddress orderAddress, Guid selectedShip)
        {
            var shippingMethod = ShippingManager.GetShippingMethod(selectedShip).ShippingMethod.First();

            IShipment theShip = cart.GetFirstShipment();

            theShip.ShippingMethodId = shippingMethod.ShippingMethodId;

            Money cost0 = theShip.GetShippingCost(_currentMarket.GetCurrentMarket(), _currentMarket.GetCurrentMarket().DefaultCurrency);

            Money cost1 = theShip.GetShippingItemsTotal(_currentMarket.GetCurrentMarket().DefaultCurrency);

            theShip.ShipmentTrackingNumber = "ABC123";
        }

        private void AddPaymentToOrder(ICart cart, Guid selectedPaymentGuid)
        {
            if (cart.GetFirstForm().Payments.Any())
            {

            }

            var selectedPaymentMethod = PaymentManager.GetPaymentMethod(selectedPaymentGuid).PaymentMethod.First();

            var payment = _orderGroupFactory.CreatePayment(cart);
            payment.PaymentMethodId = selectedPaymentMethod.PaymentMethodId;
            payment.PaymentMethodName = selectedPaymentMethod.Name;

            var className = selectedPaymentMethod.ClassName;

            payment.Amount = _orderGroupCalculator.GetTotal(cart).Amount;

            cart.AddPayment(payment);
        }

        private IOrderAddress AddAddressToOrder(ICart cart)
        {
            IOrderAddress shippingAddress = null;

            if (CustomerContext.Current.CurrentContact == null)
            {
                var shipment = cart.GetFirstShipment();

                if (shipment == null)
                {

                }

                IOrderAddress myOrderAddress = _orderGroupFactory.CreateOrderAddress(cart);
                myOrderAddress.CountryName = "Sweden";
                myOrderAddress.Id = "MyNewAddress";
                myOrderAddress.Email = "abc@test.com";

                shippingAddress = myOrderAddress;
            }
            else
            {
                if (CustomerContext.Current.CurrentContact.PreferredShippingAddress == null)
                {
                    CustomerAddress newCustomerAddress = CustomerAddress.CreateForApplication();

                    newCustomerAddress.AddressType = CustomerAddressTypeEnum.Shipping;
                    newCustomerAddress.ContactId = CustomerContext.Current.CurrentContact.PrimaryKeyId;
                    newCustomerAddress.CountryCode = "SWE";
                    newCustomerAddress.CountryName = "Sweden";
                    newCustomerAddress.Name = "new customer address"; // mandatory
                    newCustomerAddress.DaytimePhoneNumber = "123456";
                    newCustomerAddress.FirstName = CustomerContext.Current.CurrentContact.FirstName;
                    newCustomerAddress.LastName = CustomerContext.Current.CurrentContact.LastName;
                    newCustomerAddress.Email = "ABC@test.com";

                    // note: Line1 & City is what is shown in CM at a few places... not the Name
                    CustomerContext.Current.CurrentContact.AddContactAddress(newCustomerAddress);
                    CustomerContext.Current.CurrentContact.SaveChanges();

                    // ... needs to be in this order
                    CustomerContext.Current.CurrentContact.PreferredShippingAddress = newCustomerAddress;
                    CustomerContext.Current.CurrentContact.SaveChanges();

                    // then, for the cart
                    //.Cart.OrderAddresses.Add(new OrderAddress(newCustAddress)); - OLD
                    shippingAddress = new OrderAddress(newCustomerAddress); // - NEW

                }
                else
                {
                    shippingAddress = new OrderAddress(CustomerContext.Current.CurrentContact.PreferredShippingAddress);
                }
            }

            return shippingAddress;
        }

        private static CustomerContact GetContact()
        {
            return CustomerContext.Current.GetContactById(GetContactId());
        }

        private static Guid GetContactId()
        {
            return PrincipalInfo.CurrentPrincipal.GetContactId();
        }
    }
}