using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Models
{
    public class LoadedHorse
    {
        public string Name { get; set; } //
        public int Age { get; set; } //
        public string Link { get; set; } //
        public string Father { get; set; } //
        public string FatherLink { get; set; } //
        public List<RaceDetails> AllRaces { get; set; }
        public List<HorseChildDetails> AllChildren { get; set; }
    }
}
