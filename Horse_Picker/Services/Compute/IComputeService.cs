using Horse_Picker.Models;
using System;
using System.Collections.Generic;

namespace Horse_Picker.Services.Compute
{
    public interface IComputeService
    {
        double ComputeTiredIndex(LoadedHorse horseFromList, DateTime date);

        double ComputeRestIndex(LoadedHorse horseFromList, DateTime date);

        double ComputePercentageIndex(LoadedHorse horseFromList, DateTime date);

        double ComputeAgeIndex(LoadedHorse horseFromList, DateTime date);

        double ComputeCategoryIndex(LoadedHorse horseFromList,
            DateTime date, RaceModel raceServices,
            Dictionary<string, int> racecategoryDictionary);

        double ComputeWinIndex(LoadedHorse horseFromList,
            DateTime date, LoadedJockey jockeyFromList,
            RaceModel raceServices,
            Dictionary<string, int> racecategoryDictionary);
        double ComputeJockeyIndex(LoadedJockey jockeyFromList, DateTime date);

        double ComputeSiblingsIndex(LoadedHorse fatherFromList,
            DateTime date, RaceModel raceServices,
            List<LoadedHorse> horses,
            Dictionary<string, int> raceCategoryDictionary);
    }
}