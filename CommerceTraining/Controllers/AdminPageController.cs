using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer.Core;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
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