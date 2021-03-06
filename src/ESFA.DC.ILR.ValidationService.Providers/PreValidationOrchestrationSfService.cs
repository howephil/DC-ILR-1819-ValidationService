﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Interface;
using ESFA.DC.ILR.ValidationService.Data.Population.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.ValidationActor.Interfaces;
using ESFA.DC.ILR.ValidationService.ValidationActor.Interfaces.Models;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace ESFA.DC.ILR.ValidationService.Providers
{
    public class PreValidationOrchestrationSfService<U> : IPreValidationOrchestrationService<U>
    {
        private readonly IPopulationService _preValidationPopulationService;
        private readonly ILearnerPerActorService _learnerPerActorService;
        private readonly ICache<IMessage> _messageCache;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IInternalDataCache _internalDataCache;
        private readonly IExternalDataCache _externalDataCache;
        private readonly IFileDataCache _fileDataCache;
        private readonly IValidationErrorCache<U> _validationErrorCache;
        private readonly IValidationOutputService<U> _validationOutputService;
        private readonly IValidationItemProviderService<IEnumerable<IMessage>> _validationItemProviderService;
        private readonly IRuleSetOrchestrationService<IMessage, U> _ruleSetOrchestrationService;
        private readonly ILogger _logger;
        private readonly IValidateXMLSchemaService _validateXmlSchemaService;

        public PreValidationOrchestrationSfService(
            IPopulationService preValidationPopulationService,
            ICache<IMessage> messageCache,
            ILearnerPerActorService learnerPerActorService,
            IJsonSerializationService jsonSerializationService,
            IInternalDataCache internalDataCache,
            IExternalDataCache externalDataCache,
            IFileDataCache fileDataCache,
            IValidationErrorCache<U> validationErrorCache,
            IValidationOutputService<U> validationOutputService,
            IValidationItemProviderService<IEnumerable<IMessage>> validationItemProviderService,
            IRuleSetOrchestrationService<IMessage, U> ruleSetOrchestrationService,
            ILogger logger,
            IValidateXMLSchemaService validateXMLSchemaService)
        {
            _preValidationPopulationService = preValidationPopulationService;
            _learnerPerActorService = learnerPerActorService;
            _messageCache = messageCache;
            _jsonSerializationService = jsonSerializationService;
            _internalDataCache = internalDataCache;
            _externalDataCache = externalDataCache;
            _fileDataCache = fileDataCache;
            _validationErrorCache = validationErrorCache;
            _validationOutputService = validationOutputService;
            _validationItemProviderService = validationItemProviderService;
            _ruleSetOrchestrationService = ruleSetOrchestrationService;
            _logger = logger;
            _validateXmlSchemaService = validateXMLSchemaService;
        }

        public async Task<IEnumerable<U>> ExecuteAsync(IPreValidationContext validationContext, CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // get ILR data from file
            await _preValidationPopulationService.PopulateAsync(cancellationToken);
            _logger.LogDebug($"Population service completed in: {stopWatch.ElapsedMilliseconds}");

            cancellationToken.ThrowIfCancellationRequested();

            // get the learners
            var ilrMessage = _messageCache.Item;

            // Possible the zip file was corrupt so we dont have message at this point
            if (ilrMessage == null)
            {
                _logger.LogWarning($"ILR Message is null, will not execute any Learner validation Job Id: {validationContext.Input}");
            }
            else
            {
                // Call XSD validation
                _validateXmlSchemaService.Validate();

                if (!_validationErrorCache.ValidationErrors.Any(x => (((IValidationError)x).Severity ?? Interface.Enum.Severity.Error) == Interface.Enum.Severity.Error))
                {
                    // get the filename
                    _fileDataCache.FileName = validationContext.Input;

                    // Message Validation
                    await _ruleSetOrchestrationService.Execute(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!_validationErrorCache.ValidationErrors.Any(x => (((IValidationError)x).Severity ?? Interface.Enum.Severity.Error) == Interface.Enum.Severity.Error))
                    {
                        await ExecuteValidationActors(validationContext, cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug(
                            $"Header validation failed, so will not execute learner validation actors , error count : {_validationErrorCache.ValidationErrors.Count}");
                    }
                }
                else
                {
                    _logger.LogDebug($"possible xsd validation failure : {_validationErrorCache.ValidationErrors.Count}");
                }

                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogDebug(
                    $"Actors results collated {_validationErrorCache.ValidationErrors.Count} validation errors");
                await _validationOutputService.ProcessAsync(cancellationToken);
                _logger.LogDebug($"Validation Final results persisted {stopWatch.ElapsedMilliseconds}");
            }

            return null;
        }

        private IValidationActor GetValidationActor()
        {
            return ActorProxy.Create<IValidationActor>(
                ActorId.CreateRandom(),
                new Uri($"{FabricRuntime.GetActivationContext().ApplicationName}/ValidationActorService"));
        }

        private async Task ExecuteValidationActors(IPreValidationContext validationContext, CancellationToken cancellationToken)
        {
            // Get L/A and split the learners into separate lists
            var messageShards = _learnerPerActorService.Process();

            var actorTasks = new List<Task<string>>();

            foreach (var messageShard in messageShards)
            {
                _logger.LogDebug($"validation Shard has {messageShard.Learners.Count} learners");

                // create actors for each Shard.
                var actor = GetValidationActor();

                // TODO:get reference data per each shard and send it to Actors
                var ilrMessageAsBytes = Encoding.UTF8.GetBytes(_jsonSerializationService.Serialize(messageShard));

                var internalDataCacheAsBytes =
                    Encoding.UTF8.GetBytes(_jsonSerializationService.Serialize(_internalDataCache));
                var externalDataCacheAsBytes =
                    Encoding.UTF8.GetBytes(_jsonSerializationService.Serialize(_externalDataCache));
                var fileDataCacheAsBytes =
                    Encoding.UTF8.GetBytes(_jsonSerializationService.Serialize(_fileDataCache));

                var validationActorModel = new ValidationActorModel
                {
                    JobId = validationContext.JobId,
                    Message = ilrMessageAsBytes,
                    InternalDataCache = internalDataCacheAsBytes,
                    ExternalDataCache = externalDataCacheAsBytes,
                    FileDataCache = fileDataCacheAsBytes,
                };

                actorTasks.Add(actor.Validate(validationActorModel, cancellationToken));
            }

            _logger.LogDebug($"Starting {actorTasks.Count} validation actors");

            await Task.WhenAll(actorTasks.ToArray());

            _logger.LogDebug("all Actors completed");

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Task<string> actorTask in actorTasks)
            {
                var errors = _jsonSerializationService.Deserialize<IEnumerable<U>>(actorTask.Result);

                foreach (var error in errors)
                {
                    _validationErrorCache.Add(error);
                }
            }
        }
    }
}
