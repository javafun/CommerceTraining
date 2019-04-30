using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(DisplayName = "ShirtVariation", MetaClassName ="Shirt_Variation",GUID = "6e65fbd6-70ff-462a-986c-d31ef7cd33a0", Description = "")]
    public class ShirtVariation : VariationContent
    {

        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [Tokenize]
        public virtual XhtmlString MainBody { get; set; }

        [IncludeInDefaultSearch]
        public virtual string Size { get; set; }

        [IncludeInDefaultSearch]
        public virtual string Color { get; set; }
        public virtual bool CanBeMonogrammed { get; set; }

        [Tokenize]
        [IncludeInDefaultSearch]
        [IncludeValuesInSearchResults]
        public virtual string ThematicTag { get; set; }
        public virtual string testc { get; set; }
    }
}