using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(DisplayName = "FashionNode", 
        MetaClassName = "Fashion_Node", 
        GUID = "885ac61a-7611-4508-a8e4-86658a40abaa", Description = "")]
    public class FashionNode : NodeContent
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