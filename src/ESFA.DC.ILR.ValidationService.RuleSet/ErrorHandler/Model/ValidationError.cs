﻿using System.Collections.Generic;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Interface.Enum;
using Newtonsoft.Json;

namespace ESFA.DC.ILR.ValidationService.RuleSet.ErrorHandler.Model
{
    public struct ValidationError : IValidationError
    {
        [JsonConstructor]
        public ValidationError(string ruleName, string learnerReferenceNumber, long? aimSequenceNumber = null, Severity? severity = null, IEnumerable<IErrorMessageParameter> errorMessageParameters = null)
        {
            LearnerReferenceNumber = learnerReferenceNumber;
            AimSequenceNumber = aimSequenceNumber;
            RuleName = ruleName;
            Severity = severity;
            ErrorMessageParameters = errorMessageParameters;
        }

        public string LearnerReferenceNumber { get; }

        public long? AimSequenceNumber { get; }

        public string RuleName { get; }

        public Severity? Severity { get; }

        public IEnumerable<IErrorMessageParameter> ErrorMessageParameters { get; }
    }
}