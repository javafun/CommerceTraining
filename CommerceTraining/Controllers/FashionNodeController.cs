using System.Web.Mvc;
using CommerceTraining.Models.Catalog;

namespace CommerceTraining.Controllers
{
    public class FashionNodeController : CatalogControllerBase<FashionNode>
    {
        public ActionResult Index(FashionNode currentContent)
        {
            return View(currentContent);
        }
    }
}