using Horse_Picker.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Models
{
    //equals credits: https://stackoverflow.com/a/20701995/11027921
    public class RaceDetails
    {
        public DateTime RaceDate { get; set; }//
        public string RaceLink { get; set; }//
        public int RaceDistance { get; set; }//
        public string RaceCategory { get; set; }//
        public string RacersName { get; set; }//
        public string RacersLink { get; set; }//
        public int WonPlace { get; set; }//
        public int RaceCompetition { get; set; }//
        public string HorseName { get; set; }//
        public List<HorseDataWrapper> HorseList { get; set; }

        //overriding Equals for List Union
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            RaceDetails r = obj as RaceDetails;
            if (r == null)
                return false;

            return (RaceDate == r.RaceDate) && (RaceDistance == r.RaceDistance) && (WonPlace == r.WonPlace);
        }


        public override int GetHashCode()
        {
            return RaceDate.GetHashCode() ^ RaceDistance.GetHashCode() ^ WonPlace.GetHashCode();
        }

        public bool Equals(RaceDetails raceToCompareTo)
        {
            if ((object)raceToCompareTo == null)
                return false;

            return (RaceDate == raceToCompareTo.RaceDate) && (RaceDistance == raceToCompareTo.RaceDistance) && (WonPlace == raceToCompareTo.WonPlace);
        }
    }
}
