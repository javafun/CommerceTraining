using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(DisplayName = "ShirtBundle", GUID = "6e2a0b8d-27a8-4eaa-ad8b-7b412dc91dab", Description = "")]
    public class ShirtBundle : BundleContent
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
