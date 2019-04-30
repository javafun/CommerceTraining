using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
using Mediachase.MetaDataPlus;
using Mediachase.Search.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace CommerceManager
{
    public class IndexBuilder : BaseCatalogIndexBuilder
    {
        private readonly ICatalogSystem _catalogSystem;

        public IndexBuilder() : this(ServiceLocator.Current.GetInstance<ICatalogSystem>(),
            ServiceLocator.Current.GetInstance<IPriceService>(),
            ServiceLocator.Current.GetInstance<IInventoryService>(),
            ServiceLocator.Current.GetInstance<MetaDataContext>(),
            ServiceLocator.Current.GetInstance<CatalogItemChangeManager>(),
            ServiceLocator.Current.GetInstance<NodeIdentityResolver>())            
        {

        }

        public IndexBuilder(ICatalogSystem catalogSystem,
            IPriceService priceService,
            IInventoryService inventoryService,
            MetaDataContext metaDataContext,
            CatalogItemChangeManager catalogItemChangeManager,
            NodeIdentityResolver nodeIdentityResolver) : base(catalogSystem, priceService, inventoryService, metaDataContext, catalogItemChangeManager, nodeIdentityResolver)
        {
            _catalogSystem = catalogSystem;
        }

        protected override void OnCatalogEntryIndex(
            ref SearchDocument document,
            CatalogEntryDto.CatalogEntryRow entry,
            string language)
        {
            if(entry != null && entry.ClassTypeId.Equals(EntryType.Product, System.StringComparison.InvariantCultureIgnoreCase))
            {
                CatalogRelationDto catalogRelationDto = _catalogSystem.GetCatalogRelationDto(0, 0, entry.CatalogEntryId, string.Empty, new CatalogRelationResponseGroup(CatalogRelationResponseGroup.ResponseGroup.CatalogEntry));

                if (catalogRelationDto != null && catalogRelationDto.CatalogEntryRelation != null && catalogRelationDto.CatalogEntryRelation.Count > 0)
                {
                    List<int> childIds = new List<int>();

                    foreach (CatalogRelationDto.CatalogEntryRelationRow relationRow in catalogRelationDto.CatalogEntryRelation)
                    {
                        childIds.Add(relationRow.ChildEntryId);
                    }

                    CatalogEntryDto skuDto = _catalogSystem.GetCatalogEntriesDto(childIds.ToArray(),
                        new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo));

                    List<string> searchProperties = new List<string>
                    {
                        SearchField.Store.NO,
                        SearchField.Index.TOKENIZED,
                        SearchField.IncludeInDefaultSearch.YES
                    };


                    List<string> colorVariations = new List<string>();
                    if (skuDto != null && skuDto.CatalogEntry != null && skuDto.CatalogEntry.Count > 0)
                    {
                        foreach (CatalogEntryDto.CatalogEntryRow row in skuDto.CatalogEntry)
                        {
                            Hashtable hash = ObjectHelper.GetMetaFieldValues(row);
                            if (hash.Contains("Color"))
                            {
                                string color = hash["Color"].ToString();
                                if (!string.IsNullOrEmpty(color) && !colorVariations.Contains(color.ToLower()))
                                {
                                    colorVariations.Add(color.ToLower());
                                    document.Add(new SearchField("Color", color.ToLower(), searchProperties.ToArray()));
                                    OnSearchIndexMessage(new Mediachase.Search.SearchIndexEventArgs(
                                        $"The color {color} was added to the index for {entry.Name}.", 1));

                                }

                                // Commerce dictionary type 

                                //PropertyDictionarySingle
                                //PropertyDictionaryMultiple
                                //PropertyStringDictionary
                            }

                        }
                    }
                }
            }
        }
    }
}
