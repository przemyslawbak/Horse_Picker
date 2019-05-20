using Horse_Picker.DataProvider;
using Horse_Picker.Models;
using Horse_Picker.NewModels;
using Horse_Picker.Wrappers;
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
        private List<LoadedHorse> _allHorses;
        private List<LoadedJockey> _allJockeys;
        private List<LoadedHistoricalRace> _allRaces;
        private HorseDataWrapper _horseWrapper;
        private IFileDataServices _dataServices;
        private IScrapDataServices _scrapServices;
        private RaceData _raceDataModel;
        private UpdateModules _updateModulesModel;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _cancellationToken;
        public MainViewModel(IFileDataServices dataServices, IScrapDataServices scrapServices)
        {
            _allHorses = new List<LoadedHorse>();
            _allJockeys = new List<LoadedJockey>();
            _allRaces = new List<LoadedHistoricalRace>();
            _loadedHorses = new List<string>();
            _loadedJockeys = new List<string>();
            _dataServices = dataServices; //data files
            _scrapServices = scrapServices; //data scrap
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
            VisibilityCancelTestingBtn = Visibility.Collapsed;
            VisibilityCancelUpdatingBtn = Visibility.Collapsed;
            VisibilityUpdatingBtn = Visibility.Visible;
            VisibilityTestingBtn = Visibility.Visible;

            LoadAllData();

            HorseList.CollectionChanged += HorseListCollectionChanged;
        }

        /// <summary>
        /// loading horses from the list in web
        /// </summary>
        private void LoadAllData()
        {
            _allJockeys = _dataServices.GetAllJockeys().ToList();

            _allHorses = _dataServices.GetAllHorses().ToList();

            _allRaces = _dataServices.GetAllRaces().ToList();
        }

        /// <summary>
        /// list of horses for AutoCompleteBox binding
        /// </summary>
        private List<string> _loadedHorses;
        public List<string> LoadedHorses
        {
            get
            {
                _loadedHorses.Clear();
                for (int i = 0; i < _allHorses.Count; i++)
                {
                    string theName = MakeTitleCase(_allHorses[i].Name);
                    _loadedHorses.Add(theName + ", " + _allHorses[i].Age.ToString());
                }

                return _loadedHorses;
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
        /// list of jockeys for AutoCompleteBox binding
        /// </summary>
        private List<string> _loadedJockeys;
        public List<string> LoadedJockeys
        {
            get
            {
                _loadedJockeys.Clear();
                for (int i = 0; i < _allJockeys.Count; i++)
                {
                    _loadedJockeys.Add(_allJockeys[i].Name);
                }

                return _loadedJockeys;
            }
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

        public bool JockeysPl
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

        public bool JockeysCz
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

        public bool HorsesCz
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

        public bool HorsesPl
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

        public bool RacesPl
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
        /// add new horse to the list
        /// </summary>
        private ICommand _newHorseCommand;
        public ICommand NewHorseCommand
        {
            get
            {
                if (_newHorseCommand == null)
                    _newHorseCommand = new RelayCommand(
                    o =>
                    {
                        _horseWrapper = new HorseDataWrapper();
                        HorseList.Add(_horseWrapper);
                    });
                return _newHorseCommand;
            }
        }

        /// <summary>
        /// clear all horses from the list
        /// </summary>
        private ICommand _clearDataCommand;
        public ICommand ClearDataCommand
        {
            get
            {
                if (_clearDataCommand == null)
                    _clearDataCommand = new RelayCommand(
                    o =>
                    {
                        HorseList.Clear();
                        Category = "fill up";
                        City = "-";
                        Distance = "0";
                        RaceNo = "0";
                        RaceDate = DateTime.Now;
                    });
                return _clearDataCommand;
            }
        }

        /// <summary>
        /// pick up horses data
        /// </summary>
        private ICommand _pickHorseDataCommand;
        public ICommand PickHorseDataCommand
        {
            get
            {
                if (_pickHorseDataCommand == null)
                    _pickHorseDataCommand = new RelayCommand(
                    o =>
                    {
                        HorseDataWrapper horseWrapper = (HorseDataWrapper)o;
                        Task task = Task.Run(() => horseWrapper = ParseHorseData(horseWrapper, DateTime.Now)); //sometimes consume time
                    });
                return _pickHorseDataCommand;
            }
        }

        /// <summary>
        /// parses the horse from _allHorses with providen data
        /// </summary>
        /// <param name="horseWrapper">horse data</param>
        /// <param name="date">day of the race</param>
        /// <returns></returns>
        private HorseDataWrapper ParseHorseData(HorseDataWrapper horseWrapper, DateTime date)
        {
            LoadedJockey jockeyFromList = new LoadedJockey();
            LoadedHorse horseFromList = new LoadedHorse();
            LoadedHorse fatherFromList = new LoadedHorse();
            //if name is entered
            if (!string.IsNullOrEmpty(horseWrapper.HorseName))
            {
                _allHorses = _allHorses.OrderBy(l => l.Age).ToList(); //from smallest to biggest

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
                    horseFromList = _allHorses
                    .Where(h => h.Name.ToLower() == horseWrapper.HorseName.ToLower())
                    .FirstOrDefault();
                }

                //if age is written
                else
                {
                    horseFromList = _allHorses
                    .Where(h => h.Name.ToLower() == horseWrapper.HorseName.ToLower())
                    .Where(h => h.Age == horseWrapper.Age)
                    .FirstOrDefault();
                }

                //case when previous horse is replaced in Horse collection item
                if (horseFromList == null)
                {
                    horseWrapper.Age = 0;

                    horseFromList = _allHorses
                    .Where(i => i.Name.ToLower() == horseWrapper
                    .HorseName.ToLower())
                    .FirstOrDefault();
                }

                _allHorses = _allHorses.OrderByDescending(l => l.Age).ToList(); //from biggest to smallest

                if (horseFromList != null)
                {
                    horseWrapper.HorseName = horseFromList.Name; //displayed name
                    horseWrapper.Age = horseFromList.Age; //displayed age
                    horseWrapper.Father = horseFromList.Father; //displayed father
                    horseWrapper.TotalRaces = horseFromList.AllRaces.Count(); //how many races
                    horseWrapper.Comments = "";

                    //win index
                    horseWrapper.WinIndex = ComputeWinIndex(horseFromList, date);

                    //category index
                    horseWrapper.CategoryIndex = ComputeCategoryIndex(horseFromList, date);
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
                    fatherFromList = _allHorses.Where(i => i.Name.ToLower() == horseWrapper.Father.ToLower()).FirstOrDefault();

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

                //jockey index
                if (!string.IsNullOrEmpty(horseWrapper.Jockey))
                {
                    jockeyFromList = _allJockeys
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
            }

            return horseWrapper;
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
            double distFactor = 0;
            double placeFactor = 0;
            double distRaceIndex = 0;

            if (horseFromList.AllRaces.Count > 0)
            {
                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    if (horseFromList.AllRaces[i].WonPlace > 0 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        placeFactor = (double)horseFromList.AllRaces[i].WonPlace / horseFromList.AllRaces[i].RaceCompetition * 10;

                        bool foundKey = categoryFactorDict.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            dictValue = categoryFactorDict[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            dictValue = 13;
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
        private double ComputeWinIndex(LoadedHorse horseFromList, DateTime date)
        {
            Dictionary<string, int> categoryFactorDict = GetRaceDictionary();
            int dictValue = 1;
            double finalResult = 0;
            double result = 0;
            double distFactor = 0;
            double placeFactor = 0;
            double distRaceIndex = 0;

            if (horseFromList.AllRaces.Count > 0)
            {

                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    if (horseFromList.AllRaces[i].WonPlace < 3 && horseFromList.AllRaces[i].WonPlace > 0 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        if (horseFromList.AllRaces[i].WonPlace == 1) placeFactor = 1;
                        if (horseFromList.AllRaces[i].WonPlace == 2) placeFactor = 0.8;

                        bool foundKey = categoryFactorDict.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            dictValue = categoryFactorDict[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            dictValue = 13;
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
            double distFactor = 0;
            double placeFactor = 0;
            double distRaceIndex = 0;

            if (jockeyFromList.AllRaces.Count > 0)
            {
                for (int i = 0; i < jockeyFromList.AllRaces.Count; i++)
                {
                    if (TaskCancellation == true)
                    {
                        break;
                    }

                    if (jockeyFromList.AllRaces[i].WonPlace > 0 && jockeyFromList.AllRaces[i].RaceDate < date)
                    {
                        if (jockeyFromList.AllRaces[i].WonPlace == 1) placeFactor = 1;
                        if (jockeyFromList.AllRaces[i].WonPlace < 1) placeFactor = 0;

                        distFactor = (double)(jockeyFromList.AllRaces[i].RaceDistance - int.Parse(Distance)) / 10000;
                        distFactor = Math.Abs(distFactor);

                        distRaceIndex = placeFactor * jockeyFromList.AllRaces[i].RaceCompetition;

                        result = placeFactor * 1000 / jockeyFromList.AllRaces[i].RaceCompetition;
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
                if (TaskCancellation == true)
                {
                    break;
                }

                _allHorses = _allHorses.OrderBy(l => l.Age).ToList(); //from smallest to biggest
                HorseChildDetails child = fatherFromList.AllChildren[i];

                if (child.ChildAge == 0)
                {
                    childFromList = _allHorses
                                .Where(h => h.Name.ToLower() == child
                                .ChildName.ToLower())
                                .FirstOrDefault();
                }
                else
                {
                    childFromList = _allHorses
                                .Where(h => h.Name.ToLower() == child.ChildName.ToLower())
                                .Where(h => h.Age == child.ChildAge)
                                .FirstOrDefault();
                }

                if (childFromList != null && childFromList.AllRaces.Count > 0)
                {
                    siblingIndex = ComputeWinIndex(childFromList, date);
                    childCounter++;
                }
                else
                {
                    siblingIndex = 0;
                }

                result = result + siblingIndex;
            }

            finalResult = result / childCounter;

            return finalResult;
        }

        /// <summary>
        /// list of race categories and values of them
        /// </summary>
        /// <returns>category dictionary with string key and int value</returns>
        private Dictionary<string, int> GetRaceDictionary()
        {
            Dictionary<string, int> categoryFactorDict = new Dictionary<string, int>();
            categoryFactorDict.Add("G1 A", 89);
            categoryFactorDict.Add("G3 A", 55);
            categoryFactorDict.Add("LR A", 34);
            categoryFactorDict.Add("LR B", 27);
            categoryFactorDict.Add("L A", 21);
            categoryFactorDict.Add("B", 8);
            categoryFactorDict.Add("A", 5);
            categoryFactorDict.Add("Gd 3", 89);
            categoryFactorDict.Add("Gd 1", 55);
            categoryFactorDict.Add("L", 30);
            categoryFactorDict.Add("F", 21);
            categoryFactorDict.Add("C", 13);
            categoryFactorDict.Add("D", 8);
            categoryFactorDict.Add("I", 5);
            categoryFactorDict.Add("II", 3);
            categoryFactorDict.Add("III", 2);
            categoryFactorDict.Add("IV", 1);
            categoryFactorDict.Add("V", 1);
            if (Category == "sulki" || Category == "kłusaki")
            {
                categoryFactorDict.Add("sulki", 34);
                categoryFactorDict.Add("kłusaki", 34);
            }
            else
            {
                categoryFactorDict.Add("sulki", 2);
                categoryFactorDict.Add("kłusaki", 2);
            }
            if (Category == "steeple" || Category == "płoty")
            {
                categoryFactorDict.Add("steeple", 34);
                categoryFactorDict.Add("płoty", 34);
            }
            else
            {
                categoryFactorDict.Add("steeple", 2);
                categoryFactorDict.Add("płoty", 2);
            }
            categoryFactorDict.Add("-", 13);
            categoryFactorDict.Add(" ", 13);
            categoryFactorDict.Add("", 13);
            return categoryFactorDict;
        }

        /// <summary>
        /// cancel result tests or updates
        /// </summary>
        private ICommand _taskCancellationCommand;
        public ICommand TaskCancellationCommand
        {
            get
            {
                if (_taskCancellationCommand == null)
                    _taskCancellationCommand = new RelayCommand(
                    o =>
                    {
                        TaskCancellation = true;

                        _tokenSource.Cancel();

                        CommandCompletedControlsSetup();
                    });
                return _taskCancellationCommand;
            }
        }

        /// <summary>
        /// test on historical results
        /// </summary>
        private ICommand _testResultsCommand;
        public ICommand TestResultsCommand
        {
            get
            {
                if (_testResultsCommand == null)
                    _testResultsCommand = new RelayCommand(
                    async o =>
                    {
                        _tokenSource = new CancellationTokenSource();
                        _cancellationToken = _tokenSource.Token;

                        CommandStartedControlsSetup("TestResultsCommand");

                        await TestHistoricalResults();

                        CommandCompletedControlsSetup();
                    });
                return _testResultsCommand;
            }
        }

        /// <summary>
        /// is parsing race stats with every single horse from all historic races
        /// </summary>
        /// <returns></returns>
        private async Task TestHistoricalResults()
        {
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            int taskCounter = 0;

            //for all races in the file
            for (int i = 0; i < _allRaces.Count; i++)
            {
                int j = i;

                if (TaskCancellation == true)
                {
                    break;
                }

                Task task = Task.Run(() =>
                {
                    while (true)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        loopCounter++;

                        ProgressBarTick("Requesting historic data", loopCounter, _allRaces.Count, 0);

                        //if the race is from 2018
                        if (_allRaces[j].RaceDate.Year == 2018)
                        {
                            Category = _allRaces[j].RaceCategory;
                            Distance = _allRaces[j].RaceDistance.ToString();

                            //for all horses in the race
                            for (int h = 0; h < _allRaces[j].HorseList.Count; h++)
                            {
                                _cancellationToken.ThrowIfCancellationRequested();
                                HorseDataWrapper horse = new HorseDataWrapper();
                                horse = ParseHorseData(_allRaces[j].HorseList[h], _allRaces[j].RaceDate);
                                _allRaces[j].HorseList[h] = horse; //get all indexes
                            }
                        }

                        taskCounter++;

                        if (loopCounter >= _allRaces.Count)
                        {
                            ProgressBarTick("Testing on historic data", taskCounter, _allRaces.Count, 0);
                        }
                    }
                }, _tokenSource.Token);

                tasks.Add(task);
            }

            Task entireTasks = Task.WhenAll(tasks);

            try
            {
                await Task.WhenAll(tasks);

                await Task.Run(() => _dataServices.SaveRaceTestResultsAsync(_allRaces)); //save the analysis to the file
            }
            catch (OperationCanceledException)
            {
                AllControlsEnabled = true;
            }
            finally
            {
                _tokenSource.Dispose();
            }
        }

        /// <summary>
        /// update data files
        /// </summary>
        private ICommand _updateDataCommand;
        public ICommand UpdateDataCommand
        {
            get
            {
                if (_updateDataCommand == null)
                    _updateDataCommand = new RelayCommand(
                    async o =>
                    {
                        _tokenSource = new CancellationTokenSource();
                        _cancellationToken = _tokenSource.Token;

                        CommandStartedControlsSetup("UpdateDataCommand");

                        //await ScrapJockeys(1, 1049, "jockeysPl"); //1 - 1049
                        //await ScrapJockeys(26200, 32549, "jockeysCz"); //4000 - 31049
                        //await ScrapHorses(1, 30049, "horsesPl"); //1 - 25049
                        //await ScrapHorses(114300, 150049, "horsesCz"); // 8000 - 150049
                        await ScrapHistoricalRaces(1, 18049, "racesPl"); // 1 - 17049

                        CommandCompletedControlsSetup();
                    });
                return _updateDataCommand;
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
            UpdateStatusBar = 0;

            List<Task> tasks = new List<Task>();
            int loopCounter = 0;

            for (int i = startIndex; i < stopIndex; i++)
            {
                if (TaskCancellation == true)
                {
                    break;
                }
                int j = i;
                LoadedHistoricalRace race = new LoadedHistoricalRace();

                Task task = Task.Run(async () =>
                {
                    while (true)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        if (dataType == "racesPl")
                            race = await Task.Run(() => _scrapServices.ScrapSingleRacePL(j));

                        //if the race is from 2018
                        if (race.RaceDate.Year == 2018)
                        {
                            lock (((ICollection)_allHorses).SyncRoot)
                            {
                                _allRaces.Add(race);
                            }
                        }

                        loopCounter++;

                        ProgressBarTick("Looking for historic data", loopCounter, stopIndex, startIndex);
                    }
                }, _tokenSource.Token);

                tasks.Add(task);
            }

            try
            {
                await Task.WhenAll(tasks);

                await Task.Run(() => _dataServices.SaveAllRaces(_allRaces.ToList())); //saves everything to JSON file
            }
            catch (OperationCanceledException)
            {
                AllControlsEnabled = true;
            }
            finally
            {
                _tokenSource.Dispose();
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
            UpdateStatusBar = 0;

            List<Task> tasks = new List<Task>();
            int loopCounter = 0;

            for (int i = startIndex; i < stopIndex; i++)
            {
                if (TaskCancellation == true)
                {
                    break;
                }
                int j = i;
                LoadedJockey jockey = new LoadedJockey();

                Task task = Task.Run(async () =>
                {
                    while (true)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        if (dataType == "jockeysPl")
                            jockey = await Task.Run(() => _scrapServices.ScrapSingleJockeyPL(j));
                        if (dataType == "jockeysCz")
                            jockey = await Task.Run(() => _scrapServices.ScrapSingleJockeyCZ(j));

                        if (jockey.Name != null)
                        {
                            lock (((ICollection)_allHorses).SyncRoot)
                            {//if objects are already in the List
                                if (_allJockeys.Any(h => h.Name.ToLower() == jockey.Name.ToLower()))
                                {
                                    LoadedJockey doubledJockey = _allJockeys.Where(h => h.Name.ToLower() == jockey.Name.ToLower()).FirstOrDefault();
                                    _allJockeys.Remove(doubledJockey);
                                    MergeJockeysData(doubledJockey, jockey);
                                }
                                else
                                {
                                    _allJockeys.Add(jockey);
                                }
                            }
                        }

                        loopCounter++;

                        if (loopCounter % 1000 == 0)
                        {
                            await Task.Run(() => _dataServices.SaveAllJockeys(_allJockeys.ToList())); //saves everything to JSON file
                        }

                        ProgressBarTick("Looking for jockeys", loopCounter, stopIndex, startIndex);
                    }
                }, _tokenSource.Token);

                tasks.Add(task);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                AllControlsEnabled = true;
            }
            finally
            {
                _tokenSource.Dispose();
            }
        }

        /// <summary>
        /// updates list of jockey races, if found some new
        /// </summary>
        /// <param name="doubledJockey">jockey for _allJockeys</param>
        /// <param name="jockey">found doubler</param>
        private void MergeJockeysData(LoadedJockey doubledJockey, LoadedJockey jockey)
        {
            doubledJockey.AllRaces = doubledJockey.AllRaces.Union(jockey.AllRaces).ToList();
            _allJockeys.Add(doubledJockey);
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
            UpdateStatusBar = 0;

            List<Task> tasks = new List<Task>();
            int loopCounter = 0;

            for (int i = startIndex; i < stopIndex; i++)
            {
                if (TaskCancellation == true)
                {
                    break;
                }
                int j = i;
                LoadedHorse horse = new LoadedHorse();

                Task task = Task.Run(async () =>
                {
                    while (true)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        if (dataType == "horsesPl")
                            horse = await Task.Run(() => _scrapServices.ScrapSingleHorsePL(j));
                        if (dataType == "horsesCz")
                            horse = await Task.Run(() => _scrapServices.ScrapSingleHorseCZ(j));

                        if (horse.Name != null)
                        {
                            lock (((ICollection)_allHorses).SyncRoot)
                            {
                                //if objects are already in the List
                                if (_allHorses.Any(h => h.Name.ToLower() == horse.Name.ToLower()))
                                {
                                    LoadedHorse doubledHorse = _allHorses.Where(h => h.Name.ToLower() == horse.Name.ToLower()).Where(h => h.Age == horse.Age).FirstOrDefault();
                                    if (doubledHorse != null)
                                    {
                                        _allHorses.Remove(doubledHorse);
                                        MergeHorsesData(doubledHorse, horse);
                                    }
                                    else
                                    {
                                        _allHorses.Add(horse);
                                    }
                                }
                                else
                                {
                                    _allHorses.Add(horse);
                                }
                            }
                        }

                        loopCounter++;

                        if (loopCounter % 1000 == 0)
                        {
                            await Task.Run(() => _dataServices.SaveAllHorses(_allHorses.ToList())); //saves everything to JSON file
                        }

                        ProgressBarTick("Looking for horses", loopCounter, stopIndex, startIndex);
                    }
                }, _tokenSource.Token);

                tasks.Add(task);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                AllControlsEnabled = true;
            }
            finally
            {
                _tokenSource.Dispose();
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
            ProgressDisplay = loopCounter + " / " + stopIndex;
        }

        /// <summary>
        /// updates list of horses races and children, if found some new
        /// </summary>
        /// <param name="doubledHorse">horse from _allHorses</param>
        /// <param name="horse">scrapped new horse</param>
        private void MergeHorsesData(LoadedHorse doubledHorse, LoadedHorse horse)
        {
            doubledHorse.AllChildren = doubledHorse.AllChildren.Union(horse.AllChildren).ToList();
            doubledHorse.AllRaces = doubledHorse.AllRaces.Union(horse.AllRaces).ToList();
            _allHorses.Add(doubledHorse);
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
            TaskCancellation = false;
            UpdateStatusBar = 0;
            VisibilityStatusBar = Visibility.Hidden;
            ValidateButtons();
            ProgressDisplay = "";
            WorkStatus = "";
            VisibilityCancelTestingBtn = Visibility.Collapsed;
            VisibilityTestingBtn = Visibility.Visible;
            VisibilityCancelUpdatingBtn = Visibility.Collapsed;
            VisibilityUpdatingBtn = Visibility.Visible;
        }
    }
}
