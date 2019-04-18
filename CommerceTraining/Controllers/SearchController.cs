using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class SearchController : PageController<SearchPage>
    {
        public IEnumerable<IContent> localContent { get; set; }
        public readonly IContentLoader _contentLoader;
        public readonly ReferenceConverter _referenceConverter;
        public readonly UrlResolver _urlResolver;

        public SearchController(IContentLoader contentLoader, ReferenceConverter referenceConverter, UrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _urlResolver = urlResolver;
        }

        public ActionResult Index(SearchPage currentPage)
        {
            var model = new SearchPageViewModel
            {
                CurrentPage = currentPage,
            };

            return View(model);
        }

        public ActionResult Search(string keyWord)
        {
            // ToDo: SearchHelper and Criteria 
            var searchFilterHelper = SearchFilterHelper.Current;
            CatalogEntrySearchCriteria criteria = searchFilterHelper.CreateSearchCriteria(keyWord,
                CatalogEntrySearchCriteria.DefaultSortOrder);
            criteria.RecordsToRetrieve = 25;
            criteria.StartingRecord = 0;
            criteria.Locale = ContentLanguage.PreferredCulture.Name;
            int count = 0;
            bool cacheResult = true;
            TimeSpan timeSpan = new TimeSpan(0, 10, 0);
            // ToDo: Search 

            ISearchResults searchResult = searchFilterHelper.SearchEntries(criteria);
            ISearchDocument aDoc = searchResult.Documents.FirstOrDefault();

            int[] ints = searchResult.GetKeyFieldValues<int>();
            List<ContentReference> refs = new List<ContentReference>();
            ints.ToList().ForEach(i => refs.Add(_referenceConverter.GetContentLink(i, CatalogContentType.CatalogEntry, 0)));

            localContent = _contentLoader.GetItems(refs, new LoaderOptions());
            // ToDo: Facets
            List<string> facetList = new List<string>();

            int facetGroups = searchResult.FacetGroups.Count();

            foreach (ISearchFacetGroup item in searchResult.FacetGroups)
            {
                foreach (var item2 in item.Facets)
                {
                    facetList.Add($"{item.Name} {item2.Name} {item2.Count}");
                }
            }

            // ToDo: As a last step - un-comment and fill up the ViewModel
            var searchResultViewModel = new SearchResultViewModel();

            searchResultViewModel.totalHits = new List<string> { "" }; // change
            searchResultViewModel.nodes = localContent.OfType<FashionNode>();
            searchResultViewModel.products = localContent.OfType<ShirtProduct>();
            searchResultViewModel.variants = localContent.OfType<ShirtVariation>();
            searchResultViewModel.allContent = localContent;
            searchResultViewModel.facets = facetList;


            return View(searchResultViewModel);
        }
    }
}