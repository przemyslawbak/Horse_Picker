using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Horse_Picker.Events;
using Horse_Picker.Models;

namespace Horse_Picker.Services.Simulate
{
    public interface ISimulateService
    {
        event EventHandler<UpdateBarEventArgs> SimulateProgressEventHandler;

        Task<ObservableCollection<RaceDetails>> SimulateResultsAsync(int fromId,
            int toId,
            ObservableCollection<RaceDetails> races,
            ObservableCollection<LoadedHorse> horses,
            ObservableCollection<LoadedJockey> jockeys,
            IRaceProvider _raceModelProvider);
        void CancelSimulation();
    }
}