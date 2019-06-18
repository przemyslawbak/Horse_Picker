using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Models
{
    public class LoadedJockey
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public List<JockeyRaceDetails> AllRaces { get; set; }
    }
}
