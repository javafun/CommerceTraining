using System;
using System.Linq;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class AssociationTypeInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var associationTypeRepository = context.Locate.Advanced.GetInstance<GroupDefinitionRepository<AssociationGroupDefinition>>();

            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = "cross-selling" });
            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = "upselling" });            
        }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}