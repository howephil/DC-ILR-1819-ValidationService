﻿using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;

namespace ESFA.DC.ILR.ValidationService.Rules.Query.Interface
{
    public interface ILearningDeliveryFAMQueryService
    {
        bool HasAnyLearningDeliveryFAMCodesForType(IEnumerable<ILearningDeliveryFAM> learningDeliveryFAMs, string famType, IEnumerable<string> famCodes);

        bool HasLearningDeliveryFAMCodeForType(IEnumerable<ILearningDeliveryFAM> learningDeliveryFAMs, string famType, string famCode);

        bool HasLearningDeliveryFAMType(IEnumerable<ILearningDeliveryFAM> learningDeliveryFAMs, string famType);

        bool HasAnyLearningDeliveryFAMTypes(IEnumerable<ILearningDeliveryFAM> learningDeliveryFAMs, IEnumerable<string> famTypes);

        ILearningDeliveryFAM GetLearningDeliveryFAMByTypeAndLatestByDateFrom(IEnumerable<ILearningDeliveryFAM> learningDeliveryFAMs, string learnDelFAMType);
    }
}
