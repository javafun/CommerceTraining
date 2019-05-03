using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]

    public class DemoCurrentMarketRegistration : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<ICurrentMarket, DemoCurrentMarket>();
        }

        public void Initialize(InitializationEngine context)
        {
            
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
