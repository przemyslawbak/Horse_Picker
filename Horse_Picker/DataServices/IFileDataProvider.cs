using Horse_Picker.Models;
using Horse_Picker.NewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.DataProvider
{
    public interface IFileDataServices
    {
        List<LoadedHorse> GetAllHorses();
        Task SaveAllHorsesAsync(List<LoadedHorse> horses);
        Task SaveAllJockeysAsync(List<LoadedJockey> allJockeys);
        List<LoadedJockey> GetAllJockeys();
        void SaveAllRaces(List<LoadedHistoricalRace> list);
        List<LoadedHistoricalRace> GetAllRaces();
        Task SaveRaceTestResultsAsync(List<LoadedHistoricalRace> allRaces);
    }
}
