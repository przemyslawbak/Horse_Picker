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
        LoadedHorse ScrapSingleHorsePL(int index);
        LoadedHorse ScrapSingleHorseCZ(int index);
        LoadedJockey ScrapSingleJockeyPL(int index);
        LoadedJockey ScrapSingleJockeyCZ(int index);
        LoadedHistoricalRace ScrapSingleRacePL(int index);
    }
}
