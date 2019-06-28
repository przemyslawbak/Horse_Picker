using System;
using System.Threading.Tasks;
using Horse_Picker.Models;

namespace Horse_Picker.Services.Scrap
{
    public interface IScrapService
    {
        Task<LoadedHorse> ScrapSingleHorsePlAsync(int index);
        Task<LoadedHorse> ScrapSingleHorseCzAsync(int index);
        Task<LoadedHistoricalRace> ScrapSingleRacePlAsync(int index);
        Task<T> ScrapGenericObject<T>(int id, string jobType);
    }
}
