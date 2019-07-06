using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Models
{
    public class ProgressBarData
    {
        public int ToId { get; internal set; }
        public int FromId { get; internal set; }
        public string JobType { get; internal set; }
        public int LoopCouner { get; internal set; }
    }
}
