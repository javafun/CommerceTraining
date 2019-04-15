using EPiServer.Core;
using System.Collections.Generic;

namespace CommerceTraining.Models.ViewModels
{
    public class AdminViewModel
    {
        public XhtmlString MainBody { get; set; }
        public List<RefInfo> Refs { get; set; }

        public AdminViewModel()
        {
            Refs = new List<RefInfo>();
        }
    }

    public class RefInfo
    {
        public ContentReference Ref { get; set; }
        public int DbId { get; set; }
        public string Code { get; set; }
        public string CatalogType { get; set; }
        public ContentReference RootRef { get; set; }
        public RefInfo()
        {
            Code = "None";
            CatalogType = "None";
        }
    }
}