using Horse_Picker.Events;
using Horse_Picker.Models;
using Horse_Picker.Wrappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Horse_Picker.Services.Update
{
    public interface IUpdateService
    {
        HorseDataWrapper GetParsedHorseData(HorseDataWrapper horseWrapper,
            DateTime date, ObservableCollection<LoadedHorse> horses,
            ObservableCollection<LoadedJockey> jockeys,
            IRaceProvider raceModelProvider);

        Dictionary<string, int> GetRaceCategoryDictionary(IRaceProvider raceModelProvider);

        Task<ObservableCollection<T>> UpdateDataAsync<T>(ObservableCollection<T> genericCollection, int jPlFrom, int jPlTo, string v);

        void CancelUpdates();

        event EventHandler<UpdateBarEventArgs> _updateProgressEventHandler;
    }
}