using Horse_Picker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Horse_Picker.Services
{
    public interface IComputeDataService
    {
        double ComputeTiredIndex(LoadedHorse horseFromList, DateTime date);
        double ComputeRestIndex(LoadedHorse horseFromList, DateTime date);
        double ComputePercentageIndex(LoadedHorse horseFromList, DateTime date);
        double ComputeAgeIndex(LoadedHorse horseFromList, DateTime date);
        double ComputeCategoryIndex(LoadedHorse horseFromList, DateTime date, IRaceModelProvider raceServices);
        double ComputeWinIndex(LoadedHorse horseFromList, DateTime date, LoadedJockey jockeyFromList, IRaceModelProvider raceServices);
        double ComputeJockeyIndex(LoadedJockey jockeyFromList, DateTime date);
        double ComputeSiblingsIndex(LoadedHorse fatherFromList, DateTime date, IRaceModelProvider raceServices, ObservableCollection<LoadedHorse> horses);
        Dictionary<string, int> GetRaceCategoryDictionary(IRaceModelProvider raceModelProvider);
    }
}
