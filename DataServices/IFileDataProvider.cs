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
        void SaveAllHorses(List<LoadedHorse> horses);
        void SaveAllJockeys(List<LoadedJockey> allJockeys);
        List<LoadedJockey> GetAllJockeys();
        void SaveAllRaces(List<LoadedHistoricalRace> list);
        List<LoadedHistoricalRace> GetAllRaces();
        void SaveRaceTestResultsAsync(List<LoadedHistoricalRace> allRaces);
    }
}
