using Horse_Picker.Commands;
using Horse_Picker.Models;
using Horse_Picker.Services;
using Horse_Picker.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Horse_Picker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        //private IEventAggregator _eventAggregator; //Prism
        private IMessageDialogService _messageDialogService;
        private IComputeDataServices _computeDataService;
        private IFileDataServices _dataServices;
        private IScrapDataServices _scrapServices;
        private IRaceModelProvider _raceModelProvider;
        private UpdateModules _updateModulesModel;
        int _degreeOfParallelism;

        public MainViewModel(IFileDataServices dataServices,
            IScrapDataServices scrapServices,
            IMessageDialogService messageDialogServices,
            IComputeDataServices computeDataService,
            IRaceModelProvider raceServices)
        {
            Horses = new ObservableCollection<LoadedHorse>();
            Jockeys = new ObservableCollection<LoadedJockey>();
            Races = new ObservableCollection<LoadedHistoricalRace>();
            LoadedHorses = new List<string>();
            LoadedJockeys = new List<string>();

            //_eventAggregator = eventAggregator; //prism events
            _dataServices = dataServices;
            _raceModelProvider = raceServices;
            _computeDataService = computeDataService;
            _scrapServices = scrapServices;
            _messageDialogService = messageDialogServices;
            HorseList = new ObservableCollection<HorseDataWrapper>();
            _updateModulesModel = new UpdateModules();
            _degreeOfParallelism = 100;

            HorseList.Clear();
            Category = "fill up";
            City = "-";
            Distance = "0";
            RaceNo = "0";
            RaceDate = DateTime.Now;

            TaskCancellation = false;
            AllControlsEnabled = true;
            VisibilityStatusBar = Visibility.Hidden;
            VisibilityCancellingMsg = Visibility.Collapsed;
            VisibilityCancelTestingBtn = Visibility.Collapsed;
            VisibilityCancelUpdatingBtn = Visibility.Collapsed;
            VisibilityUpdatingBtn = Visibility.Visible;
            VisibilityTestingBtn = Visibility.Visible;

            NewHorseCommand = new DelegateCommand(OnNewHorseExecute);
            ClearDataCommand = new DelegateCommand(OnClearDataExecute);
            TaskCancellationCommand = new DelegateCommand(OnTaskCancellationExecute);
            TestResultsCommand = new AsyncCommand(async () => await OnTestResultExecuteAsync());
            PickHorseDataCommand = new DelegateCommand(OnPickHorseDataExecute);
            UpdateDataCommand = new AsyncCommand(async () => await OnUpdateDataExecuteAsync());

            LoadAllData();
            PopulateLists();
            CategoryFactorDict = _computeDataService.GetRaceCategoryDictionary(_raceModelProvider);

            HorseList.CollectionChanged += OnHorseListCollectionChanged;
        }

        /// <summary>
        /// on `Test results` btn click,
        /// is testing strategy on historic results
        /// </summary>
        /// <returns></returns>
        public async Task OnTestResultExecuteAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;

            await TestHistoricalResultsAsync();

            stopwatch.Stop();
            MessageBox.Show(stopwatch.Elapsed.ToString());
        }

        /// <summary>
        /// on `Find` horse btn click,
        /// on `AutoComplete` of the horse name / jockey TextBoxes,
        /// is parsing horse with horses from data files
        /// </summary>
        /// <param name="obj"></param>
        public void OnPickHorseDataExecute(object obj)
        {
            HorseWrapper = (HorseDataWrapper)obj;
            DateTime date = DateTime.Now;
            Task.Run(() => HorseWrapper = ParseHorseData(HorseWrapper, date)); //consumes time
        }

        /// <summary>
        /// on `Update data` btn click,
        /// is scraping data from www
        /// </summary>
        /// <returns></returns>
        public async Task OnUpdateDataExecuteAsync()
        {
            UpdateModules = new ObservableCollection<bool>();

            UpdateHorsesCz = false;
            UpdateHorsesPl = false;
            UpdateJockeysCz = false;
            UpdateJockeysPl = false;
            UpdateRacesPl = false;

            //default values
            JPlFrom = 1;
            JPlTo = 1049;
            JCzFrom = 4000;
            JCzTo = 31049;
            HPlFrom = 1;
            HPlTo = 25049;
            HCzFrom = 8000;
            HCzTo = 150049;
            HistPlFrom = 1;
            HistPlTo = 17049;


            var result = _messageDialogService.ShowUpdateWindow();

            UpdateModules.Add(UpdateHorsesCz);
            UpdateModules.Add(UpdateHorsesPl);
            UpdateModules.Add(UpdateJockeysCz);
            UpdateModules.Add(UpdateJockeysPl);
            UpdateModules.Add(UpdateRacesPl);

            bool isAnyTrue = UpdateModules.Any(module => module == true);

            if (result == MessageDialogResult.Update && isAnyTrue)
            {
                var stopwatch = Stopwatch.StartNew();
                TokenSource = new CancellationTokenSource();
                CancellationToken = TokenSource.Token;

                if (UpdateJockeysPl) await ScrapJockeys(JPlFrom, JPlTo + 1, "jockeysPl"); //1 - 1049
                if (UpdateJockeysCz) await ScrapJockeys(JCzFrom, JCzTo + 1, "jockeysCz"); //4000 - 31049
                if (UpdateHorsesPl) await ScrapHorses(HPlFrom, HPlTo + 1, "horsesPl"); //1 - 25049
                if (UpdateHorsesCz) await ScrapHorses(HCzFrom, HCzTo + 1, "horsesCz"); // 8000 - 150049
                if (UpdateRacesPl) await ScrapHistoricalRaces(HistPlFrom, HistPlTo + 1, "racesPl"); // 1 - 17049

                stopwatch.Stop();
                MessageBox.Show(stopwatch.Elapsed.ToString());

                PopulateLists();
            }
        }

        /// <summary>
        /// on `cancel` btn click,
        /// is canceling TokenSource for every single Task,
        /// `TaskCancellation` property is breaking every single loop in Tasks
        /// </summary>
        /// <param name="obj"></param>
        public void OnTaskCancellationExecute(object obj)
        {
            TaskCancellation = true;

            TokenSource.Cancel();

            CommandCompletedControlsSetup();
        }

        /// <summary>
        /// on `Clear race` btn click,
        /// resets all rece properties,
        /// clears list of horses for the race
        /// </summary>
        /// <param name="obj"></param>
        public void OnClearDataExecute(object obj)
        {
            HorseList.Clear();
            Category = "fill up";
            City = "-";
            Distance = "0";
            RaceNo = "0";
            RaceDate = DateTime.Now;
        }

        /// <summary>
        /// on `Add new horse (+)` btn click,
        /// is adding new blank horse to the list
        /// </summary>
        /// <param name="obj"></param>
        public void OnNewHorseExecute(object obj)
        {
            HorseWrapper = new HorseDataWrapper();
            HorseList.Add(HorseWrapper);
        }

        /// <summary>
        /// loads horses from the file data services
        /// </summary>
        public void LoadAllData()
        {
            Horses.Clear();
            Jockeys.Clear();
            Races.Clear();

            foreach (var horse in _dataServices.GetAllHorses())
            {
                Horses.Add(new LoadedHorse
                {
                    Name = horse.Name,
                    Age = horse.Age,
                    AllRaces = horse.AllRaces,
                    AllChildren = horse.AllChildren,
                    Father = horse.Father,
                    Link = horse.Link,
                    FatherLink = horse.FatherLink
                });
            }

            foreach (var jockey in _dataServices.GetAllJockeys())
            {
                Jockeys.Add(new LoadedJockey
                {
                    Name = jockey.Name,
                    AllRaces = jockey.AllRaces,
                    Link = jockey.Link
                });
            }

            foreach (var race in _dataServices.GetAllRaces())
            {
                Races.Add(new LoadedHistoricalRace
                {
                    RaceCategory = race.RaceCategory,
                    RaceDate = race.RaceDate,
                    RaceDistance = race.RaceDistance,
                    RaceLink = race.RaceLink,
                    HorseList = race.HorseList
                });
            }
        }

        /// <summary>
        /// populates LoadedHorses and LoadedJockeys for AutoCompleteBox binding
        /// </summary>
        public void PopulateLists()
        {
            LoadedHorses.Clear();
            LoadedJockeys.Clear();

            for (int i = 0; i < Horses.Count; i++)
            {
                string theName = MakeTitleCase(Horses[i].Name);
                if (theName != "")
                {
                    LoadedHorses.Add(theName + ", " + Horses[i].Age.ToString());
                }
            }
            for (int i = 0; i < Jockeys.Count; i++)
            {
                if (Jockeys[i].Name != "")
                {
                    LoadedJockeys.Add(Jockeys[i].Name);
                }
            }
        }

        /// <summary>
        /// makes title case for horse name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MakeTitleCase(string name)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name) && name != "--Not found--")
            {
                TextInfo myCI = new CultureInfo("en-US", false).TextInfo; //creates CI
                name = name.ToLower().Trim(' '); //takes to lower, to take to TC later
                name = myCI.ToTitleCase(name); //takes to TC
            }
            else
            {
                return "";
            }

            return name;
        }

        /// <summary>
        /// on horse collection change validates buttons again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnHorseListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateButtons();
        }

        //commands
        public ICommand NewHorseCommand { get; private set; }
        public ICommand ClearDataCommand { get; private set; }
        public ICommand TaskCancellationCommand { get; private set; }
        public IAsyncCommand TestResultsCommand { get; private set; }
        public IAsyncCommand UpdateDataCommand { get; private set; }
        public ICommand PickHorseDataCommand { get; private set; }

        //properties
        public ObservableCollection<HorseDataWrapper> HorseList { get; set; }
        public ObservableCollection<bool> UpdateModules { get; private set; }
        public HorseDataWrapper HorseWrapper { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public CancellationTokenSource TokenSource { get; set; }
        public ObservableCollection<LoadedHorse> Horses { get; private set; }
        public ObservableCollection<LoadedJockey> Jockeys { get; private set; }
        public ObservableCollection<LoadedHistoricalRace> Races { get; private set; }
        public List<string> LoadedHorses { get; }
        public List<string> LoadedJockeys { get; }
        public Dictionary<string, int> CategoryFactorDict { get; set; }

        //prop for scrap PL jockeys from ID int
        public int JPlFrom
        {
            get
            {
                return _updateModulesModel.JPlFrom;
            }
            set
            {
                _updateModulesModel.JPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL jockeys to ID int
        public int JPlTo
        {
            get
            {
                return _updateModulesModel.JPlTo;
            }
            set
            {
                _updateModulesModel.JPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ jockeys from ID int
        public int JCzFrom
        {
            get
            {
                return _updateModulesModel.JCzFrom;
            }
            set
            {
                _updateModulesModel.JCzFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ jockeys to ID int
        public int JCzTo
        {
            get
            {
                return _updateModulesModel.JCzTo;
            }
            set
            {
                _updateModulesModel.JCzTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL horses from ID int
        public int HPlFrom
        {
            get
            {
                return _updateModulesModel.HPlFrom;
            }
            set
            {
                _updateModulesModel.HPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL horses to ID int
        public int HPlTo
        {
            get
            {
                return _updateModulesModel.HPlTo;
            }
            set
            {
                _updateModulesModel.HPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ horses from ID int
        public int HCzFrom
        {
            get
            {
                return _updateModulesModel.HCzFrom;
            }
            set
            {
                _updateModulesModel.HCzFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ horses to ID int
        public int HCzTo
        {
            get
            {
                return _updateModulesModel.HCzTo;
            }
            set
            {
                _updateModulesModel.HCzTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL historic races from ID int
        public int HistPlFrom
        {
            get
            {
                return _updateModulesModel.HistPlFrom;
            }
            set
            {
                _updateModulesModel.HistPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL historic races to ID int
        public int HistPlTo
        {
            get
            {
                return _updateModulesModel.HistPlTo;
            }
            set
            {
                _updateModulesModel.HistPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for update jockeys PL checkbox
        public bool UpdateJockeysPl
        {
            get
            {
                return _updateModulesModel.JockeysPl;
            }
            set
            {
                _updateModulesModel.JockeysPl = value;
                OnPropertyChanged();
            }
        }

        //prop for update jockeys CZ checkbox
        public bool UpdateJockeysCz
        {
            get
            {
                return _updateModulesModel.JockeysCz;
            }
            set
            {
                _updateModulesModel.JockeysCz = value;
                OnPropertyChanged();
            }
        }

        //prop for update horses CZ checkbox
        public bool UpdateHorsesCz
        {
            get
            {
                return _updateModulesModel.HorsesCz;
            }
            set
            {
                _updateModulesModel.HorsesCz = value;
                OnPropertyChanged();
            }
        }

        //prop for update horses PL checkbox
        public bool UpdateHorsesPl
        {
            get
            {
                return _updateModulesModel.HorsesPl;
            }
            set
            {
                _updateModulesModel.HorsesPl = value;
                OnPropertyChanged();
            }
        }

        //prop for update historic data PL checkbox
        public bool UpdateRacesPl
        {
            get
            {
                return _updateModulesModel.RacesPl;
            }
            set
            {
                _updateModulesModel.RacesPl = value;
                OnPropertyChanged();
            }
        }

        //prop race distance
        public string Distance
        {
            get
            {
                return _raceModelProvider.Distance;
            }
            set
            {
                _raceModelProvider.Distance = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop race category
        public string Category
        {
            get
            {
                return _raceModelProvider.Category;
            }
            set
            {
                _raceModelProvider.Category = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop race city
        public string City
        {
            get
            {
                return _raceModelProvider.City;
            }
            set
            {
                _raceModelProvider.City = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop race No
        public string RaceNo
        {
            get
            {
                return _raceModelProvider.RaceNo;
            }
            set
            {
                _raceModelProvider.RaceNo = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop date of the race
        public DateTime RaceDate
        {
            get
            {
                return _raceModelProvider.RaceDate;
            }
            set
            {
                _raceModelProvider.RaceDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// IsEnabled prop for some controls during data updates / tests
        /// </summary>
        private bool _allControlsEnabled;
        public bool AllControlsEnabled
        {
            get
            {
                return _allControlsEnabled;
            }
            set
            {
                _allControlsEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// `Add Horse` btn IsEnabled prop
        /// </summary>
        private bool _isNewHorseEnabled;
        public bool IsNewHorseEnabled
        {
            get
            {
                return _isNewHorseEnabled;
            }
            set
            {
                _isNewHorseEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// `Save` btn IsEnabled prop
        /// </summary>
        private bool _isSaveEnabled;
        public bool IsSaveEnabled
        {
            get
            {
                return _isSaveEnabled;
            }
            set
            {
                _isSaveEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// `ProgressBar` Value
        /// </summary>
        private int _updateStatusBar;
        public int UpdateStatusBar
        {
            get
            {
                return _updateStatusBar;
            }
            set
            {
                _updateStatusBar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// displayed numeric progress, ex. 371 / 18049
        /// </summary>
        private string _progressDisplay;
        public string ProgressDisplay
        {
            get
            {
                return _progressDisplay;
            }
            set
            {
                _progressDisplay = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// name of the work (update/test) currently done
        /// </summary>
        private string _workStatus;
        public string WorkStatus
        {
            get
            {
                return _workStatus;
            }
            set
            {
                _workStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// validates buttons being enabled
        /// </summary>
        private void ValidateButtons()
        {
            int n;
            bool isDistanceCorrect = int.TryParse(Distance, out n);

            //if distance is numeric
            if (isDistanceCorrect)
            {
                if (int.Parse(Distance) < 100) isDistanceCorrect = false; //check if is greater than 100m
            }
            bool isRaceNoNumeric = int.TryParse(Distance, out n);

            //for `Add new horse` btn
            if (City == "-" || !isDistanceCorrect || !isRaceNoNumeric || Category == "fill up" || RaceNo == "0")
            {
                IsNewHorseEnabled = false;
            }
            else
            {
                IsNewHorseEnabled = true;
            }
            if (AllControlsEnabled == false)
            {
                IsNewHorseEnabled = false;
            }

            //for `Save race` btn
            if (HorseList != null && HorseList.Count > 0 && IsNewHorseEnabled)
            {
                IsSaveEnabled = true;
            }
            else
            {
                IsSaveEnabled = false;
            }
            if (AllControlsEnabled == false)
            {
                IsSaveEnabled = false;
            }
        }

        /// <summary>
        /// display `status bar` or not?
        /// </summary>
        private Visibility _visibilityStatusBar;
        public Visibility VisibilityStatusBar
        {
            get
            {
                return _visibilityStatusBar;
            }
            set
            {
                _visibilityStatusBar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// display `cancel updates` btn or not
        /// </summary>
        private Visibility _visibilityCancelUpdatesBtn;
        public Visibility VisibilityCancelUpdatingBtn
        {
            get
            {
                return _visibilityCancelUpdatesBtn;
            }
            set
            {
                _visibilityCancelUpdatesBtn = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// display `cancel tests` btn or not
        /// </summary>
        private Visibility _visibilityCancelTestingBtn;
        public Visibility VisibilityCancelTestingBtn
        {
            get
            {
                return _visibilityCancelTestingBtn;
            }
            set
            {
                _visibilityCancelTestingBtn = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// display `tests` btn or not
        /// </summary>
        private Visibility _visibilityTestingBtn;
        public Visibility VisibilityTestingBtn
        {
            get
            {
                return _visibilityTestingBtn;
            }
            set
            {
                _visibilityTestingBtn = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// display `tests` btn or not
        /// </summary>
        private Visibility _visibilityUpdatingBtn;
        public Visibility VisibilityUpdatingBtn
        {
            get
            {
                return _visibilityUpdatingBtn;
            }
            set
            {
                _visibilityUpdatingBtn = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// display `cancel tests` btn or not
        /// </summary>
        private Visibility _visibilityCancellingMsg;
        public Visibility VisibilityCancellingMsg
        {
            get
            {
                return _visibilityCancellingMsg;
            }
            set
            {
                _visibilityCancellingMsg = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// for update/test cancellation
        /// </summary>
        private bool _taskCancellation;
        public bool TaskCancellation
        {
            get
            {
                return _taskCancellation;
            }
            set
            {
                _taskCancellation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// parses the horse from Horses with providen data
        /// </summary>
        /// <param name="horseWrapper">horse data</param>
        /// <param name="date">day of the race</param>
        /// <returns></returns>
        public HorseDataWrapper ParseHorseData(HorseDataWrapper horseWrapper, DateTime date)
        {
            LoadedJockey jockeyFromList = new LoadedJockey();
            LoadedHorse horseFromList = new LoadedHorse();
            LoadedHorse fatherFromList = new LoadedHorse();
            //if name is entered
            if (!string.IsNullOrEmpty(horseWrapper.HorseName))
            {
                Horses = new ObservableCollection<LoadedHorse>(Horses.OrderBy(l => l.Age)); //from smallest to biggest

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
                    horseFromList = Horses
                    .Where(h => h.Name.ToLower() == horseWrapper.HorseName.ToLower())
                    .FirstOrDefault();
                }

                //if age is written
                else
                {
                    horseFromList = Horses
                    .Where(h => h.Name.ToLower() == horseWrapper.HorseName.ToLower())
                    .Where(h => h.Age == horseWrapper.Age)
                    .FirstOrDefault();
                }

                //case when previous horse is replaced in Horse collection item
                if (horseFromList == null)
                {
                    horseWrapper.Age = 0;

                    horseFromList = Horses
                    .Where(i => i.Name.ToLower() == horseWrapper
                    .HorseName.ToLower())
                    .FirstOrDefault();
                }

                Horses = new ObservableCollection<LoadedHorse>(Horses.OrderByDescending(l => l.Age)); //from biggest to smallest

                //jockey index
                if (!string.IsNullOrEmpty(horseWrapper.Jockey))
                {
                    jockeyFromList = Jockeys
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
                        horseWrapper.WinIndex = _computeDataService.ComputeWinIndex(horseFromList, date, jockeyFromList, _raceModelProvider);
                    else
                        horseWrapper.WinIndex = _computeDataService.ComputeWinIndex(horseFromList, date, null, _raceModelProvider);

                    //category index
                    horseWrapper.CategoryIndex = _computeDataService.ComputeCategoryIndex(horseFromList, date, _raceModelProvider);

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
                    fatherFromList = Horses.Where(i => i.Name.ToLower() == horseWrapper.Father.ToLower()).FirstOrDefault();

                    if (fatherFromList != null)
                    {
                        horseWrapper.SiblingsIndex = _computeDataService.ComputeSiblingsIndex(fatherFromList, date, _raceModelProvider, Horses);
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

        //SemaphoreSlim credits: https://blog.briandrupieski.com/throttling-asynchronous-methods-in-csharp
        //SemaphoreSlim corrections: https://stackoverflow.com/questions/56640694/why-my-code-is-throwing-the-semaphore-has-been-disposed-exception/
        //Task.Run corrections: https://stackoverflow.com/questions/56628009/how-should-i-use-task-run-in-my-code-for-proper-scalability-and-performance/

        /// <summary>
        /// parsing race stats with every single horse from all historic races
        /// </summary>
        /// <returns></returns>
        public async Task TestHistoricalResultsAsync()
        {
            //init values and controls
            CommandStartedControlsSetup("TestResultsCommand");

            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            ProgressBarTick("Testing on historic data", loopCounter, Races.Count, 0);
            //for all races in the file
            for (int i = 0; i < Races.Count; i++)
            {
                int j = i;

                if (TaskCancellation == true)
                {
                    break;
                }

                await throttler.WaitAsync(TokenSource.Token);

                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        CancellationToken.ThrowIfCancellationRequested();

                        //if the race is from 2018
                        if (Races[j].RaceDate.Year == 2018)
                        {
                            Category = Races[j].RaceCategory;
                            Distance = Races[j].RaceDistance.ToString();

                            //for all horses in the race
                            for (int h = 0; h < Races[j].HorseList.Count; h++)
                            {
                                if (TaskCancellation == true)
                                {
                                    break;
                                }
                                CancellationToken.ThrowIfCancellationRequested();
                                HorseDataWrapper horse = new HorseDataWrapper();
                                horse = ParseHorseData(Races[j].HorseList[h], Races[j].RaceDate);
                                Races[j].HorseList[h] = horse; //get all indexes
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //
                    }
                    finally
                    {
                        loopCounter++;

                        ProgressBarTick("Testing on historic data", loopCounter, Races.Count, 0);

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
                //
            }
            finally
            {
                await _dataServices.SaveRaceTestResultsAsync(Races.ToList()); //save the analysis to the file

                AllControlsEnabled = true;

                CommandCompletedControlsSetup();

                VisibilityCancellingMsg = Visibility.Collapsed;

                throttler.Dispose();
            }
        }

        /// <summary>
        /// scraps historic races
        /// </summary>
        /// <param name="startIndex">page to start loop</param>
        /// <param name="stopIndex">page to finish loop</param>
        /// <param name="dataType">what site to use</param>
        /// <returns></returns>
        public async Task ScrapHistoricalRaces(int startIndex, int stopIndex, string dataType)
        {
            //init values and controls
            Races.Clear(); //method does not remove doublers
            CommandStartedControlsSetup("UpdateDataCommand");

            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            ProgressBarTick("Looking for historic races", loopCounter, stopIndex, startIndex);

            for (int i = startIndex; i < stopIndex; i++)
            {
                int j = i;

                if (TaskCancellation == true)
                {
                    break;
                }

                await throttler.WaitAsync(TokenSource.Token);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        LoadedHistoricalRace race = new LoadedHistoricalRace();

                        CancellationToken.ThrowIfCancellationRequested();

                        if (dataType == "racesPl") race = await _scrapServices.ScrapSingleRacePlAsync(j);

                        //if the race is from 2018
                        if (race.RaceDate.Year == 2018)
                        {
                            lock (((ICollection)Races).SyncRoot)
                            {
                                Races.Add(race);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //
                    }
                    finally
                    {
                        loopCounter++;

                        ProgressBarTick("Looking for historic data", loopCounter, stopIndex, startIndex);

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
                //
            }
            finally
            {
                _dataServices.SaveAllRaces(Races.ToList()); //saves everything to JSON file

                AllControlsEnabled = true;

                CommandCompletedControlsSetup();

                VisibilityCancellingMsg = Visibility.Collapsed;

                throttler.Dispose();
            }
        }

        /// <summary>
        /// scraps jockey data from the website
        /// </summary>
        /// <param name="startIndex">page to start loop</param>
        /// <param name="stopIndex">page to finish loop</param>
        /// <param name="dataType">what site to use</param>
        /// <returns></returns>
        public async Task ScrapJockeys(int startIndex, int stopIndex, string dataType)
        {
            //init values and controls
            CommandStartedControlsSetup("UpdateDataCommand");

            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            ProgressBarTick("Looking for jockeys", loopCounter, stopIndex, startIndex);

            for (int i = startIndex; i < stopIndex; i++)
            {
                int j = i;

                if (TaskCancellation == true)
                {
                    break;
                }

                await throttler.WaitAsync(TokenSource.Token);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        LoadedJockey jockey = new LoadedJockey();

                        CancellationToken.ThrowIfCancellationRequested();

                        if (dataType == "jockeysPl") jockey = await _scrapServices.ScrapSingleJockeyPlAsync(j);
                        if (dataType == "jockeysCz") jockey = await _scrapServices.ScrapSingleJockeyCzAsync(j);

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
                    catch (Exception e)
                    {
                        //
                    }
                    finally
                    {
                        loopCounter++;

                        if (loopCounter % 1000 == 0)
                        {
                            await _dataServices.SaveAllJockeysAsync(Jockeys.ToList());
                        }

                        ProgressBarTick("Looking for jockeys", loopCounter, stopIndex, startIndex);

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
                //
            }
            finally
            {
                await _dataServices.SaveAllJockeysAsync(Jockeys.ToList()); //saves everything to JSON file

                AllControlsEnabled = true;

                CommandCompletedControlsSetup();

                VisibilityCancellingMsg = Visibility.Collapsed;

                throttler.Dispose();
            }
        }

        /// <summary>
        /// updates list of jockey races, if found some new
        /// </summary>
        /// <param name="doubledJockey">jockey for Jockeys</param>
        /// <param name="jockey">found doubler</param>
        public void MergeJockeysData(LoadedJockey doubledJockey, LoadedJockey jockey)
        {
            doubledJockey.AllRaces = doubledJockey.AllRaces.Union(jockey.AllRaces).ToList();
            Jockeys.Add(doubledJockey);
        }

        /// <summary>
        /// scraps horse data from the website
        /// </summary>
        /// <param name="startIndex">page to start loop</param>
        /// <param name="stopIndex">page to finish loop</param>
        /// <param name="dataType">what site to use</param>
        /// <returns></returns>
        public async Task ScrapHorses(int startIndex, int stopIndex, string dataType)
        {
            //init values and controls
            CommandStartedControlsSetup("UpdateDataCommand");

            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            ProgressBarTick("Looking for horses", loopCounter, stopIndex, startIndex);

            for (int i = startIndex; i < stopIndex; i++)
            {
                int j = i;

                if (TaskCancellation == true)
                {
                    break;
                }

                await throttler.WaitAsync(TokenSource.Token);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        LoadedHorse horse = new LoadedHorse();

                        CancellationToken.ThrowIfCancellationRequested();

                        if (dataType == "horsesPl") horse = await _scrapServices.ScrapSingleHorsePlAsync(j);
                        if (dataType == "horsesCz") horse = await _scrapServices.ScrapSingleHorseCzAsync(j);

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
                    catch (Exception e)
                    {
                        //
                    }
                    finally
                    {
                        loopCounter++;

                        //saves all every 1000 records, just in case
                        if (loopCounter % 1000 == 0)
                        {
                            await _dataServices.SaveAllHorsesAsync(Horses.ToList());
                        }

                        ProgressBarTick("Looking for horses", loopCounter, stopIndex, startIndex);

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
                //
            }
            finally
            {
                await _dataServices.SaveAllHorsesAsync(Horses.ToList()); //saves everything to JSON file

                AllControlsEnabled = true;

                CommandCompletedControlsSetup();

                VisibilityCancellingMsg = Visibility.Collapsed;

                throttler.Dispose();
            }
        }

        /// <summary>
        /// updates data on progress bar for any task
        /// </summary>
        /// <param name="workStatus">name of commenced work</param>
        /// <param name="loopCounter">current counter in the loop</param>
        /// <param name="stopIndex">when loop finishes</param>
        /// <param name="startIndex">when loop starts</param>
        public void ProgressBarTick(string workStatus, int loopCounter, int stopIndex, int startIndex)
        {
            WorkStatus = workStatus;
            UpdateStatusBar = loopCounter * 100 / (stopIndex - startIndex);
            ProgressDisplay = loopCounter + " / " + (stopIndex - startIndex);
        }

        /// <summary>
        /// updates list of horses races and children, if found some new
        /// </summary>
        /// <param name="doubledHorse">horse from Horses</param>
        /// <param name="horse">scrapped new horse</param>
        public void MergeHorsesData(LoadedHorse doubledHorse, LoadedHorse horse)
        {
            doubledHorse.AllChildren = doubledHorse.AllChildren.Union(horse.AllChildren).ToList();
            doubledHorse.AllRaces = doubledHorse.AllRaces.Union(horse.AllRaces).ToList();
            Horses.Add(doubledHorse);
        }

        /// <summary>
        /// changes some display props on starting long running tasks
        /// </summary>
        /// <param name="command"></param>
        public void CommandStartedControlsSetup(string command)
        {
            TaskCancellation = false;
            AllControlsEnabled = false;
            ValidateButtons();
            VisibilityStatusBar = Visibility.Visible;
            UpdateStatusBar = 0;

            if (command == "TestResultsCommand")
            {
                VisibilityCancelTestingBtn = Visibility.Visible;
                VisibilityTestingBtn = Visibility.Collapsed;
            }

            if (command == "UpdateDataCommand")
            {
                VisibilityCancelUpdatingBtn = Visibility.Visible;
                VisibilityUpdatingBtn = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// changes some display props on stopped long running tasks
        /// </summary>
        public void CommandCompletedControlsSetup()
        {
            //TokenSource.Dispose();
            TaskCancellation = false;
            UpdateStatusBar = 0;
            VisibilityStatusBar = Visibility.Hidden;
            ValidateButtons();
            ProgressDisplay = "";
            WorkStatus = "";
            VisibilityCancellingMsg = Visibility.Visible;
            VisibilityCancelTestingBtn = Visibility.Collapsed;
            VisibilityTestingBtn = Visibility.Visible;
            VisibilityCancelUpdatingBtn = Visibility.Collapsed;
            VisibilityUpdatingBtn = Visibility.Visible;
        }
    }
}
