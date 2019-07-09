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
            DateTime date, List<LoadedHorse> horses,
            List<LoadedJockey> jockeys,
            RaceModel raceModelProvider);

        void CancelUpdates();

        Task<List<T>> UpdateDataAsync<T>(List<T> genericCollection, int jPlFrom, int jPlTo, string v);

    }
}