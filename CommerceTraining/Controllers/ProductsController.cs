using System.Web.Mvc;
using CommerceTraining.Models.Catalog;

namespace CommerceTraining.Controllers
{
    public class ProductsController : CatalogControllerBase<ShirtProduct>
    {
        public ActionResult Index(ShirtProduct currentContent)
        {
            return View(currentContent);
        }
    }
}