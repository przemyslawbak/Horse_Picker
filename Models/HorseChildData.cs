using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Models
{
    public class HorseChildDetails
    {
        public string ChildName { get; set; }
        public string ChildLink { get; set; }
        public int ChildAge { get; set; }

        //overriding Equals for List Union
        public override bool Equals(object obj)
        {
            // Try to cast the object to compare to to be a Person
            var childDetails = obj as HorseChildDetails;

            return Equals(childDetails);
        }

        public override int GetHashCode()
        {
            return ChildName.ToLower().GetHashCode();
        }

        public bool Equals(HorseChildDetails raceToCompareTo)
        {
            // Check if person is being compared to a non person. In that case always return false.
            if (raceToCompareTo == null) return false;

            // Check if both person objects contain the same Name. In that case they're assumed equal.
            return ChildName.ToLower().Equals(raceToCompareTo.ChildName.ToLower());
        }
    }
}
