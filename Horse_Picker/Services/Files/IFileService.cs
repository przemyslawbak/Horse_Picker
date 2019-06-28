﻿using Horse_Picker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horse_Picker.Services.Files
{
    public interface IFileService
    {
        List<LoadedHorse> GetAllHorses();
        Task SaveAllHorsesAsync(List<LoadedHorse> horses);
        Task SaveAllJockeysAsync(List<LoadedJockey> allJockeys);
        List<LoadedJockey> GetAllJockeys();
        void SaveAllRaces(List<LoadedHistoricalRace> list);
        List<LoadedHistoricalRace> GetAllRaces();
        Task SaveRaceSimulatedResultsAsync(List<LoadedHistoricalRace> allRaces);
    }
}