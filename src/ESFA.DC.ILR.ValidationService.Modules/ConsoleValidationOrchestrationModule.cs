﻿using System.Collections.Generic;
using Autofac;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Providers;
using ESFA.DC.ILR.ValidationService.RuleSet;
using ESFA.DC.ILR.ValidationService.RuleSet.ErrorHandler;
using ESFA.DC.ILR.ValidationService.Stubs;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Xml;

namespace ESFA.DC.ILR.ValidationService.Modules
{
    public class ConsoleValidationOrchestrationModule : ActorValidationOrchestrationModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsolePreValidationOrchestrationService<ILearner, IValidationError>>()
                .As<IPreValidationOrchestrationService<ILearner, IValidationError>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MessageFileSystemProviderServiceStub>().As<IValidationItemProviderService<IMessage>>();
            base.Load(builder);
        }
    }
}