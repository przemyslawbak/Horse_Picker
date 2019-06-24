using Horse_Picker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Services
{
    public class RaceProvider : IRaceProvider
    {
        RaceModel _raceModel = new RaceModel();
        public DateTime RaceDate { get => _raceModel.RaceDate; set => _raceModel.RaceDate = value; }
        public string RaceNo { get => _raceModel.RaceNo; set => _raceModel.RaceNo = value; }
        public string City { get => _raceModel.City; set => _raceModel.City = value; }
        public string Category { get => _raceModel.Category; set => _raceModel.Category = value; }
        public string Distance { get => _raceModel.Distance; set => _raceModel.Distance = value; }
        public bool IsButtonVisible { get => _raceModel.IsButtonVisible; set => _raceModel.IsButtonVisible = value; }
    }
}
