using Horse_Picker.Commands;
using Horse_Picker.Models;
using Horse_Picker.Services;
using Horse_Picker.Wrappers;
using Horse_Picker.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Horse_Picker.Services.Message;
using Horse_Picker.Services.Update;
using Horse_Picker.Services.Simulate;
using Horse_Picker.Services.Files;
using Prism.Events;
using Horse_Picker.Services.Dictionary;

namespace Horse_Picker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IDictionariesService _dictionaryService;
        private IEventAggregator _eventAggregator;
        private IMessageService _messageDialogService;
        private IUpdateService _updateDataService;
        private ISimulateService _simulateDataService;
        private IFileService _dataServices;
        private IRaceProvider _raceModelProvider;

        public MainViewModel(IFileService dataServices,
            IMessageService messageDialogServices,
            IRaceProvider raceServices,
            IUpdateService updateDataService,
            ISimulateService simulateDataService,
            IEventAggregator eventAggregator,
            IDictionariesService dictionaryService)
        {
            Horses = new ObservableCollection<LoadedHorse>();
            Jockeys = new ObservableCollection<LoadedJockey>();
            Races = new ObservableCollection<LoadedHistoricalRace>();
            HorseList = new ObservableCollection<HorseDataWrapper>();
            LoadedHorses = new List<string>();
            LoadedJockeys = new List<string>();

            _eventAggregator = eventAggregator;
            _dictionaryService = dictionaryService;
            _dataServices = dataServices;
            _updateDataService = updateDataService;
            _simulateDataService = simulateDataService;
            _raceModelProvider = raceServices;
            _messageDialogService = messageDialogServices;

            AllControlsEnabled = true;
            VisibilityStatusBar = Visibility.Hidden;
            VisibilityCancellingMsg = Visibility.Collapsed;
            VisibilityCancelTestingBtn = Visibility.Collapsed;
            VisibilityCancelUpdatingBtn = Visibility.Collapsed;
            VisibilityUpdatingBtn = Visibility.Visible;
            VisibilityTestingBtn = Visibility.Visible;

            NewHorseCommand = new DelegateCommand(OnNewHorseExecute);
            ClearDataCommand = new DelegateCommand(OnClearDataExecute);
            UpdateCancellationCommand = new DelegateCommand(OnUpdateCancellationExecute);
            SimulateCancellationCommand = new DelegateCommand(OnSimulateCancellationExecute);
            SimulateResultsCommand = new AsyncCommand(async () => await OnSimulateResultsExecuteAsync());
            PickHorseDataCommand = new DelegateCommand(OnPickHorseDataExecute);
            UpdateDataCommand = new AsyncCommand(async () => await OnUpdateDataExecuteAsync());

            ClearDataCommand.Execute(null);
            LoadAllData();
            PopulateLists();
            CategoryFactorDict = _dictionaryService.GetRaceCategoryDictionary(_raceModelProvider);

            HorseList.CollectionChanged += OnHorseListCollectionChanged;
            _eventAggregator.GetEvent<DataUpdateEvent>().Subscribe(OnDataUpdate);
        }

        private void OnDataUpdate(UpdateModules updateModules)
        {
            DataUpdateModules = updateModules;
        }

        private void OnSimulateCancellationExecute(object obj)
        {
            _simulateDataService.CancelUpdates();

            CommandCompletedControlsSetup();
        }

        /// <summary>
        /// on `cancel` btn click,
        /// is canceling TokenSource for every single Task,
        /// `TaskCancellation` property is breaking every single loop in Tasks
        /// </summary>
        /// <param name="obj"></param>
        public void OnUpdateCancellationExecute(object obj)
        {
            _updateDataService.CancelUpdates();

            CommandCompletedControlsSetup();
        }

        /// <summary>
        /// on `Test results` btn click,
        /// is testing strategy on historic results
        /// </summary>
        /// <returns></returns>
        public async Task OnSimulateResultsExecuteAsync()
        {
            var stopwatch = Stopwatch.StartNew();//stopwatch

            _simulateDataService._simulateProgressEventHandler += new EventHandler<UpdateBarEventArgs>(ProgressBarTick); //sub to service event

            CommandStartedControlsSetup("SimulateResultsCommand");

            Races = await _simulateDataService.SimulateResultsAsync(0, Races.Count, Races, Horses, Jockeys, _raceModelProvider);

            _simulateDataService._simulateProgressEventHandler -= new EventHandler<UpdateBarEventArgs>(ProgressBarTick); //sub to service event

            stopwatch.Stop();//stopwatch
            MessageBox.Show(stopwatch.Elapsed.ToString());//stopwatch

            CommandCompletedControlsSetup();
            AllControlsEnabled = true;
            VisibilityCancellingMsg = Visibility.Collapsed;

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
            Task.Run(() => HorseWrapper = _updateDataService.GetParsedHorseData(HorseWrapper, date, Horses, Jockeys, _raceModelProvider)); //consumes time
        }

        /// <summary>
        /// on `Update data` btn click,
        /// is scraping data from www
        /// </summary>
        /// <returns></returns>
        public async Task OnUpdateDataExecuteAsync()
        {
            var result = _messageDialogService.ShowUpdateWindow();

            UpdateModules = new ObservableCollection<bool>();
            UpdateModules.Add(DataUpdateModules.JockeysPl);
            UpdateModules.Add(DataUpdateModules.JockeysCz);
            UpdateModules.Add(DataUpdateModules.HorsesCz);
            UpdateModules.Add(DataUpdateModules.HorsesPl);
            UpdateModules.Add(DataUpdateModules.RacesPl);

            bool isAnyTrue = UpdateModules.Any(module => module == true);

            if (result == MessageDialogResult.Update && isAnyTrue)
            {
                CommandStartedControlsSetup("UpdateDataCommand");

                var stopwatch = Stopwatch.StartNew(); //stopwatch

                _updateDataService._updateProgressEventHandler += new EventHandler<UpdateBarEventArgs>(ProgressBarTick); //sub to service event

                if (DataUpdateModules.JockeysPl)
                    Jockeys = await _updateDataService.UpdateDataAsync(Jockeys, DataUpdateModules.JPlFrom, DataUpdateModules.JPlTo, "updateJockeysPl");
                if (DataUpdateModules.JockeysCz)
                    Jockeys = await _updateDataService.UpdateDataAsync(Jockeys, DataUpdateModules.JCzFrom, DataUpdateModules.JCzTo, "updateJockeysCz");
                if (DataUpdateModules.HorsesCz)
                    Horses = await _updateDataService.UpdateDataAsync(Horses, DataUpdateModules.HPlFrom, DataUpdateModules.HPlTo, "updateHorsesPl");
                if (DataUpdateModules.HorsesPl)
                    Horses = await _updateDataService.UpdateDataAsync(Horses, DataUpdateModules.HCzFrom, DataUpdateModules.HCzTo, "updateHorsesCz");
                if (DataUpdateModules.RacesPl)
                    Races = await _updateDataService.UpdateDataAsync(Races, DataUpdateModules.HistPlFrom, DataUpdateModules.HistPlTo, "updateHistoricPl");

                _updateDataService._updateProgressEventHandler -= new EventHandler<UpdateBarEventArgs>(ProgressBarTick); //unsub from service event

                stopwatch.Stop();//stopwatch
                MessageBox.Show(stopwatch.Elapsed.ToString());//stopwatch

                CommandCompletedControlsSetup();
                AllControlsEnabled = true;
                VisibilityCancellingMsg = Visibility.Collapsed;

                PopulateLists();
            }
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
        public ICommand UpdateCancellationCommand { get; private set; }
        public ICommand SimulateCancellationCommand { get; private set; }
        public IAsyncCommand SimulateResultsCommand { get; private set; }
        public IAsyncCommand UpdateDataCommand { get; private set; }
        public ICommand PickHorseDataCommand { get; private set; }

        //properties
        public UpdateModules DataUpdateModules { get; set; }
        public ObservableCollection<HorseDataWrapper> HorseList { get; set; }
        public ObservableCollection<bool> UpdateModules { get; private set; }
        public HorseDataWrapper HorseWrapper { get; private set; }
        public ObservableCollection<LoadedHorse> Horses { get; private set; }
        public ObservableCollection<LoadedJockey> Jockeys { get; private set; }
        public ObservableCollection<LoadedHistoricalRace> Races { get; private set; }
        public List<string> LoadedHorses { get; }
        public List<string> LoadedJockeys { get; }
        public Dictionary<string, int> CategoryFactorDict { get; set; }

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
        /// updates data on progress bar for any task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProgressBarTick(object sender, UpdateBarEventArgs e)
        {
            WorkStatus = e.JobType;
            UpdateStatusBar = e.LoopCouner * 100 / (e.ToId - e.FromId);
            ProgressDisplay = e.LoopCouner + " / " + (e.ToId - e.FromId);
        }


        /// <summary>
        /// changes some display props on starting long running tasks
        /// </summary>
        /// <param name="command"></param>
        public void CommandStartedControlsSetup(string command)
        {
            AllControlsEnabled = false;
            ValidateButtons();
            VisibilityStatusBar = Visibility.Visible;
            UpdateStatusBar = 0;

            if (command == "SimulateResultsCommand")
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
