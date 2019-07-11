using Horse_Picker.Models;
using Horse_Picker.Services.Compute;
using System;
using System.Collections.Generic;
using Xunit;

namespace Horse_Picker.Tests.Services.Compute
{
    public class ComputeServiceTests
    {
        private ComputeService _serviceClass;

        private Dictionary<string, int> _raceCategoryDictionary;
        private RaceModel _raceModelProvider = new RaceModel()
        {
            Category = "I",
            City = "Waw",
            Distance = "1600",
            RaceDate = new DateTime(2019, 11, 10),
            RaceNo = "1"
        };

        public ComputeServiceTests()
        {
            _serviceClass = new ComputeService();
            _raceCategoryDictionary = new Dictionary<string, int>();
        }

        private Dictionary<string, int> GetCategoryDictionary(RaceModel raceModelProvider)
        {
            Dictionary<string, int> categoryFactorDict = new Dictionary<string, int>();

            categoryFactorDict.Add("G1 A", 11);
            categoryFactorDict.Add("G3 A", 10);
            categoryFactorDict.Add("LR A", 9);
            categoryFactorDict.Add("LR B", 8);
            categoryFactorDict.Add("L A", 7);
            categoryFactorDict.Add("B", 5);
            categoryFactorDict.Add("A", 4);
            categoryFactorDict.Add("Gd 3", 11);
            categoryFactorDict.Add("Gd 1", 10);
            categoryFactorDict.Add("L", 8);
            categoryFactorDict.Add("F", 7);
            categoryFactorDict.Add("C", 6);
            categoryFactorDict.Add("D", 5);
            categoryFactorDict.Add("I", 4);
            categoryFactorDict.Add("II", 3);
            categoryFactorDict.Add("III", 2);
            categoryFactorDict.Add("IV", 1);
            categoryFactorDict.Add("V", 1);
            if (raceModelProvider.Category == "sulki" || raceModelProvider.Category == "kłusaki")
            {
                categoryFactorDict.Add("sulki", 9);
                categoryFactorDict.Add("kłusaki", 9);
            }
            else
            {
                categoryFactorDict.Add("sulki", 2);
                categoryFactorDict.Add("kłusaki", 2);
            }
            if (raceModelProvider.Category == "steeple" || raceModelProvider.Category == "płoty")
            {
                categoryFactorDict.Add("steeple", 9);
                categoryFactorDict.Add("płoty", 9);
            }
            else
            {
                categoryFactorDict.Add("steeple", 2);
                categoryFactorDict.Add("płoty", 2);
            }
            categoryFactorDict.Add("-", 6);
            categoryFactorDict.Add(" ", 6);
            categoryFactorDict.Add("", 6);

            return categoryFactorDict;
        }

        [Fact]
        public void ResetComputeVariables_UpdatesProperties()
        {
            _serviceClass.DictValue = 10;
            _serviceClass.FinalResult = 10;
            _serviceClass.Result = 10;
            _serviceClass.SiblingIndex = 10;
            _serviceClass.Counter = 10;
            _serviceClass.ChildFromList = null;

            _serviceClass.ResetComputeVariables();

            Assert.Equal(1, _serviceClass.DictValue);
            Assert.Equal(0, _serviceClass.FinalResult);
            Assert.Equal(0, _serviceClass.Result);
            Assert.Equal(0, _serviceClass.SiblingIndex);
            Assert.Equal(0, _serviceClass.Counter);
            Assert.NotNull(_serviceClass.ChildFromList);
        }

        [Fact]
        public void ResetLoopVariables_UpdatesProperties()
        {
            _serviceClass.PlaceFactor = 10;
            _serviceClass.DistanceRaceIndex = 10;
            _serviceClass.DistanceFactor = 10;

            _serviceClass.ResetLoopVariables();

            Assert.Equal(0, _serviceClass.PlaceFactor);
            Assert.Equal(0, _serviceClass.DistanceRaceIndex);
            Assert.Equal(0, _serviceClass.DistanceFactor);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(4, 1)]
        [InlineData(5, 1)]
        [InlineData(6, 1.6)]
        [InlineData(7, 1.9)]
        [InlineData(8, 2.2)]
        [InlineData(9, 2.5)]
        [InlineData(10, 2.8)]
        public void ComputeAgeIndex_CalledWithVariousAge_CalculatesAIndex(int inputAge, double expectedAgeIndex)
        {
            LoadedHorse horse = new LoadedHorse()
            {
                Age = inputAge
            };

            double ageIndex = _serviceClass.ComputeAgeIndex(horse, DateTime.Now);

            Assert.Equal(expectedAgeIndex, ageIndex, 1); //precision attribute - 1
        }

        [Fact]
        public void ComputeCategoryIndex_Called_CalculatesCategoryIndex()
        {
            LoadedHorse _horse = new LoadedHorse();
            _raceCategoryDictionary = GetCategoryDictionary(_raceModelProvider);

            double categoryIndex = _serviceClass.ComputeCategoryIndex(_horse, DateTime.Now, _raceModelProvider, _raceCategoryDictionary);

            Assert.Equal(1, categoryIndex);
        }
    }
}
