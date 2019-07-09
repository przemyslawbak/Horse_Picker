using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Horse_Picker.Events;
using Horse_Picker.Models;

namespace Horse_Picker.Services.Simulate
{
    public interface ISimulateService
    {
        Task<List<RaceDetails>> SimulateResultsAsync(int fromId,
            int toId,
            List<RaceDetails> races,
            List<LoadedHorse> horses,
            List<LoadedJockey> jockeys,
            RaceModel raceModelProvider);
        void CancelSimulation();
    }
}