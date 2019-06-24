using Horse_Picker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Horse_Picker.Services.Compute
{
    public interface IComputeService
    {
        double ComputeTiredIndex(LoadedHorse horseFromList, DateTime date);

        double ComputeRestIndex(LoadedHorse horseFromList, DateTime date);

        double ComputePercentageIndex(LoadedHorse horseFromList, DateTime date);

        double ComputeAgeIndex(LoadedHorse horseFromList, DateTime date);

        double ComputeCategoryIndex(LoadedHorse horseFromList,
            DateTime date, IRaceProvider raceServices,
            Dictionary<string, int> racecategoryDictionary);

        double ComputeWinIndex(LoadedHorse horseFromList,
            DateTime date, LoadedJockey jockeyFromList,
            IRaceProvider raceServices,
            Dictionary<string, int> racecategoryDictionary);
        double ComputeJockeyIndex(LoadedJockey jockeyFromList, DateTime date);

        double ComputeSiblingsIndex(LoadedHorse fatherFromList,
            DateTime date, IRaceProvider raceServices,
            ObservableCollection<LoadedHorse> horses,
            Dictionary<string, int> raceCategoryDictionary);
    }
}
