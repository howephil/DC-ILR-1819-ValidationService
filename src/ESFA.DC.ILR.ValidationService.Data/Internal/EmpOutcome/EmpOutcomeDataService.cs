﻿using System.Linq;
using ESFA.DC.ILR.ValidationService.Data.Internal.EmpOutcome.Interface;
using IInternalDataCache = ESFA.DC.ILR.ValidationService.Data.Interface.IInternalDataCache;

namespace ESFA.DC.ILR.ValidationService.Data.Internal.EmpOutcome
{
    public class EmpOutcomeDataService : IEmpOutcomeDataService
    {
        private readonly IInternalDataCache _internalDataCache;

        public EmpOutcomeDataService(IInternalDataCache internalDataCache)
        {
            _internalDataCache = internalDataCache;
        }

        public bool Exists(int empOutcome)
        {
            return _internalDataCache.EmpOutcomes.Contains(empOutcome);
        }
    }
}
