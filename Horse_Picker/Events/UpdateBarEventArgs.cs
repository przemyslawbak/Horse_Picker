using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Events
{
    //ListEventArgs credits: https://stackoverflow.com/a/14058441/11027921
    public class UpdateBarEventArgs : EventArgs
    {
        public string JobType { get; set; }
        public int LoopCouner { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public UpdateBarEventArgs(string jobType, int loopCounter, int toId, int fromId)
        {
            JobType = jobType;
            LoopCouner = loopCounter;
            FromId = fromId;
            ToId = toId;
        }
    }
}
