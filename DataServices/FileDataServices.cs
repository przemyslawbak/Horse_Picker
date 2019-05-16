using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Horse_Picker.Dialogs;
using Horse_Picker.Models;
using Horse_Picker.NewModels;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;

namespace Horse_Picker.DataProvider
{
    public class FileDataServices : IFileDataServices
    {
        DialogBox dialog = new DialogBox();
        string _horsesFileName = "_horses_list.json";
        string _jockeysFileName = "_jockeys_list.json";
        string _racesFileName = "_historic_races.json";
        string _testsFileName = "_historic_tests.json";

        public List<LoadedHorse> GetAllHorses()
        {
            List<LoadedHorse> _allHorses = new List<LoadedHorse>();
            if (File.Exists(_horsesFileName))
            {

                using (StreamReader r = new StreamReader(_horsesFileName))
                {
                    string json = r.ReadToEnd();
                    _allHorses = JsonConvert.DeserializeObject<List<LoadedHorse>>(json);
                }
            }

            return _allHorses;
        }

        public List<LoadedJockey> GetAllJockeys()
        {
            List<LoadedJockey> _allJockeys = new List<LoadedJockey>();
            if (File.Exists(_jockeysFileName))
            {
                using (StreamReader r = new StreamReader(_jockeysFileName))
                {
                    string json = r.ReadToEnd();
                    _allJockeys = JsonConvert.DeserializeObject<List<LoadedJockey>>(json);
                }
            }

            return _allJockeys;
        }

        public List<LoadedHistoricalRace> GetAllRaces()
        {
            List<LoadedHistoricalRace> _allRaces = new List<LoadedHistoricalRace>();
            if (File.Exists(_racesFileName))
            {
                using (StreamReader r = new StreamReader(_racesFileName))
                {
                    string json = r.ReadToEnd();
                    _allRaces = JsonConvert.DeserializeObject<List<LoadedHistoricalRace>>(json);
                }
            }

            return _allRaces;
        }

        public void SaveAllHorses(List<LoadedHorse> allHorses)
        {
            if (allHorses.Count != 0)
            {
                try
                {
                    if (File.Exists(_horsesFileName)) File.Delete(_horsesFileName);

                    using (StreamWriter file = File.CreateText(_horsesFileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, allHorses);
                    }
                }
                catch (Exception e)
                {
                    dialog.ShowDialog("Could not save the results, " + e.ToString(), "Error");
                }
            }
        }

        public void SaveAllJockeys(List<LoadedJockey> allJockeys)
        {
            if (allJockeys.Count != 0)
            {
                if (File.Exists(_jockeysFileName)) File.Delete(_jockeysFileName);

                try
                {
                    using (StreamWriter file = File.CreateText(_jockeysFileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, allJockeys);
                    }
                }
                catch (Exception e)
                {
                    dialog.ShowDialog("Could not save the results, " + e.ToString(), "Error");
                }
            }
        }

        public void SaveAllRaces(List<LoadedHistoricalRace> allRaces)
        {
            if (allRaces.Count != 0)
            {
                if (File.Exists(_racesFileName)) File.Delete(_racesFileName);

                try
                {
                    using (StreamWriter file = File.CreateText(_racesFileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, allRaces);
                    }
                }
                catch (Exception e)
                {
                    dialog.ShowDialog("Could not save the results, " + e.ToString(), "Error");
                }
            }
        }

        public void SaveRaceTestResultsAsync(List<LoadedHistoricalRace> allRaces)
        {

            if (allRaces.Count != 0)
            {
                if (File.Exists(_testsFileName)) File.Delete(_testsFileName);

                try
                {
                    using (StreamWriter file = File.CreateText(_testsFileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, allRaces);
                    }
                }
                catch (Exception e)
                {
                    dialog.ShowDialog("Could not save the results, " + e.ToString(), "Error");
                }
            }
        }
    }
}
