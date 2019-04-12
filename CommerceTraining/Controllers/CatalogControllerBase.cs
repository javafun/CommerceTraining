using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class CatalogControllerBase<T> : 
        ContentController<T> where T: CatalogContentBase
    {
        
    }
}