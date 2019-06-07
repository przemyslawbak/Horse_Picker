using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Models
{
    public class UpdateModules
    {
        public bool JockeysPl { get; set; }
        public bool JockeysCz { get; set; }
        public bool HorsesCz { get; set; }
        public bool HorsesPl { get; set; }
        public bool RacesPl { get; set; }
        public int JPlFrom { get; set; }
        public int JPlTo { get; set; }
        public int JCzTo { get; set; }
        public int JCzFrom { get; set; }
        public int HPlFrom { get; set; }
        public int HPlTo { get; set; }
        public int HCzTo { get; set; }
        public int HCzFrom { get; set; }
        public int HistPlFrom { get; set; }
        public int HistPlTo { get; set; }
    }
}
