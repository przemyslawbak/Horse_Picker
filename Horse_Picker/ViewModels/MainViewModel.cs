using Horse_Picker.Commands;
using Horse_Picker.Models;
using Horse_Picker.Services;
using Horse_Picker.Wrappers;
using Horse_Picker.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Horse_Picker.Services.Message;
using Horse_Picker.Services.Update;
using Horse_Picker.Services.Simulate;
using Horse_Picker.Services.Files;
using Prism.Events;
using Horse_Picker.Services.Dictionary;
using System.Runtime.CompilerServices;

namespace Horse_Picker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;
        private IMessageService _messageDialogService;
        private IUpdateService _updateDataService;
        private ISimulateService _simulateDataService;
        private IFileService _dataServices;
        private IDictionariesService _dictionaryService;

        public MainViewModel(IFileService dataServices,
            IMessageService messageDialogServices,
            IUpdateService updateDataService,
            ISimulateService simulateDataService,
            IEventAggregator eventAggregator,
            IDictionariesService dictionaryService)
        {
            Horses = new List<LoadedHorse>();
            Jockeys = new List<LoadedJockey>();
            Races = new List<RaceDetails>();
            HorseList = new ObservableCollection<HorseDataWrapper>();
            LoadedHorses = new List<string>();
            LoadedJockeys = new List<string>();
            DateTimeNow = DateTime.Now;
            DataUpdateModules = new UpdateModules();

            _eventAggregator = eventAggregator;
            _dataServices = dataServices;
            _updateDataService = updateDataService;
            _simulateDataService = simulateDataService;
            _messageDialogService = messageDialogServices;
            _dictionaryService = dictionaryService;

            AllControlsEnabled = true;
            VisibilityUpdatingBtn = true;
            VisibilityTestingBtn = true;

            NewHorseCommand = new DelegateCommand(OnNewHorseExecute);
            ClearDataCommand = new DelegateCommand(OnClearDataExecute);
            UpdateCancellationCommand = new DelegateCommand(OnUpdateCancellationExecute);
            SimulateCancellationCommand = new DelegateCommand(OnSimulateCancellationExecute);
            SimulateResultsCommand = new AsyncCommand(async () => await OnSimulateResultsExecuteAsync());
            PickHorseDataCommand = new DelegateCommand(OnPickHorseDataExecute);
            UpdateDataCommand = new AsyncCommand(async () => await OnUpdateDataExecuteAsync());

            RaceModelProvider = new RaceModel();
            CategoryFactorDict = _dictionaryService.GetRaceCategoryDictionary(RaceModelProvider);

            //delegates and commands
            ClearDataCommand.Execute(null);
            HorseList.CollectionChanged += OnHorseListCollectionChanged;
            _eventAggregator.GetEvent<DataUpdateEvent>().Subscribe(OnDataUpdate); //watches for update view model properties update event
            _eventAggregator.GetEvent<ProgressBarEvent>().Subscribe(OnProgressBarTick); //watches for service layer progress bar data update event
            _eventAggregator.GetEvent<LoadDataEvent>().Subscribe(OnLoadAllDataAsync); //watches for vm load data update event
            _eventAggregator.GetEvent<LoadDataEvent>().Publish(); // publish vm load data update event
        }

        /// <summary>
        /// event subscrition for update properties came from UpdateViewModel
        /// </summary>
        /// <param name="updateModules"></param>
        private void OnDataUpdate(UpdateModules updateModules)
        {
            DataUpdateModules = updateModules;
        }

        /// <summary>
        /// executed on click simulation `cancel` btn
        /// </summary>
        /// <param name="obj"></param>
        public void OnSimulateCancellationExecute(object obj)
        {
            _simulateDataService.CancelSimulation();

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
            string callersName = GetCallerName();
            CommandStartedControlsSetup(callersName); //setup controls for job time being

            Races = await _simulateDataService.SimulateResultsAsync(0, Races.Count, Races, Horses, Jockeys, RaceModelProvider);

            ResetControls(); //reset controlls after job done
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
            HorseWrapper = _updateDataService.GetParsedHorseData(HorseWrapper, DateTimeNow, Horses, Jockeys, RaceModelProvider); //consumes time
        }

        /// <summary>
        /// on `Update data` btn click,
        /// is scraping data from www
        /// </summary>
        /// <returns></returns>
        public async Task OnUpdateDataExecuteAsync()
        {
            var result = _messageDialogService.ShowUpdateWindow();

            UpdateModules = new List<bool>
            {
                DataUpdateModules.JockeysPl,
                DataUpdateModules.JockeysCz,
                DataUpdateModules.HorsesCz,
                DataUpdateModules.HorsesPl,
                DataUpdateModules.RacesPl
            };

            bool isAnyTrue = UpdateModules.Any(module => module == true);

            if (result == MessageDialogResult.Update && isAnyTrue)
            {
                string callersName = GetCallerName();
                CommandStartedControlsSetup(callersName); //setup controls for job time being

                if (DataUpdateModules.JockeysPl)
                    Jockeys = await _updateDataService.UpdateDataAsync(Jockeys, DataUpdateModules.JPlFrom, DataUpdateModules.JPlTo, "updateJockeysPl");
                if (DataUpdateModules.JockeysCz)
                    Jockeys = await _updateDataService.UpdateDataAsync(Jockeys, DataUpdateModules.JCzFrom, DataUpdateModules.JCzTo, "updateJockeysCz");
                if (DataUpdateModules.HorsesPl)
                    Horses = await _updateDataService.UpdateDataAsync(Horses, DataUpdateModules.HPlFrom, DataUpdateModules.HPlTo, "updateHorsesPl");
                if (DataUpdateModules.HorsesCz)
                    Horses = await _updateDataService.UpdateDataAsync(Horses, DataUpdateModules.HCzFrom, DataUpdateModules.HCzTo, "updateHorsesCz");
                if (DataUpdateModules.RacesPl)
                    Races = await _updateDataService.UpdateDataAsync(Races, DataUpdateModules.HistPlFrom, DataUpdateModules.HistPlTo, "updateHistoricPl");

                ResetControls(); //reset controlls after job done

                _eventAggregator.GetEvent<LoadDataEvent>().Publish(); // publish vm load data update event
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
            RaceDate = DateTimeNow;
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
        /// subscribed to event
        /// loads horses from the file data services
        /// async void eventhandler testing credits: https://stackoverflow.com/a/19415703/11027921
        /// delegates https://docs.microsoft.com/en-us/dotnet/api/system.eventhandler?view=netframework-4.8
        /// </summary>
        public async void OnLoadAllDataAsync()
        {
            string callersName = GetCallerName();
            CommandStartedControlsSetup(callersName);

            if (Horses.Count == 0 && Jockeys.Count == 0 && Races.Count == 0)
            {
                List<LoadedHorse> horses = await _dataServices.GetAllHorsesAsync();
                List<LoadedJockey> jockeys = await _dataServices.GetAllJockeysAsync();
                List<RaceDetails> races = await _dataServices.GetAllRacesAsync();

                foreach (var horse in horses)
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

                Horses = Horses.OrderBy(o => o.Age).ToList();

                foreach (var jockey in jockeys)
                {
                    Jockeys.Add(new LoadedJockey
                    {
                        Name = jockey.Name,
                        AllRaces = jockey.AllRaces,
                        Link = jockey.Link
                    });
                }

                foreach (var race in races)
                {
                    Races.Add(new RaceDetails
                    {
                        RaceCategory = race.RaceCategory,
                        RaceDate = race.RaceDate,
                        RaceDistance = race.RaceDistance,
                        RaceLink = race.RaceLink,
                        HorseList = race.HorseList
                    });
                }
            }

            PopulateLists();

            CommandCompletedControlsSetup();
            ResetControls();
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
        public RaceModel RaceModelProvider { get; set; }
        public DateTime DateTimeNow { get; set; }
        public UpdateModules DataUpdateModules { get; set; }
        public ObservableCollection<HorseDataWrapper> HorseList { get; set; }
        public List<bool> UpdateModules { get; private set; }
        public HorseDataWrapper HorseWrapper { get; set; }
        public List<LoadedHorse> Horses { get; set; }
        public List<LoadedJockey> Jockeys { get; set; }
        public List<RaceDetails> Races { get; set; }
        public List<string> LoadedHorses { get; }
        public List<string> LoadedJockeys { get; }
        public Dictionary<string, int> CategoryFactorDict { get; set; }

        //prop race distance
        public string Distance
        {
            get
            {
                return RaceModelProvider.Distance;
            }
            set
            {
                RaceModelProvider.Distance = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop race category
        public string Category
        {
            get
            {
                return RaceModelProvider.Category;
            }
            set
            {
                RaceModelProvider.Category = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop race city
        public string City
        {
            get
            {
                return RaceModelProvider.City;
            }
            set
            {
                RaceModelProvider.City = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop race No
        public string RaceNo
        {
            get
            {
                return RaceModelProvider.RaceNo;
            }
            set
            {
                RaceModelProvider.RaceNo = value;
                OnPropertyChanged();
                ValidateButtons();
            }
        }

        //prop date of the race
        public DateTime RaceDate
        {
            get
            {
                return RaceModelProvider.RaceDate;
            }
            set
            {
                RaceModelProvider.RaceDate = value;
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
        public void ValidateButtons()
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

        //cool visibility stuff: https://stackoverflow.com/questions/7000819/binding-a-buttons-visibility-to-a-bool-value-in-viewmodel

        /// <summary>
        /// display `status bar` or not?
        /// </summary>
        private bool _visibilityStatusBar;
        public bool VisibilityStatusBar
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
        private bool _visibilityCancelUpdatesBtn;
        public bool VisibilityCancelUpdatingBtn
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
        private bool _visibilityCancelTestingBtn;
        public bool VisibilityCancelTestingBtn
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
        private bool _visibilityTestingBtn;
        public bool VisibilityTestingBtn
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
        private bool _visibilityUpdatingBtn;
        public bool VisibilityUpdatingBtn
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
        private bool _visibilityCancellingMsg;
        public bool VisibilityCancellingMsg
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
        public void OnProgressBarTick(ProgressBarData bar)
        {
            int divider = bar.ToId - bar.FromId;

            WorkStatus = bar.JobType;

            if (divider > 0)
            {
                UpdateStatusBar = bar.LoopCouner * 100 / (bar.ToId - bar.FromId);
                ProgressDisplay = bar.LoopCouner + " / " + (bar.ToId - bar.FromId);
            }
            else if (divider == 0)
            {
                UpdateStatusBar = 1 / 1;
                ProgressDisplay = "1 / 1";
            }
            else if (divider < 0)
            {
                UpdateStatusBar = 1 / 1;
                ProgressDisplay = "ERROR";
            }

        }

        /// <summary>
        /// changes some display props on starting long running tasks
        /// </summary>
        /// <param name="caller"></param>
        public void CommandStartedControlsSetup(string caller)
        {
            AllControlsEnabled = false;
            ValidateButtons();
            VisibilityStatusBar = true;
            UpdateStatusBar = 0;

            if (caller == "OnSimulateResultsExecuteAsync")
            {
                VisibilityCancelTestingBtn = true;
                VisibilityTestingBtn = false;
            }

            if (caller == "OnUpdateDataExecuteAsync")
            {
                VisibilityCancelUpdatingBtn = true;
                VisibilityUpdatingBtn = false;
            }

            if (caller == "OnLoadAllDataAsync")
            {
                VisibilityCancelUpdatingBtn = false;
                VisibilityUpdatingBtn = true;
            }
        }

        /// <summary>
        /// changes some display props on stopped long running tasks
        /// </summary>
        public void CommandCompletedControlsSetup()
        {
            UpdateStatusBar = 0;
            VisibilityStatusBar = false;
            ValidateButtons();
            ProgressDisplay = "";
            WorkStatus = "";
            VisibilityCancellingMsg = true;
            VisibilityCancelTestingBtn = false;
            VisibilityTestingBtn = true;
            VisibilityCancelUpdatingBtn = false;
            VisibilityUpdatingBtn = true;
        }

        /// <summary>
        /// resets visibility controls after job done
        /// </summary>
        public void ResetControls()
        {
            CommandCompletedControlsSetup();
            AllControlsEnabled = true;
            VisibilityCancellingMsg = false;
        }

        /// <summary>
        /// Gets the name of the caller
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetCallerName([CallerMemberName] string caller = null)
        {
            return caller;
        }
    }
}