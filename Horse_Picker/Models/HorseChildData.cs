using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Models
{
    //equals credits: https://stackoverflow.com/a/20701995/11027921
    public class HorseChildDetails
    {
        public string ChildName { get; set; }
        public string ChildLink { get; set; }
        public int ChildAge { get; set; }

        public override bool Equals(object obj)
        {
            var childDetails = obj as HorseChildDetails;

            return Equals(childDetails);
        }

        public override int GetHashCode()
        {
            return ChildName.ToLower().GetHashCode();
        }

        public bool Equals(HorseChildDetails raceToCompareTo)
        {
            if (raceToCompareTo == null) return false;

            return ChildName.ToLower().Equals(raceToCompareTo.ChildName.ToLower());
        }
    }
}
