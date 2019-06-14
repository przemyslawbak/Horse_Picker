using Horse_Picker.Commands;
using Horse_Picker.DataProvider;
using Horse_Picker.Dialogs;
using Horse_Picker.Models;
using Horse_Picker.NewModels;
using Horse_Picker.Wrappers;
using Prism.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        private IFileDataServices _dataServices;
        private IScrapDataServices _scrapServices;
        private RaceData _raceDataModel;
        private UpdateModules _updateModulesModel;
        public MainViewModel(IFileDataServices dataServices, IScrapDataServices scrapServices, IMessageDialogService messageDialogService)
        {
            Horses = new ObservableCollection<LoadedHorse>();
            Jockeys = new ObservableCollection<LoadedJockey>();
            Races = new ObservableCollection<LoadedHistoricalRace>();

            _loadedHorses = new List<string>();
            _loadedJockeys = new List<string>();
            //_eventAggregator = eventAggregator; //prism events
            _dataServices = dataServices; //data files
            _scrapServices = scrapServices; //data scrap
            _messageDialogService = messageDialogService; //dialogs
            HorseList = new ObservableCollection<HorseDataWrapper>();
            _raceDataModel = new RaceData();
            _updateModulesModel = new UpdateModules();

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

            HorseList.CollectionChanged += HorseListCollectionChanged;
        }

        public async Task OnTestResultExecuteAsync()
        {
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;

            await TestHistoricalResultsAsync();
        }

        public void OnPickHorseDataExecute(object obj)
        {
            HorseWrapper = (HorseDataWrapper)obj;
            DateTime date = DateTime.Now;
            Task.Run(() => HorseWrapper = ParseHorseData(HorseWrapper, date)); //consumes time
        }

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

            //DODAC KONTEKST DLA OKNA
            UpdateModules.Add(UpdateHorsesCz);
            UpdateModules.Add(UpdateHorsesPl);
            UpdateModules.Add(UpdateJockeysCz);
            UpdateModules.Add(UpdateJockeysPl);
            UpdateModules.Add(UpdateRacesPl);

            bool isAnyTrue = UpdateModules.Any(module => module == true);

            if (result == MessageDialogResult.Update && isAnyTrue)
            {

                TokenSource = new CancellationTokenSource();
                CancellationToken = TokenSource.Token;

                if (UpdateJockeysPl) await ScrapJockeys(JPlFrom, JPlTo + 1, "jockeysPl"); //1 - 1049
                if (UpdateJockeysCz) await ScrapJockeys(JCzFrom, JCzTo + 1, "jockeysCz"); //4000 - 31049
                if (UpdateHorsesPl) await ScrapHorses(HPlFrom, HPlTo + 1, "horsesPl"); //1 - 25049
                if (UpdateHorsesCz) await ScrapHorses(HCzFrom, HCzTo + 1, "horsesCz"); // 8000 - 150049
                if (UpdateRacesPl) await ScrapHistoricalRaces(HistPlFrom, HistPlTo + 1, "racesPl"); // 1 - 17049
            }
        }

        public void OnTaskCancellationExecute(object obj)
        {
            TaskCancellation = true;

            TokenSource.Cancel();

            CommandCompletedControlsSetup();
        }

        public void OnClearDataExecute(object obj)
        {
            HorseList.Clear();
            Category = "fill up";
            City = "-";
            Distance = "0";
            RaceNo = "0";
            RaceDate = DateTime.Now;
        }

        public void OnNewHorseExecute(object obj)
        {
            HorseWrapper = new HorseDataWrapper();
            HorseList.Add(HorseWrapper);
        }

        /// <summary>
        /// loading horses from the list in web
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

        public ICommand NewHorseCommand { get; private set; }
        public ICommand ClearDataCommand { get; private set; }
        public ICommand TaskCancellationCommand { get; private set; }
        public IAsyncCommand TestResultsCommand { get; private set; }
        public IAsyncCommand UpdateDataCommand { get; private set; }
        public ICommand PickHorseDataCommand { get; private set; }

        public ObservableCollection<bool> UpdateModules { get; private set; }
        public HorseDataWrapper HorseWrapper { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public CancellationTokenSource TokenSource { get; private set; }
        public ObservableCollection<LoadedHorse> Horses { get; private set; }
        public ObservableCollection<LoadedJockey> Jockeys { get; private set; }
        public ObservableCollection<LoadedHistoricalRace> Races { get; private set; }

        /// <summary>
        /// list of horses for AutoCompleteBox binding
        /// </summary>
        private List<string> _loadedHorses;
        public List<string> LoadedHorses
        {
            get
            {
                _loadedHorses.Clear();
                for (int i = 0; i < Horses.Count; i++)
                {
                    string theName = MakeTitleCase(Horses[i].Name);
                    _loadedHorses.Add(theName + ", " + Horses[i].Age.ToString());
                }

                return _loadedHorses;
            }
        }

        /// <summary>
        /// list of jockeys for AutoCompleteBox binding
        /// </summary>
        private List<string> _loadedJockeys;
        public List<string> LoadedJockeys
        {
            get
            {
                _loadedJockeys.Clear();
                for (int i = 0; i < Jockeys.Count; i++)
                {
                    _loadedJockeys.Add(Jockeys[i].Name);
                }

                return _loadedJockeys;
            }
        }

        private string MakeTitleCase(string name)
        {
            if (!string.IsNullOrEmpty(name) && name != "--Not found--")
            {
                TextInfo myCI = new CultureInfo("en-US", false).TextInfo; //creates CI
                name = name.ToLower().Trim(' '); //takes to lower, to take to TC later
                name = myCI.ToTitleCase(name); //takes to TC
            }

            return name;
        }

        /// <summary>
        /// dictionary of race categories
        /// </summary>
        public Dictionary<string, int> CategoryFactorDict
        {
            get
            {
                Dictionary<string, int> _categoryFactorDict = GetRaceDictionary();
                return _categoryFactorDict;
            }
        }

        /// <summary>
        /// pcollection of displayed horses
        /// </summary>
        public ObservableCollection<HorseDataWrapper> HorseList { get; set; }

        /// <summary>
        /// changes Save btn IsEnabled prop when collection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HorseListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateButtons();
        }

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

        public string Distance
        {
            get
            {
                return _raceDataModel.Distance;
            }
            set
            {
                _raceDataModel.Distance = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        public string Category
        {
            get
            {
                return _raceDataModel.Category;
            }
            set
            {
                _raceDataModel.Category = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        public string City
        {
            get
            {
                return _raceDataModel.City;
            }
            set
            {
                _raceDataModel.City = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        public string RaceNo
        {
            get
            {
                return _raceDataModel.RaceNo;
            }
            set
            {
                _raceDataModel.RaceNo = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        public DateTime RaceDate
        {
            get
            {
                return _raceDataModel.RaceDate;
            }
            set
            {
                _raceDataModel.RaceDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// IsEnabled prop for all controls during data updates / tests
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
        /// Add Horse btn IsEnabled prop
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
        /// Save btn IsEnabled prop
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
        /// ProgressBar Value
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
        /// name of the work currently done
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
        /// display status bar or not?
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
        /// display "cancel updates" btn or not
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
        /// display "cancel tests" btn or not
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
        /// display "tests" btn or not
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
        /// display "tests" btn or not
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
        /// display "cancel tests" btn or not
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
        /// checks if race data is filled up
        /// credits: https://stackoverflow.com/questions/22683040/how-to-check-all-properties-of-an-object-whether-null-or-empty
        /// </summary>
        /// <param name="myObject"></param>
        /// <returns></returns>
        bool IsAnyNullOrEmptyOrWhiteSpace(object myObject)
        {
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(myObject);
                    if (string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return true;
                    }
                }
            }
            return false;
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
                        horseWrapper.JockeyIndex = ComputeJockeyIndex(jockeyFromList, date);
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
                        horseWrapper.WinIndex = ComputeWinIndex(horseFromList, date, jockeyFromList);
                    else
                        horseWrapper.WinIndex = ComputeWinIndex(horseFromList, date, null);

                    //category index
                    horseWrapper.CategoryIndex = ComputeCategoryIndex(horseFromList, date);

                    //age index
                    horseWrapper.AgeIndex = ComputeAgeIndex(horseFromList, date);

                    //tired index
                    horseWrapper.TiredIndex = ComputeTiredIndex(horseFromList, date);

                    //win percentage
                    horseWrapper.PercentageIndex = ComputePercentageIndex(horseFromList, date);

                    //days of rest
                    horseWrapper.RestIndex = ComputeRestIndex(horseFromList, date);
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
                        horseWrapper.SiblingsIndex = ComputeSiblingsIndex(fatherFromList, date);
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

        private double ComputeTiredIndex(LoadedHorse horseFromList, DateTime date)
        {
            double finalResult = 0;
            int raceCounter = 0;

            if (horseFromList.AllRaces.Count > 0)
            {
                //get races only before race date, sort by race date from biggest to smallest
                horseFromList.AllRaces = horseFromList.AllRaces.Where(l => l.RaceDate < date).OrderByDescending(l => l.RaceDate).ToList();

                if (horseFromList.AllRaces.Count == 0)
                    return 1;

                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    //if task cancelled by the user
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    DateTime twoYearsBack = date.AddYears(-2);

                    //for all races 2 years back from this race
                    if (horseFromList.AllRaces[i].RaceDate < date && horseFromList.AllRaces[i].RaceDate > twoYearsBack)
                    {
                        raceCounter++;
                    }
                }

                if (raceCounter > 12)
                {
                    finalResult = 1 + (raceCounter - 12) * 0.1;
                }
                else
                {
                    return 1;
                }

                return finalResult;
            }
            else
            {
                return 1;
            }
        }

        private double ComputeRestIndex(LoadedHorse horseFromList, DateTime date)
        {
            double finalResult = 0;
            double breakDays = 0;

            if (horseFromList.AllRaces.Count > 0)
            {
                //get races only before race date, sort by race date from biggest to smallest
                horseFromList.AllRaces = horseFromList.AllRaces.Where(l => l.RaceDate < date).OrderByDescending(l => l.RaceDate).ToList();

                if (horseFromList.AllRaces.Count == 0)
                    return 1;

                breakDays = (date - horseFromList.AllRaces[0].RaceDate).TotalDays;

                if (breakDays > 90)
                {
                    finalResult = 1 + (breakDays - 90) / 100;
                }
                else if (breakDays < 60)
                {
                    finalResult = 1 + breakDays / 50;
                }
                else
                {
                    finalResult = 1;
                }

                return finalResult;
            }
            else
            {
                return 1;
            }
        }

        private double ComputePercentageIndex(LoadedHorse horseFromList, DateTime date)
        {
            int winCounter = 0;
            double finalResult = 0;

            if (horseFromList.AllRaces.Count > 0)
            {

                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    //if task cancelled by the user
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    if (horseFromList.AllRaces[i].WonPlace == 1 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        winCounter++;
                    }
                }

                double percentage = (double)winCounter / horseFromList.AllRaces.Count * 100;

                if (percentage > 20)
                {
                    finalResult = 1 + (percentage - 20) * 0.1;
                }
                else
                {
                    finalResult = 1;
                }

                return finalResult;
            }
            else
            {
                return 1;
            }
        }

        private double ComputeAgeIndex(LoadedHorse horseFromList, DateTime date)
        {
            int yearsDifference = DateTime.Now.Year - date.Year; //how many years passed since race

            int horsesRaceAge = horseFromList.Age - yearsDifference; //how old was horse

            if (horsesRaceAge > 5)
            {
                int multiplier = horsesRaceAge - 4;
                double result = 1 + 0.3 * multiplier;
                return result;
            }
            else
            {
                return 1;
            }

        }

        /// <summary>
        /// calculates category index for the horse
        /// </summary>
        /// <param name="horseFromList">horse data</param>
        /// <param name="date">day of the race</param>
        /// <returns></returns>
        private double ComputeCategoryIndex(LoadedHorse horseFromList, DateTime date)
        {
            Dictionary<string, int> categoryFactorDict = GetRaceDictionary();
            int dictValue = 1;
            double finalResult;
            double result = 0;

            if (horseFromList.AllRaces.Count > 0)
            {
                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    double distFactor = 0;
                    double placeFactor = 0;
                    double distRaceIndex = 0;

                    //if task cancelled by the user
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    if (horseFromList.AllRaces[i].WonPlace > 0 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        placeFactor = (double)horseFromList.AllRaces[i].WonPlace / horseFromList.AllRaces[i].RaceCompetition * 10;

                        //increase factor for races over 12 horses and place between 1-4
                        if (horseFromList.AllRaces[i].RaceCompetition > 12 && horseFromList.AllRaces[i].WonPlace < 5)
                        {
                            placeFactor = placeFactor * 1.5;
                        }

                        bool foundKey = categoryFactorDict.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            dictValue = categoryFactorDict[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            dictValue = 5;
                        }

                        distFactor = (double)(horseFromList.AllRaces[i].RaceDistance - int.Parse(Distance)) / 10000 * dictValue;
                        distFactor = Math.Abs(distFactor);

                        distRaceIndex = placeFactor * dictValue / 10;

                        result = result + distRaceIndex - distFactor;
                    }
                }

                finalResult = result / horseFromList.AllRaces.Count;

                return finalResult;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// calculates win index for the horse
        /// </summary>
        /// <param name="horseFromList">horse data</param>
        /// <param name="date">day of the race</param>
        /// <returns></returns>
        private double ComputeWinIndex(LoadedHorse horseFromList, DateTime date, LoadedJockey jockeyFromList)
        {
            Dictionary<string, int> categoryFactorDict = GetRaceDictionary();
            int dictValue = 1;
            double finalResult = 0;
            double result = 0;

            if (horseFromList.AllRaces.Count > 0)
            {

                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    double distFactor = 0;
                    double placeFactor = 0;
                    double distRaceIndex = 0;

                    //get races only before race date, sort by race date from biggest to smallest
                    horseFromList.AllRaces = horseFromList.AllRaces.Where(l => l.RaceDate < date).OrderByDescending(l => l.RaceDate).ToList();

                    if (horseFromList.AllRaces.Count == 0)
                        return 0;

                    //if task cancelled by the user
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    if (horseFromList.AllRaces[i].WonPlace < 3 && horseFromList.AllRaces[i].WonPlace > 0 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        if (horseFromList.AllRaces[i].WonPlace == 1)
                            placeFactor = 1;
                        if (horseFromList.AllRaces[i].WonPlace == 2)
                            placeFactor = 0.7;

                        if (jockeyFromList != null)
                        {
                            //bonus if was the same jockey as in current race
                            if (!string.IsNullOrEmpty(jockeyFromList.Name) && !string.IsNullOrEmpty(horseFromList.AllRaces[i].RacersName))
                            {
                                if (horseFromList.AllRaces[i].RacersName.Contains(jockeyFromList.Name))
                                    placeFactor = placeFactor * 1.5;
                            }
                        }

                        //bonus for place factor if won race in last 3 races
                        if (i < 3)
                        {
                            placeFactor = placeFactor * 1.5;
                        }

                        //increase factor for races over 12 horses and place between 1-4
                        if (horseFromList.AllRaces[i].RaceCompetition > 12 && horseFromList.AllRaces[i].WonPlace < 5)
                        {
                            placeFactor = placeFactor * 1.5;
                        }

                        bool foundKey = categoryFactorDict.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            dictValue = categoryFactorDict[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            dictValue = 5;
                        }

                        distFactor = (double)(horseFromList.AllRaces[i].RaceDistance - int.Parse(Distance)) / 10000 * dictValue;
                        distFactor = Math.Abs(distFactor);

                        distRaceIndex = placeFactor * horseFromList.AllRaces[i].RaceCompetition * dictValue / 10;

                        result = result + distRaceIndex - distFactor;
                    }
                }

                finalResult = result / horseFromList.AllRaces.Count;

                return finalResult;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// calculates index for the jockey
        /// </summary>
        /// <param name="jockeyFromList">name of the jockey</param>
        /// <param name="date">day of the race</param>
        /// <returns></returns>
        private double ComputeJockeyIndex(LoadedJockey jockeyFromList, DateTime date)
        {
            double finalResult = 0;
            double result = 0;

            if (jockeyFromList.AllRaces.Count > 0)
            {
                for (int i = 0; i < jockeyFromList.AllRaces.Count; i++)
                {
                    double placeFactor = 0;
                    double distRaceIndex = 0;

                    //if task cancelled by the user
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    if (jockeyFromList.AllRaces[i].WonPlace > 0 && jockeyFromList.AllRaces[i].RaceDate < date)
                    {
                        if (jockeyFromList.AllRaces[i].WonPlace == 1)
                            placeFactor = 1;
                        if (jockeyFromList.AllRaces[i].WonPlace == 2)
                            placeFactor = 0.7;

                        distRaceIndex = placeFactor * jockeyFromList.AllRaces[i].RaceCompetition / 10;

                        result = result + distRaceIndex;
                    }
                }

                finalResult = result / jockeyFromList.AllRaces.Count;

                return finalResult;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// calculates index for horses siblings
        /// </summary>
        /// <param name="fatherFromList">data of horses father</param>
        /// <param name="date">day of the race</param>
        /// <returns></returns>
        private double ComputeSiblingsIndex(LoadedHorse fatherFromList, DateTime date)
        {
            double finalResult = 0;
            double result = 0;
            double siblingIndex = 0;
            int childCounter = 0;
            LoadedHorse childFromList;

            for (int i = 0; i < fatherFromList.AllChildren.Count; i++)
            {
                //if task cancelled by the user
                if (TaskCancellation == true)
                {
                    break;
                }

                Horses = new ObservableCollection<LoadedHorse>(Horses.OrderBy(l => l.Age)); //from smallest to biggest
                HorseChildDetails child = fatherFromList.AllChildren[i];

                if (child.ChildAge == 0)
                {
                    childFromList = Horses
                                .Where(h => h.Name.ToLower() == child
                                .ChildName.ToLower())
                                .FirstOrDefault();
                }
                else
                {
                    childFromList = Horses
                                .Where(h => h.Name.ToLower() == child.ChildName.ToLower())
                                .Where(h => h.Age == child.ChildAge)
                                .FirstOrDefault();
                }

                if (childFromList != null && childFromList.AllRaces.Count > 0)
                {
                    siblingIndex = ComputeWinIndex(childFromList, date, null);
                    childCounter++;
                }
                else
                {
                    siblingIndex = 0;
                }

                result = result + siblingIndex;
            }

            if (childCounter != 0)
            {
                finalResult = result / childCounter;
            }
            else
            {
                finalResult = 0;
            }

            return finalResult;
        }

        /// <summary>
        /// list of race categories and values of them
        /// </summary>
        /// <returns>category dictionary with string key and int value</returns>
        private Dictionary<string, int> GetRaceDictionary()
        {
            Dictionary<string, int> categoryFactorDict = new Dictionary<string, int>();
            categoryFactorDict.Add("G1 A", 11);
            categoryFactorDict.Add("G3 A", 10);
            categoryFactorDict.Add("LR A", 9);
            categoryFactorDict.Add("LR B", 8);
            categoryFactorDict.Add("L A", 7);
            categoryFactorDict.Add("B", 5);
            categoryFactorDict.Add("A", 4);
            categoryFactorDict.Add("Gd 3", 11);
            categoryFactorDict.Add("Gd 1", 10);
            categoryFactorDict.Add("L", 8);
            categoryFactorDict.Add("F", 7);
            categoryFactorDict.Add("C", 6);
            categoryFactorDict.Add("D", 5);
            categoryFactorDict.Add("I", 4);
            categoryFactorDict.Add("II", 3);
            categoryFactorDict.Add("III", 2);
            categoryFactorDict.Add("IV", 1);
            categoryFactorDict.Add("V", 1);
            if (Category == "sulki" || Category == "kłusaki")
            {
                categoryFactorDict.Add("sulki", 9);
                categoryFactorDict.Add("kłusaki", 9);
            }
            else
            {
                categoryFactorDict.Add("sulki", 2);
                categoryFactorDict.Add("kłusaki", 2);
            }
            if (Category == "steeple" || Category == "płoty")
            {
                categoryFactorDict.Add("steeple", 9);
                categoryFactorDict.Add("płoty", 9);
            }
            else
            {
                categoryFactorDict.Add("steeple", 2);
                categoryFactorDict.Add("płoty", 2);
            }
            categoryFactorDict.Add("-", 6);
            categoryFactorDict.Add(" ", 6);
            categoryFactorDict.Add("", 6);
            return categoryFactorDict;
        }

        /// <summary>
        /// is parsing race stats with every single horse from all historic races
        /// </summary>
        /// <returns></returns>
        public async Task TestHistoricalResultsAsync()
        {
            //init values and controls
            CommandStartedControlsSetup("TestResultsCommand");

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

                Task task = Task.Run(() =>
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

                    loopCounter++;

                    ProgressBarTick("Testing on historic data", loopCounter, Races.Count, 0);

                }, TokenSource.Token);

                tasks.Add(task);
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
            }
        }

        /// <summary>
        /// scraps historic races
        /// </summary>
        /// <param name="startIndex">page to start loop</param>
        /// <param name="stopIndex">page to finish loop</param>
        /// <param name="dataType">what site to use</param>
        /// <returns></returns>
        private async Task ScrapHistoricalRaces(int startIndex, int stopIndex, string dataType)
        {
            //init values and controls
            Races.Clear(); //method does not remove doublers
            CommandStartedControlsSetup("UpdateDataCommand");
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            ProgressBarTick("Looking for historic races", loopCounter, stopIndex, startIndex);

            for (int i = startIndex; i < stopIndex; i++)
            {
                if (TaskCancellation == true)
                {
                    break;
                }
                int j = i;

                Task task = Task.Run(async () =>
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

                    loopCounter++;

                    ProgressBarTick("Looking for historic data", loopCounter, stopIndex, startIndex);

                }, TokenSource.Token);

                tasks.Add(task);
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
            }
        }

        /// <summary>
        /// scraps jockey data from the website
        /// </summary>
        /// <param name="startIndex">page to start loop</param>
        /// <param name="stopIndex">page to finish loop</param>
        /// <param name="dataType">what site to use</param>
        /// <returns></returns>
        private async Task ScrapJockeys(int startIndex, int stopIndex, string dataType)
        {
            //init values and controls
            CommandStartedControlsSetup("UpdateDataCommand");
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            ProgressBarTick("Looking for jockeys", loopCounter, stopIndex, startIndex);

            for (int i = startIndex; i < stopIndex; i++)
            {
                if (TaskCancellation == true)
                {
                    break;
                }

                int j = i;

                Task task = Task.Run(async () =>
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

                    loopCounter++;

                    //saves all every 1000 records, just in case
                    if (loopCounter % 1000 == 0)
                    {
                        await _dataServices.SaveAllJockeysAsync(Jockeys.ToList());
                    }

                    ProgressBarTick("Looking for jockeys", loopCounter, stopIndex, startIndex);

                }, TokenSource.Token);

                tasks.Add(task);
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
            }
        }

        /// <summary>
        /// updates list of jockey races, if found some new
        /// </summary>
        /// <param name="doubledJockey">jockey for Jockeys</param>
        /// <param name="jockey">found doubler</param>
        private void MergeJockeysData(LoadedJockey doubledJockey, LoadedJockey jockey)
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
        private async Task ScrapHorses(int startIndex, int stopIndex, string dataType)
        {
            //init values and controls
            CommandStartedControlsSetup("UpdateDataCommand");
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            ProgressBarTick("Looking for horses", loopCounter, stopIndex, startIndex);

            for (int i = startIndex; i < stopIndex; i++)
            {
                if (TaskCancellation == true)
                {
                    break;
                }

                int j = i;

                Task task = Task.Run(async () =>
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

                    loopCounter++;

                    //saves all every 1000 records, just in case
                    if (loopCounter % 1000 == 0)
                    {
                        await _dataServices.SaveAllHorsesAsync(Horses.ToList());
                    }

                    ProgressBarTick("Looking for horses", loopCounter, stopIndex, startIndex);

                }, TokenSource.Token);

                tasks.Add(task);
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
            }
        }

        /// <summary>
        /// updates data on progress bar for any task
        /// </summary>
        /// <param name="workStatus">name of commenced work</param>
        /// <param name="loopCounter">current counter in the loop</param>
        /// <param name="stopIndex">when loop finishes</param>
        /// <param name="startIndex">when loop starts</param>
        private void ProgressBarTick(string workStatus, int loopCounter, int stopIndex, int startIndex)
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
        private void MergeHorsesData(LoadedHorse doubledHorse, LoadedHorse horse)
        {
            doubledHorse.AllChildren = doubledHorse.AllChildren.Union(horse.AllChildren).ToList();
            doubledHorse.AllRaces = doubledHorse.AllRaces.Union(horse.AllRaces).ToList();
            Horses.Add(doubledHorse);
        }

        private void CommandStartedControlsSetup(string command)
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

        private void CommandCompletedControlsSetup()
        {
            TokenSource.Dispose();
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
