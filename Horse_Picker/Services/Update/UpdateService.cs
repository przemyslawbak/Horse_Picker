﻿using Horse_Picker.Events;
using Horse_Picker.Models;
using Horse_Picker.Services.Compute;
using Horse_Picker.Services.Dictionary;
using Horse_Picker.Services.Files;
using Horse_Picker.Services.Scrap;
using Horse_Picker.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Horse_Picker.Services.Update
{
    //update progress bar question: https://stackoverflow.com/questions/56690258/is-there-any-way-that-my-service-layer-will-update-vm-properties-or-methods-res
    /// <summary>
    /// SemaphoreSlim performance test:
    /// UpdateJockeysAsync(PL) - all records
    /// _degreeOfParallelism = 10; -> 22m15s
    /// _degreeOfParallelism = 30; -> 7m41s
    /// _degreeOfParallelism = 50; -> 6m12s
    /// _degreeOfParallelism = 70; -> 4m49s
    /// _degreeOfParallelism = 85; -> 5m20s (many conn. exceptions, try with better net)
    /// _degreeOfParallelism = 100; -> 5m50s (many conn. exceptions, try with better net)
    /// </summary>
    public class UpdateService : IUpdateService
    {
        int _idToProgressBar;
        int _idFromProgressBar;
        int _loopCounterProgressBar;
        string _jobTypeProgressBar;
        int _degreeOfParallelism;
        private IScrapService _scrapDataService;
        private IFileService _dataServices;
        private IComputeService _computeDataService;
        private IDictionariesService _dictionaryService;

        public UpdateService(IFileService dataServices,
            IScrapService scrapDataService,
            IComputeService computeDataService,
            IDictionariesService dictionaryService)
        {
            _degreeOfParallelism = 40;

            _dataServices = dataServices;
            _dictionaryService = dictionaryService;
            _scrapDataService = scrapDataService;
            _computeDataService = computeDataService;

            Horses = new ObservableCollection<LoadedHorse>();
            Jockeys = new ObservableCollection<LoadedJockey>();
            Races = new ObservableCollection<RaceDetails>();
        }

        public ObservableCollection<LoadedHorse> Horses { get; private set; }
        public ObservableCollection<LoadedJockey> Jockeys { get; private set; }
        public ObservableCollection<RaceDetails> Races { get; private set; }
        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public event EventHandler<UpdateBarEventArgs> UpdateProgressEventHandler;

        /// <summary>
        /// updates collections of generic type
        /// I/O operations
        /// creates loads of parallel tasks to improve performance speed
        /// use of SemaphoreSlim to control tasks
        /// SemaphoreSlim credits: https://blog.briandrupieski.com/throttling-asynchronous-methods-in-csharp
        /// SemaphoreSlim corrections: https://stackoverflow.com/a/56641448/11027921
        /// Task.Run corrections: https://stackoverflow.com/a/56631208/11027921
        /// credits for SemaphoreSlim cancellation: https://stackoverflow.com/a/24099764/11027921
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="genericCollection">collection parameter</param>
        /// <param name="idFrom">object id limitation</param>
        /// <param name="idTo">object id limitation</param>
        /// <param name="jobType">type of scrapping</param>
        /// <returns></returns>
        public async Task<ObservableCollection<T>> UpdateDataAsync<T>(ObservableCollection<T> genericCollection, int idFrom, int idTo, string jobType)
        {
            //variables
            _loopCounterProgressBar = 0;
            _idFromProgressBar = idFrom;
            _idToProgressBar = idTo;

            //parse collections, display job
            if (typeof(T) == typeof(LoadedHorse))
            {
                Horses = new ObservableCollection<LoadedHorse>(genericCollection.Cast<LoadedHorse>());
                _jobTypeProgressBar = "Updating horse data";
            }
            else if (typeof(T) == typeof(LoadedJockey))
            {
                Jockeys = new ObservableCollection<LoadedJockey>(genericCollection.Cast<LoadedJockey>());
                _jobTypeProgressBar = "Updating jockey data";
            }
            else if (typeof(T) == typeof(RaceDetails))
            {
                Races = new ObservableCollection<RaceDetails>(genericCollection.Cast<RaceDetails>());
                _jobTypeProgressBar = "Updating historic data";
            }

            //initial
            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;
            OnProgressBarTick();

            //run loop
            for (int i = idFrom; i < idTo + 1; i++)
            {
                int id = i;

                if (CancellationToken.IsCancellationRequested)
                    break;

                //create loads of parallel tasks
                tasks.Add(Task.Run(async () =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        if (CancellationToken.IsCancellationRequested)
                            return;

                        if (typeof(T) == typeof(LoadedHorse))
                        {
                            await UpdateHorsesAsync(jobType, id);
                        }
                        else if (typeof(T) == typeof(LoadedJockey))
                        {
                            await UpdateJockeysAsync(jobType, id);
                        }
                        else if (typeof(T) == typeof(RaceDetails))
                        {
                            await UpdateRacesAsync(jobType, id);
                        }
                    }
                    catch
                    {
                        MessageBox.Show(id.ToString());
                    }
                    finally
                    {
                        _loopCounterProgressBar++;

                        EventHandler<UpdateBarEventArgs> progressBarTick = UpdateProgressEventHandler;

                        OnProgressBarTick();

                        throttler.Release();
                    }
                }));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            finally
            {
                //save when finish
                if (typeof(T) == typeof(LoadedJockey))
                {
                    await _dataServices.SaveAllJockeysAsync(Jockeys.ToList());
                }
                else if (typeof(T) == typeof(LoadedHorse))
                {
                    await _dataServices.SaveAllHorsesAsync(Horses.ToList());
                }
                else if (typeof(T) == typeof(RaceDetails))
                {
                    if (jobType.Contains("Historic"))
                    {
                        await _dataServices.SaveAllRaces(Races.ToList());
                    }
                    else if (jobType.Contains("testRaces"))
                    {
                        await _dataServices.SaveRaceSimulatedResultsAsync(Races.ToList());
                    }
                }
            }

            if (typeof(T) == typeof(LoadedHorse))
            {
                return (ObservableCollection<T>)Convert.ChangeType(Horses, typeof(ObservableCollection<T>));
            }
            else if (typeof(T) == typeof(LoadedJockey))
            {
                return (ObservableCollection<T>)Convert.ChangeType(Jockeys, typeof(ObservableCollection<T>));
            }
            else if (typeof(T) == typeof(RaceDetails))
            {
                return (ObservableCollection<T>)Convert.ChangeType(Races, typeof(ObservableCollection<T>));
            }
            else { throw new ArgumentException(); }
        }

        protected void OnProgressBarTick()
        {
            UpdateProgressEventHandler(this, new UpdateBarEventArgs(_jobTypeProgressBar, _loopCounterProgressBar, _idToProgressBar, _idFromProgressBar));
        }

        private async Task UpdateRacesAsync(string jobType, int id)
        {
            RaceDetails race = new RaceDetails();

            race = await _scrapDataService.ScrapGenericObject<RaceDetails>(id, jobType);

            //if the race is from 2018
            if (race != null)
            {
                if (race.RaceDate.Year == 2018)
                {
                    lock (((ICollection)Races).SyncRoot)
                    {
                        Races.Add(race);
                    }
                }
            }
        }

        /// <summary>
        /// updates single jockey object
        /// gets data from scrap service
        /// compares with objects in current database
        /// calls merge method if object already in database
        /// </summary>
        /// <param name="jobType">type of scrapping</param>
        /// <param name="id">id on the website</param>
        /// <returns></returns>
        private async Task UpdateJockeysAsync(string jobType, int id)
        {
            LoadedJockey jockey = new LoadedJockey();

            jockey = await _scrapDataService.ScrapGenericObject<LoadedJockey>(id, jobType);

            if (jockey != null)
            {
                lock (((ICollection)Jockeys).SyncRoot)
                {
                    //if objects are already in the List
                    if (Jockeys.Any(h => h.Name.ToLower() == jockey.Name.ToLower()))
                    {
                        LoadedJockey doubledJockey = Jockeys.Where(h => h.Name.ToLower() == jockey.Name.ToLower()).FirstOrDefault();
                        Jockeys.Remove(doubledJockey);
                        MergeJockeysData(doubledJockey, jockey);
                    }
                    else
                    {
                        Jockeys.Add(jockey);
                    }
                }
            }
        }

        /// <summary>
        /// updates single horse object
        /// gets data from scrap service
        /// compares with objects in current database
        /// calls merge method if object already in database
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task UpdateHorsesAsync(string jobType, int id)
        {
            LoadedHorse horse = new LoadedHorse();

            horse = await _scrapDataService.ScrapGenericObject<LoadedHorse>(id, jobType);

            if (horse != null)
            {
                lock (((ICollection)Horses).SyncRoot)
                {
                    //if objects are already in the List
                    if (Horses.Any(h => h.Name.ToLower() == horse.Name.ToLower()))
                    {
                        LoadedHorse doubledHorse = Horses.Where(h => h.Name.ToLower() == horse.Name.ToLower()).Where(h => h.Age == horse.Age).FirstOrDefault();
                        if (doubledHorse != null)
                        {
                            Horses.Remove(doubledHorse);
                            MergeHorsesData(doubledHorse, horse);
                        }
                        else
                        {
                            Horses.Add(horse);
                        }
                    }
                    else
                    {
                        Horses.Add(horse);
                    }
                }
            }
        }

        /// <summary>
        /// updates list of jockey races, if found some new
        /// </summary>
        /// <param name="doubledJockey">jockey for Jockeys</param>
        /// <param name="jockey">found doubler</param>
        public void MergeJockeysData(LoadedJockey doubledJockey, LoadedJockey jockey)
        {
            if (jockey.AllRaces != null)
            {
                doubledJockey.AllRaces = doubledJockey.AllRaces.Union(jockey.AllRaces).ToList();
            }

            Jockeys.Add(doubledJockey);
        }

        /// <summary>
        /// updates list of horses races and children, if found some new
        /// </summary>
        /// <param name="doubledHorse">horse from Horses</param>
        /// <param name="horse">scrapped new horse</param>
        public void MergeHorsesData(LoadedHorse doubledHorse, LoadedHorse horse)
        {
            if (horse.AllRaces != null)
            {
                doubledHorse.AllRaces = doubledHorse.AllRaces.Union(horse.AllRaces).ToList();
            }

            if (horse.AllChildren != null)
            {
                doubledHorse.AllChildren = doubledHorse.AllChildren.Union(horse.AllChildren).ToList();
            }

            Horses.Add(doubledHorse);
        }

        /// <summary>
        /// parses the horse from Horses with providen data
        /// passing RaceModel to the services credits: https://stackoverflow.com/a/56650635/11027921
        /// </summary>
        /// <param name="horseWrapper">horse data</param>
        /// <param name="date">day of the race</param>
        /// <returns></returns>
        public HorseDataWrapper GetParsedHorseData(HorseDataWrapper horseWrapper,
            DateTime date, ObservableCollection<LoadedHorse> horses,
            ObservableCollection<LoadedJockey> jockeys,
            IRaceProvider raceModelProvider)
        {
            Dictionary<string, int> raceCategoryDictionary = _dictionaryService.GetRaceCategoryDictionary(raceModelProvider);
            LoadedJockey jockeyFromList = new LoadedJockey();
            LoadedHorse horseFromList = new LoadedHorse();
            LoadedHorse fatherFromList = new LoadedHorse();

            //if name is entered
            if (!string.IsNullOrEmpty(horseWrapper.HorseName))
            {
                horses = new ObservableCollection<LoadedHorse>(horses.OrderBy(l => l.Age)); //from smallest to biggest

                //if age is from AutoCompleteBox
                if (horseWrapper.HorseName.Contains(", "))
                {
                    string theAge = horseWrapper.HorseName.Split(',')[1].Trim(' ');
                    string theName = horseWrapper.HorseName.Split(',')[0].Trim(' ');

                    int n;

                    bool parseTest = int.TryParse(theAge, out n);
                    if (parseTest)
                    {
                        horseWrapper.Age = int.Parse(theAge); //age
                    }
                    horseWrapper.HorseName = theName;
                }

                //if age is not written
                if (horseWrapper.Age == 0)
                {
                    horseFromList = horses
                    .Where(h => h.Name.ToLower() == horseWrapper.HorseName.ToLower())
                    .FirstOrDefault();
                }

                //if age is written
                else
                {
                    horseFromList = horses
                    .Where(h => h.Name.ToLower() == horseWrapper.HorseName.ToLower())
                    .Where(h => h.Age == horseWrapper.Age)
                    .FirstOrDefault();
                }

                //case when previous horse is replaced in Horse collection item
                if (horseFromList == null)
                {
                    horseWrapper.Age = 0;

                    horseFromList = horses
                    .Where(i => i.Name.ToLower() == horseWrapper
                    .HorseName.ToLower())
                    .FirstOrDefault();
                }

                horses = new ObservableCollection<LoadedHorse>(horses.OrderByDescending(l => l.Age)); //from biggest to smallest

                //jockey index
                if (!string.IsNullOrEmpty(horseWrapper.Jockey))
                {
                    jockeyFromList = jockeys
                    .Where(i => i.Name.ToLower() == horseWrapper
                    .Jockey.ToLower())
                    .FirstOrDefault();

                    if (jockeyFromList != null)
                    {
                        horseWrapper.JockeyIndex = _computeDataService.ComputeJockeyIndex(jockeyFromList, date);
                    }
                    else
                    {
                        horseWrapper.Jockey = "--Not found--";
                        horseWrapper.JockeyIndex = 0; //clear
                    }
                }

                if (horseFromList != null)
                {
                    horseWrapper.HorseName = horseFromList.Name; //displayed name
                    horseWrapper.Age = horseFromList.Age; //displayed age
                    horseWrapper.Father = horseFromList.Father; //displayed father
                    horseWrapper.TotalRaces = horseFromList.AllRaces.Count(); //how many races
                    horseWrapper.Comments = "";

                    //win index
                    if (jockeyFromList != null)
                        horseWrapper.WinIndex = _computeDataService.ComputeWinIndex(horseFromList, date, jockeyFromList, raceModelProvider, raceCategoryDictionary);
                    else
                        horseWrapper.WinIndex = _computeDataService.ComputeWinIndex(horseFromList, date, null, raceModelProvider, raceCategoryDictionary);

                    //category index
                    horseWrapper.CategoryIndex = _computeDataService.ComputeCategoryIndex(horseFromList, date, raceModelProvider, raceCategoryDictionary);

                    //age index
                    horseWrapper.AgeIndex = _computeDataService.ComputeAgeIndex(horseFromList, date);

                    //tired index
                    horseWrapper.TiredIndex = _computeDataService.ComputeTiredIndex(horseFromList, date);

                    //win percentage
                    horseWrapper.PercentageIndex = _computeDataService.ComputePercentageIndex(horseFromList, date);

                    //days of rest
                    horseWrapper.RestIndex = _computeDataService.ComputeRestIndex(horseFromList, date);
                }
                else
                {
                    horseWrapper.Age = 0; //clear
                    horseWrapper.TotalRaces = 0; //clear
                    horseWrapper.Comments = "--Data missing--";
                    horseWrapper.WinIndex = 0; //clear
                    horseWrapper.CategoryIndex = 0; //clear
                }

                //siblings index
                if (!string.IsNullOrEmpty(horseWrapper.Father))
                {
                    fatherFromList = horses.Where(i => i.Name.ToLower() == horseWrapper.Father.ToLower()).FirstOrDefault();

                    if (fatherFromList != null)
                    {
                        horseWrapper.SiblingsIndex = _computeDataService.ComputeSiblingsIndex(fatherFromList, date, raceModelProvider, horses, raceCategoryDictionary);
                    }
                    else
                    {
                        horseWrapper.Father = "--Not found--";
                        horseWrapper.SiblingsIndex = 0; //clear
                    }
                }

                //comments = score
                horseWrapper.Comments = horseWrapper.HorseScore.ToString("0.000");
            }

            return horseWrapper;
        }

        /// <summary>
        /// cancellation token for updating
        /// </summary>
        public void CancelUpdates()
        {
            TokenSource.Cancel();
        }
    }
}
