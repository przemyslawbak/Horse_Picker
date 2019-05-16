using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.NewModels
{
    public class JockeyRaceDetails
    {
        public DateTime RaceDate { get; set; }//
        public int RaceDistance { get; set; }//
        public string HorsesName { get; set; }//
        public int WonPlace { get; set; }//
        public int RaceCompetition { get; set; }//

        //overriding Equals for List Union
        public override bool Equals(object obj)
        {
            // Try to cast the object to compare to to be a Person
            var raceDetails = obj as JockeyRaceDetails;

            return Equals(raceDetails);
        }

        public override int GetHashCode()
        {
            return RaceDate.GetHashCode();
        }

        public bool Equals(JockeyRaceDetails raceToCompareTo)
        {
            // Check if person is being compared to a non person. In that case always return false.
            if (raceToCompareTo == null) return false;

            // Check if both person objects contain the same Name. In that case they're assumed equal.
            return RaceDate.Equals(raceToCompareTo.RaceDate) && RaceDistance.Equals(raceToCompareTo.RaceDistance) && WonPlace.Equals(raceToCompareTo.WonPlace);
        }
    }
}
