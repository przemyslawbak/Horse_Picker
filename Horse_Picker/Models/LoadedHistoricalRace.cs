using Horse_Picker.Models;
using Horse_Picker.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.NewModels
{
    public class LoadedHistoricalRace
    {
        public DateTime RaceDate { get; set; } //
        public string RaceLink { get; set; } //
        public int RaceDistance { get; set; } //
        public string RaceCategory { get; set; }
        public List<HorseDataWrapper> HorseList { get; set; }

    }
}
