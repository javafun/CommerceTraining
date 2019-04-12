using System.Web.Mvc;
using CommerceTraining.Models.Catalog;

namespace CommerceTraining.Controllers
{
    public class VariationsController : CatalogControllerBase<ShirtVariation>
    {
        public ActionResult Index(ShirtVariation currentContent)
        {
            return View(currentContent);
        }
    }
}