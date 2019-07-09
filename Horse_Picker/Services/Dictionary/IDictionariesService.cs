using Horse_Picker.Models;
using System.Collections.Generic;

namespace Horse_Picker.Services.Dictionary
{
    public interface IDictionariesService
    {
        Dictionary<string, string> GetJockeyPlNodeDictionary();

        Dictionary<string, string> GetJockeyCzNodeDictionary();

        Dictionary<string, string> GetHorsePlNodeDictionary();

        Dictionary<string, string> GetHorseCzNodeDictionary();

        Dictionary<string, string> GetRacePlNodeDictionary();

        Dictionary<string, int> GetMonthDictionary();

        Dictionary<string, int> GetRaceCategoryDictionary(RaceModel raceModelProvider);


    }
}