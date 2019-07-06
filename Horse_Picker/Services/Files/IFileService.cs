using Horse_Picker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horse_Picker.Services.Files
{
    public interface IFileService
    {
        Task<List<LoadedHorse>> GetAllHorsesAsync();
        Task SaveAllHorsesAsync(List<LoadedHorse> horses);
        Task SaveAllJockeysAsync(List<LoadedJockey> allJockeys);
        Task<List<LoadedJockey>> GetAllJockeysAsync();
        Task SaveAllRaces(List<RaceDetails> list);
        Task<List<RaceDetails>> GetAllRacesAsync();
        Task SaveRaceSimulatedResultsAsync(List<RaceDetails> allRaces);
    }
}
