using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    //[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class EPiServerCommerceInitializationModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            DependencyResolver.SetResolver(
                new StructureMapDependencyResolver(context.StructureMap()));
        }

        public void Initialize(InitializationEngine context)
        {
            //DependencyResolver.SetResolver(new ServiceLocatorDependencyResolver(context.Locate.Advanced));
            CatalogRouteHelper.MapDefaultHierarchialRouter(RouteTable.Routes, false);
        }

        public void Preload(string[] parameters) { }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }


    //[InitializableModule]
    //[ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    //public class EPiServerCommerceInitializationModule2 : IInitializableModule
    //{
    //    public void Initialize(InitializationEngine context)
    //    {

    //        CatalogRouteHelper.MapDefaultHierarchialRouter(RouteTable.Routes, false);
    //    }

    //    public void Preload(string[] parameters) { }

    //    public void Uninitialize(InitializationEngine context)
    //    {
    //    }
    //}
}
