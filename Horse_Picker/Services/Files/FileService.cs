using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Horse_Picker.Models;
using Newtonsoft.Json;

namespace Horse_Picker.Services.Files
{
    //JsonConvert async CPU bound https://stackoverflow.com/a/19912382/11027921 , https://stackoverflow.com/q/49610328/11027921
    public class FileService : IFileService
    {
        string _horsesFileName = "_horses_list.json";
        string _jockeysFileName = "_jockeys_list.json";
        string _racesFileName = "_historic_races.json";
        string _testsFileName = "_test_results.txt";

        public async Task<List<LoadedHorse>> GetAllHorses()
        {
            List<LoadedHorse> _allHorses = new List<LoadedHorse>();
            await Task.Run(() =>
            {
                _allHorses = new List<LoadedHorse>();
                if (File.Exists(_horsesFileName))
                {

                    using (StreamReader r = new StreamReader(_horsesFileName))
                    {
                        string json = r.ReadToEnd();
                        _allHorses = JsonConvert.DeserializeObject<List<LoadedHorse>>(json);
                    }
                }
            });

            return _allHorses;
        }

        public async Task<List<LoadedJockey>> GetAllJockeys()
        {
            List<LoadedJockey> _allJockeys = new List<LoadedJockey>();
            await Task.Run(() =>
            {
                _allJockeys = new List<LoadedJockey>();
                if (File.Exists(_jockeysFileName))
                {
                    using (StreamReader r = new StreamReader(_jockeysFileName))
                    {
                        string json = r.ReadToEnd();
                        _allJockeys = JsonConvert.DeserializeObject<List<LoadedJockey>>(json);
                    }
                }
            });

            return _allJockeys;
        }

        public async Task<List<RaceDetails>> GetAllRaces()
        {
            List<RaceDetails> _allRaces = new List<RaceDetails>();
            await Task.Run(() =>
            {
                _allRaces = new List<RaceDetails>();
                if (File.Exists(_racesFileName))
                {
                    using (StreamReader r = new StreamReader(_racesFileName))
                    {
                        string json = r.ReadToEnd();
                        _allRaces = JsonConvert.DeserializeObject<List<RaceDetails>>(json);
                    }
                }
            });

            return _allRaces;
        }

        public async Task SaveAllHorsesAsync(List<LoadedHorse> allHorses)
        {
            await Task.Run(() =>
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
                        //
                    }
                }
            });
        }

        public async Task SaveAllJockeysAsync(List<LoadedJockey> allJockeys)
        {
            await Task.Run(() =>
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
                        //
                    }
                }
            });
        }

        public async Task SaveAllRaces(List<RaceDetails> allRaces)
        {
            await Task.Run(() =>
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
                        //
                    }
                }
            });
        }

        public async Task SaveRaceSimulatedResultsAsync(List<RaceDetails> allRaces)
        {
            string line;
            if (allRaces.Count != 0)
            {
                if (File.Exists(_testsFileName)) File.Delete(_testsFileName);

                try
                {
                    for (int j = 0; j < allRaces.Count; j++)
                    {
                        line = "";
                        await SaveTestResultLine(line);
                        line = allRaces[j].RaceDate + " - " + allRaces[j].RaceDistance + " - " + allRaces[j].RaceCategory;
                        await SaveTestResultLine(line);
                        line = "Name - Jockey - WI - JI - CI - SI - AI - PI - RI - TI - Score";
                        await SaveTestResultLine(line);

                        for (int h = 0; h < allRaces[j].HorseList.Count; h++)
                        {
                            line = allRaces[j].HorseList[h].HorseName + " - "
                                + allRaces[j].HorseList[h].Jockey + " - "
                                + allRaces[j].HorseList[h].WinIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].JockeyIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].CategoryIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].SiblingsIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].AgeIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].PercentageIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].RestIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].TiredIndex.ToString("0.000") + " - "
                                + allRaces[j].HorseList[h].HorseScore.ToString("0.000");
                            await SaveTestResultLine(line);
                        }

                    }
                }
                catch (Exception e)
                {
                    //
                }
            }
        }

        async Task SaveTestResultLine(string line)
        {
            using (TextWriter csvLineBuilder = new StreamWriter(_testsFileName, true))
            {
                await csvLineBuilder.WriteLineAsync(line);
            }
        }
    }
}
