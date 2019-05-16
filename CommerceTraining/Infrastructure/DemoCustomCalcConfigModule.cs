using CommerceTraining.Infrastructure.CartAndCheckout;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Calculator;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DemoCustomCalcConfigModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.Intercept<ITaxCalculator>(
                (_, defaultTaxCalculator) => new DemoCustomTaxCalc(defaultTaxCalculator));

        }

        public void Initialize(InitializationEngine context)
        {

        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
