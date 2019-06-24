using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Horse_Picker.Events;
using Horse_Picker.Models;

namespace Horse_Picker.Services.Simulate
{
    public interface ISimulateService
    {
        event EventHandler<UpdateBarEventArgs> _simulateProgressEventHandler;

        Task<ObservableCollection<LoadedHistoricalRace>> SimulateResultsAsync(int fromId,
            int toId,
            ObservableCollection<LoadedHistoricalRace> races,
            ObservableCollection<LoadedHorse> horses,
            ObservableCollection<LoadedJockey> jockeys,
            IRaceProvider _raceModelProvider);
        void CancelUpdates();
    }
}