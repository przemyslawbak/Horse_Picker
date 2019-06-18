using Horse_Picker.Models;
using System;
using System.Collections.Generic;

namespace Horse_Picker.Services
{
    public interface IComputeDataServices
    {
        double ComputeTiredIndex(LoadedHorse horseFromList, DateTime date);
        double ComputeRestIndex(LoadedHorse horseFromList, DateTime date);
        double ComputePercentageIndex(LoadedHorse horseFromList, DateTime date);
        double ComputeAgeIndex(LoadedHorse horseFromList, DateTime date);
        double ComputeCategoryIndex(LoadedHorse horseFromList, DateTime date);
        double ComputeWinIndex(LoadedHorse horseFromList, DateTime date, LoadedJockey jockeyFromList);
        double ComputeJockeyIndex(LoadedJockey jockeyFromList, DateTime date);
        double ComputeSiblingsIndex(LoadedHorse fatherFromList, DateTime date);
        Dictionary<string, int> RaceCategoryDictionary { get; }
    }
}
