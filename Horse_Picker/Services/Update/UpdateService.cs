using Horse_Picker.Events;
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

namespace Horse_Picker.Services.Update
{
    //update progress bar question: https://stackoverflow.com/questions/56690258/is-there-any-way-that-my-service-layer-will-update-vm-properties-or-methods-res

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
            _degreeOfParallelism = 10;

            _dataServices = dataServices;
            _dictionaryService = dictionaryService;
            _scrapDataService = scrapDataService;
            _computeDataService = computeDataService;

            Horses = new ObservableCollection<LoadedHorse>();
            Jockeys = new ObservableCollection<LoadedJockey>();
            Races = new ObservableCollection<LoadedHistoricalRace>();
        }

        public ObservableCollection<LoadedHorse> Horses { get; private set; }
        public ObservableCollection<LoadedJockey> Jockeys { get; private set; }
        public ObservableCollection<LoadedHistoricalRace> Races { get; private set; }
        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public event EventHandler<UpdateBarEventArgs> _updateProgressEventHandler;

        //SemaphoreSlim credits: https://blog.briandrupieski.com/throttling-asynchronous-methods-in-csharp
        //SemaphoreSlim corrections: https://stackoverflow.com/questions/56640694/why-my-code-is-throwing-the-semaphore-has-been-disposed-exception/
        //Task.Run corrections: https://stackoverflow.com/questions/56628009/how-should-i-use-task-run-in-my-code-for-proper-scalability-and-performance/

        public async Task<ObservableCollection<T>> UpdateDataAsync<T>(ObservableCollection<T> genericCollection, int idFrom, int idTo, string jobType)
        {
            //variables
            _loopCounterProgressBar = 0;
            _idFromProgressBar = idFrom;
            _idToProgressBar = idTo;

            //parse collections, display job
            if (typeof(T) == typeof(LoadedHorse))
            {
                Horses.Clear();
                Horses = new ObservableCollection<LoadedHorse>(genericCollection.Cast<LoadedHorse>());
                _jobTypeProgressBar = "Updating horse data";
            }
            else if (typeof(T) == typeof(LoadedJockey))
            {
                Jockeys.Clear();
                Jockeys = new ObservableCollection<LoadedJockey>(genericCollection.Cast<LoadedJockey>());
                _jobTypeProgressBar = "Updating jockey data";
            }
            else if (typeof(T) == typeof(LoadedHistoricalRace))
            {
                Races.Clear();
                Races = new ObservableCollection<LoadedHistoricalRace>(genericCollection.Cast<LoadedHistoricalRace>());
                _jobTypeProgressBar = "Updating historic data";
            }

            //initial
            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;
            OnProgressBarTick();

            //run loop
            for (int i = idFrom; i < idTo; i++)
            {
                int id = i;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        if (CancellationToken.IsCancellationRequested)
                            return;

                        await throttler.WaitAsync(TokenSource.Token);

                        if (jobType.Contains("Horses"))
                        {
                            await UpdateHorsesAsync(jobType, id);
                        }
                        else if (jobType.Contains("Jockeys"))
                        {
                            await UpdateJockeysAsync(jobType, id);
                        }
                        else if (jobType.Contains("Historic"))
                        {
                            await UpdateRacesAsync(jobType, id);
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {
                        _loopCounterProgressBar++;

                        EventHandler<UpdateBarEventArgs> progressBarTick = _updateProgressEventHandler;

                        OnProgressBarTick();

                        throttler.Release();
                    }
                }));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {

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
                else if (typeof(T) == typeof(LoadedHistoricalRace))
                {
                    if (jobType.Contains("Historic"))
                    {
                        _dataServices.SaveAllRaces(Races.ToList());
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
            else if (typeof(T) == typeof(LoadedHistoricalRace))
            {
                return (ObservableCollection<T>)Convert.ChangeType(Races, typeof(ObservableCollection<T>));
            }
            else { throw new ArgumentException(); }
        }

        protected void OnProgressBarTick()
        {
            _updateProgressEventHandler(this, new UpdateBarEventArgs(_jobTypeProgressBar, _loopCounterProgressBar, _idToProgressBar, _idFromProgressBar));
        }

        private async Task UpdateRacesAsync(string jobType, int id)
        {
            LoadedHistoricalRace race = new LoadedHistoricalRace();

            if (jobType == "updateHistoricPl") race = await _scrapDataService.ScrapSingleRacePlAsync(id);

            //if the race is from 2018
            if (race.RaceDate.Year == 2018)
            {
                lock (((ICollection)Races).SyncRoot)
                {
                    Races.Add(race);
                }
            }
        }

        private async Task UpdateJockeysAsync(string jobType, int id)
        {
            LoadedJockey jockey = new LoadedJockey();

            jockey = await _scrapDataService.ScrapGenericObject<LoadedJockey>(id, jobType);

            if (jockey.Name != null)
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

        private async Task UpdateHorsesAsync(string jobType, int id)
        {
            LoadedHorse horse = new LoadedHorse();

            if (jobType == "updateHorsesPl") horse = await _scrapDataService.ScrapSingleHorsePlAsync(id);
            if (jobType == "updateHorsesCz") horse = await _scrapDataService.ScrapSingleHorseCzAsync(id);

            if (horse.Name != null)
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
        public ObservableCollection<LoadedJockey> MergeJockeysData(LoadedJockey doubledJockey, LoadedJockey jockey)
        {
            if (jockey.AllRaces != null)
            {
                doubledJockey.AllRaces = doubledJockey.AllRaces.Union(jockey.AllRaces).ToList();
            }

            Jockeys.Add(doubledJockey);

            return Jockeys;
        }

        /// <summary>
        /// updates list of horses races and children, if found some new
        /// </summary>
        /// <param name="doubledHorse">horse from Horses</param>
        /// <param name="horse">scrapped new horse</param>
        public ObservableCollection<LoadedHorse> MergeHorsesData(LoadedHorse doubledHorse, LoadedHorse horse)
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

            return Horses;
        }

        //passing RaceModel to the services credits: https://stackoverflow.com/questions/56646346/should-i-pass-view-model-to-my-service-and-if-yes-how-to-do-it/

        /// <summary>
        /// parses the horse from Horses with providen data
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

        public void CancelUpdates()
        {
            TokenSource.Cancel();
        }
    }
}
