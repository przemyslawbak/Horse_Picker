using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horse_Picker.Models;
using Horse_Picker.NewModels;

namespace Horse_Picker.DataProvider
{
    public interface IScrapDataServices
    {
        Task<LoadedHorse> ScrapSingleHorsePlAsync(int index);
        Task<LoadedHorse> ScrapSingleHorseCzAsync(int index);
        Task<LoadedJockey> ScrapSingleJockeyPlAsync(int index);
        Task<LoadedJockey> ScrapSingleJockeyCzAsync(int index);
        Task<LoadedHistoricalRace> ScrapSingleRacePlAsync(int index);
    }
}
