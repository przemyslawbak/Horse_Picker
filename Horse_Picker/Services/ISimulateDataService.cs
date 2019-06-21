using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Horse_Picker.Events;
using Horse_Picker.Models;

namespace Horse_Picker.Services
{
    public interface ISimulateDataService
    {
        event EventHandler<UpdateBarEventArgs> _simulateProgressEventHandler;

        Task<ObservableCollection<LoadedHistoricalRace>> SimulateResultsAsync(int fromId,
            int toId,
            ObservableCollection<LoadedHistoricalRace> races,
            ObservableCollection<LoadedHorse> horses,
            ObservableCollection<LoadedJockey> jockeys,
            IRaceModelProvider _raceModelProvider);
        void CancelUpdates();
    }
}