using Horse_Picker.Models;
using Horse_Picker.Services.Compute;
using Horse_Picker.Services.Dictionary;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Horse_Picker.Tests.Services.Compute
{
    public class ComputeServiceTests
    {
        private ComputeService _serviceClass;

        public ComputeServiceTests()
        {
            _serviceClass = new ComputeService();
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
        [InlineData(6, 2.6)]
        [InlineData(7, 3.9)]
        [InlineData(8, 5.2)]
        [InlineData(9, 6.5)]
        [InlineData(10, 7.8)]
        public void ComputeAgeIndex_CalculatesAIndex(int inputAge, double expectedAgeIndex)
        {
            LoadedHorse horse = new LoadedHorse()
            {
                Age = inputAge
            };

            double ageIndex = _serviceClass.ComputeAgeIndex(horse, DateTime.Now);

            Assert.Equal(expectedAgeIndex, ageIndex, 1); //precision attribute - 1
        }


    }
}
