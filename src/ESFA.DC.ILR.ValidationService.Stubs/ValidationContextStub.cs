﻿using ESFA.DC.ILR.ValidationService.Interface;

namespace ESFA.DC.ILR.ValidationService.Stubs
{
    public class ValidationContextStub : IValidationContext
    {
        public string Input { get; set; }

        public string Output { get; set; }
    }
}