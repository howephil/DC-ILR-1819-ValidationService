﻿using ESFA.DC.ILR.ValidationService.Data.Interface;
using ESFA.DC.ILR.ValidationService.Data.Internal;
using ESFA.DC.ILR.ValidationService.Data.Internal.Model;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Data.Tests.Internal
{
    /// <summary>
    /// the lookups details provider test fixture
    /// </summary>
    public class LookupDetailsProviderTests
    {
        /// <summary>
        /// Provider 'is current' values match expectation.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <param name="testCaseDate">The test case date.</param>
        /// <param name="expectation">if set to <c>true</c> [expectation].</param>
        [Theory]
        [InlineData(1, "2013-06-14", true)]
        [InlineData(1, "2013-06-13", false)]
        [InlineData(2, "2015-09-03", true)]
        [InlineData(2, "2007-09-03", false)]
        [InlineData(3, "2012-06-18", false)]
        [InlineData(5, "2013-06-14", true)]
        [InlineData(5, "2010-10-14", false)]
        [InlineData(9, "2004-05-01", true)]
        [InlineData(9, "2008-08-27", false)]
        public void ProviderIsCurrentValuesMatchExpectation(int candidate, string testCaseDate, bool expectation)
        {
            // arrange
            var sut = NewService();
            var testDate = DateTime.Parse(testCaseDate);

            // act
            var result = sut.IsCurrent(LookupTimeRestrictedKey.TTAccom, candidate, testDate);

            // assert
            Assert.Equal(expectation, result);
        }

        /// <summary>
        /// Provider contains value matches expectation.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <param name="expectation">if set to <c>true</c> [expectation].</param>
        [Theory]
        [InlineData(1, true)]
        [InlineData(3, false)]
        [InlineData(9, true)]
        [InlineData(26, false)]
        public void ProviderContainsSimpleValueMatchesExpectation(int candidate, bool expectation)
        {
            // arrange
            var sut = NewService();

            // act
            var result = sut.Contains(LookupSimpleKey.FINTYPE, candidate);

            // assert
            Assert.Equal(expectation, result);
        }

        /// <summary>
        /// Provider contains coded value matches expectation.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <param name="expectation">if set to <c>true</c> [expectation].</param>
        [Theory]
        [InlineData("TNP", true)]
        [InlineData("AMP", false)]
        [InlineData("ME", true)]
        [InlineData("UOY", false)]
        public void ProviderContainsCodedValueMatchesExpectation(string candidate, bool expectation)
        {
            // arrange
            var sut = NewService();

            // act
            var result = sut.Contains(LookupCodedKey.AppFinRecord, candidate);

            // assert
            Assert.Equal(expectation, result);
        }

        /// <summary>
        /// Provider contains limited life value matches expectation.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <param name="expectation">if set to <c>true</c> [expectation].</param>
        [Theory]
        [InlineData(1, true)]
        [InlineData(3, false)]
        [InlineData(9, true)]
        [InlineData(26, false)]
        public void ProviderContainsLimitedLifeValueMatchesExpectation(int candidate, bool expectation)
        {
            // arrange
            var sut = NewService();

            // act
            var result = sut.Contains(LookupTimeRestrictedKey.TTAccom, candidate);

            // assert
            Assert.Equal(expectation, result);
        }

        /// <summary>
        /// New service.
        /// </summary>
        /// <returns>a <seealso cref="LookupDetailsProvider"/></returns>
        public LookupDetailsProvider NewService()
        {
            var cacheFactory = new Mock<ICreateInternalDataCache>();
            var cache = new InternalDataCache();
            var finTypes = new List<int>() { 1, 2, 4, 5, 6, 9, 24, 25, 29, 45 };
            var codedTypes = new List<string>() { "TNP", "PMR", "AEC", "UI", "OT", "ME", "YOU" };
            var tTAccomItems = new Dictionary<int, ValidityPeriods>()
            {
                [1] = new ValidityPeriods(validFrom: DateTime.Parse("2013-06-14"), validTo: DateTime.Parse("2020-06-14")),
                [2] = new ValidityPeriods(validFrom: DateTime.Parse("2009-04-28"), validTo: DateTime.Parse("2020-06-14")),
                [4] = new ValidityPeriods(validFrom: DateTime.Parse("2012-09-06"), validTo: DateTime.Parse("2015-02-28")),
                [5] = new ValidityPeriods(validFrom: DateTime.Parse("2010-11-21"), validTo: DateTime.Parse("2020-06-14")),
                [6] = new ValidityPeriods(validFrom: DateTime.Parse("2018-07-02"), validTo: DateTime.Parse("2020-06-14")),
                [9] = new ValidityPeriods(validFrom: DateTime.Parse("2000-02-01"), validTo: DateTime.Parse("2008-08-26")),
            };

            cache.SimpleLookups.Add(LookupSimpleKey.FINTYPE, finTypes);
            cache.CodedLookups.Add(LookupCodedKey.AppFinRecord, codedTypes);
            cache.LimitedLifeLookups.Add(LookupTimeRestrictedKey.TTAccom, tTAccomItems);

            cacheFactory
                .Setup(c => c.Create())
                .Returns(cache);

            return new LookupDetailsProvider(cacheFactory.Object);
        }
    }
}
