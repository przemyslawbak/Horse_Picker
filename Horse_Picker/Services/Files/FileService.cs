﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Horse_Picker.Models;
using Newtonsoft.Json;

namespace Horse_Picker.Services.Files
{
    //JsonConvert async CPU bound https://stackoverflow.com/a/19912382/11027921 , https://stackoverflow.com/q/49610328/11027921
    //async file writing credits: https://stackoverflow.com/a/25521636
    public class FileService : IFileService
    {
        string _horsesFileName = "_horses_list.json";
        string _jockeysFileName = "_jockeys_list.json";
        string _racesFileName = "_historic_races.json";
        string _testsFileName = "_test_results.txt";

        public async Task<List<LoadedHorse>> GetAllHorsesAsync()
        {
            string json = "";
            List<LoadedHorse> _allHorses = new List<LoadedHorse>();

            if (File.Exists(_horsesFileName))
            {
                using (StreamReader r = new StreamReader(_horsesFileName))
                {
                    json = await r.ReadToEndAsync();
                }

                await Task.Run(() =>
                {
                    _allHorses = JsonConvert.DeserializeObject<List<LoadedHorse>>(json);
                });
            }

            return _allHorses;
        }

        public async Task<List<LoadedJockey>> GetAllJockeysAsync()
        {
            string json = "";
            List<LoadedJockey> _allJockeys = new List<LoadedJockey>();

            if (File.Exists(_jockeysFileName))
            {
                using (StreamReader r = new StreamReader(_jockeysFileName))
                {
                    json = await r.ReadToEndAsync();
                }

                await Task.Run(() =>
                {
                    _allJockeys = JsonConvert.DeserializeObject<List<LoadedJockey>>(json);
                });
            }

            return _allJockeys;
        }

        public async Task<List<RaceDetails>> GetAllRacesAsync()
        {
            string json = "";
            List<RaceDetails> _allRaces = new List<RaceDetails>();

            if (File.Exists(_racesFileName))
            {
                using (StreamReader r = new StreamReader(_racesFileName))
                {
                    json = await r.ReadToEndAsync();
                }

                await Task.Run(() =>
                {
                    _allRaces = JsonConvert.DeserializeObject<List<RaceDetails>>(json);
                });
            }

            return _allRaces;
        }

        public async Task SaveAllHorsesAsync(List<LoadedHorse> allHorses)
        {
            string json = "";
            List<LoadedHorse> _allHorses = new List<LoadedHorse>();

            await Task.Run(() =>
            {
                if (allHorses.Count != 0)
                {
                    json = JsonConvert.SerializeObject(allHorses);
                }
            });

            var buffer = Encoding.UTF8.GetBytes(json);

            using (var fileStream = new FileStream(_horsesFileName, FileMode.OpenOrCreate,
        FileAccess.Write, FileShare.None, buffer.Length, true))
            {
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        public async Task SaveAllJockeysAsync(List<LoadedJockey> allJockeys)
        {
            string json = "";
            List<LoadedJockey> _allJockeys = new List<LoadedJockey>();

            await Task.Run(() =>
            {
                if (allJockeys.Count != 0)
                {
                    json = JsonConvert.SerializeObject(allJockeys);
                }
            });

            var buffer = Encoding.UTF8.GetBytes(json);

            using (var fileStream = new FileStream(_jockeysFileName, FileMode.OpenOrCreate,
        FileAccess.Write, FileShare.None, buffer.Length, true))
            {
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        public async Task SaveAllRaces(List<RaceDetails> allRaces)
        {
            string json = "";
            List<RaceDetails> _allRaces = new List<RaceDetails>();

            await Task.Run(() =>
            {
                if (allRaces.Count != 0)
                {
                    json = JsonConvert.SerializeObject(allRaces);
                }
            });

            var buffer = Encoding.UTF8.GetBytes(json);

            using (var fileStream = new FileStream(_racesFileName, FileMode.OpenOrCreate,
        FileAccess.Write, FileShare.None, buffer.Length, true))
            {
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }
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
                            StringBuilder sbLine = new StringBuilder();

                            sbLine.Append(allRaces[j].HorseList[h].HorseName);
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].Jockey);
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].WinIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].JockeyIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].CategoryIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].SiblingsIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].AgeIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].PercentageIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].RestIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].TiredIndex.ToString("0.000"));
                            sbLine.Append(" - ");
                            sbLine.Append(allRaces[j].HorseList[h].HorseScore.ToString("0.000"));

                            line = sbLine.ToString();

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
