using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(DisplayName = "BlouseProduct",
        GUID = "9f3a0b2d-27a8-4eaa-ad8b-7b412dc91dab",
        Description = "")]
    public class BlouseProduct : ProductContent
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [Tokenize]
        public virtual XhtmlString MainBody { get; set; }
    }
}
