﻿using System;
using System.Collections.Generic;

namespace ESFA.DC.ILR.ValidationService.Data.External.LARS.Interface
{
    public interface ILARSDataService
    {
        bool EffectiveDatesValidforLearnAimRef(string learnAimRef, DateTime date);

        bool FrameworkCodeExistsForFrameworkAims(string learnAimRef, int? progType, int? fworkCode, int? pwayCode);

        bool FrameworkCodeExistsForCommonComponent(string learnAimRef, int? progType, int? fworkCode, int? pwayCode);

        bool LearnAimRefExists(string learnAimRef);

        bool LearnAimRefExistsForLearningDeliveryCategoryRef(string learnAimRef, int categoryRef);

        bool NotionalNVQLevelMatchForLearnAimRef(string learnAimRef, string level);

        bool NotionalNVQLevelV2MatchForLearnAimRefAndLevel(string learnAimRef, string level);

        bool NotionalNVQLevelV2MatchForLearnAimRefAndLevels(string learnAimRef, IEnumerable<string> levels);

        bool FullLevel2EntitlementCategoryMatchForLearnAimRef(string learnAimRef, int level);

        bool FullLevel3EntitlementCategoryMatchForLearnAimRef(string learnAimRef, int level);

        bool FullLevel3PercentForLearnAimRefAndDateAndPercentValue(string learnAimRef, DateTime learnStartDate, decimal percentValue);

        bool LearnDirectClassSystemCode1MatchForLearnAimRef(string learnAimRef);

        bool BasicSkillsMatchForLearnAimRef(string learnAimRef, int basicSkills);
    }
}
