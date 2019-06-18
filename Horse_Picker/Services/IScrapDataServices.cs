using System.Threading.Tasks;
using Horse_Picker.Models;

namespace Horse_Picker.Services
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
