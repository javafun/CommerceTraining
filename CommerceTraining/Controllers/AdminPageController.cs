using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer.Core;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class AdminPageController : PageController<AdminPage>
    {
        private readonly ReferenceConverter _referenceConverter;

        public AdminPageController(ReferenceConverter referenceConverter)
        {
            _referenceConverter = referenceConverter;
        }


        public ActionResult Index(AdminPage currentPage)
        {
            AdminViewModel model = new AdminViewModel();

            ContentReference aRef = _referenceConverter.GetContentLink("Shirts_1");
            ContentReference aRef2 = _referenceConverter.GetContentLink("Men_1");
            ContentReference aRef3 = _referenceConverter.GetContentLink("Long-Sleeve-Shirt_1");

            // Alternatively, using object id to retrieve content reference.
            //ContentReference aRef3 = _referenceConverter.GetContentLink(2,CatalogContentType.CatalogEntry,0);

            CheckReferenceConverter(aRef, model);
            CheckReferenceConverter(aRef2, model);
            CheckReferenceConverter(aRef3, model);

            return View(model);
        }

        public void CreateTaxCategoryAndJurisdiction()
        {
            CatalogTaxDto t_Dto = CatalogTaxManager.CreateTaxCategory("VAT", true);

            JurisdictionDto jurisdictionDto = JurisdictionManager.GetJurisdictions(JurisdictionManager.JurisdictionType.Tax);

            JurisdictionDto.JurisdictionRow jurisdictionRow = jurisdictionDto.Jurisdiction.NewJurisdictionRow();
            jurisdictionRow.County = "HomeLand";
            jurisdictionRow.DisplayName = "HomeLand";
            jurisdictionRow.District = "WholeCountry";
            jurisdictionRow.CountryCode = "se";
            jurisdictionRow.Code = "se";
            jurisdictionRow.JurisdictionType = (int)JurisdictionManager.JurisdictionType.Tax;
            jurisdictionDto.Jurisdiction.AddJurisdictionRow(jurisdictionRow);

            JurisdictionDto.JurisdictionGroupRow jurisdictionGroup = jurisdictionDto.JurisdictionGroup.NewJurisdictionGroupRow();
            jurisdictionGroup.DisplayName = "HomeLand Group";
            jurisdictionGroup.Code = "se_gr";
            jurisdictionGroup.JurisdictionType = JurisdictionManager.JurisdictionType.Tax.GetHashCode();
            jurisdictionDto.JurisdictionGroup.AddJurisdictionGroupRow(jurisdictionGroup);

            JurisdictionDto.JurisdictionRelationRow jurisdictionRelation = jurisdictionDto.JurisdictionRelation.NewJurisdictionRelationRow();
            jurisdictionRelation.JurisdictionRow = jurisdictionRow;
            jurisdictionRelation.JurisdictionGroupRow = jurisdictionGroup;
            jurisdictionDto.JurisdictionRelation.AddJurisdictionRelationRow(jurisdictionRelation);

            JurisdictionManager.SaveJurisdiction(jurisdictionDto);
        }

        public void CreateTaxes()
        {
            TaxDto orderTaxDto = TaxManager.GetTaxDto(TaxType.SalesTax);

            TaxDto.TaxRow taxRow = orderTaxDto.Tax.AddTaxRow(TaxType.SalesTax.GetHashCode(), "HomeLand_VAT", 10);

            TaxDto.TaxValueRow taxValueRow = orderTaxDto.TaxValue.NewTaxValueRow();

            taxValueRow.TaxId = taxRow.TaxId;
            taxValueRow.JurisdictionGroupId = JurisdictionManager.GetJurisdictionGroup("se_gr").JurisdictionGroup[0].JurisdictionGroupId;
            taxValueRow.TaxCategory = "VAT";
            taxValueRow.Percentage = 25;
            taxValueRow.AffectiveDate = DateTime.UtcNow;
            orderTaxDto.TaxValue.AddTaxValueRow(taxValueRow);

            TaxManager.SaveTax(orderTaxDto);
        }

        // For test
        private static void GetTaxes()
        {
            string taxCategory = CatalogTaxManager.GetTaxCategoryNameById(1);
            TaxValue[] t = OrderContext.Current.GetTaxes(
                Guid.Empty
                //OrderConfiguration.Instance.ApplicationId // ignored 
                , taxCategory, "sv", "sv", null, null, null, null, null);

            //TaxManager.GetTaxes()
        }

        private void CheckReferenceConverter(ContentReference contentReference, AdminViewModel model)
        {
            RefInfo refInfo= new RefInfo();
            refInfo.Ref = contentReference;
            refInfo.Code = _referenceConverter.GetCode(contentReference);
            refInfo.DbId = _referenceConverter.GetObjectId(contentReference);
            refInfo.CatalogType = _referenceConverter.GetContentType(contentReference).ToString();
            refInfo.RootRef = _referenceConverter.GetRootLink();
            model.Refs.Add(refInfo);
        }
    }
}