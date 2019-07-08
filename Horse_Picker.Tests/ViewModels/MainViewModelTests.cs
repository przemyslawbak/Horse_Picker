using Horse_Picker.Events;
using Horse_Picker.Models;
using Horse_Picker.Services;
using Horse_Picker.Services.Dictionary;
using Horse_Picker.Services.Files;
using Horse_Picker.Services.Message;
using Horse_Picker.Services.Simulate;
using Horse_Picker.Services.Update;
using Horse_Picker.ViewModels;
using Horse_Picker.Wrappers;
using Moq;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xunit;

namespace Horse_Picker.Tests.ViewModels
{
    //testing events: https://stackoverflow.com/a/9693800/11027921
    public class MainViewModelTests
    {
        private MainViewModel _viewModel;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IMessageService> _messageDialogServicesMock;
        private Mock<IFileService> _dataServicesMock;
        private Mock<IRaceProvider> _raceModelProviderMock;
        private Mock<IUpdateService> _updateDataMock;
        private Mock<ISimulateService> _simulateDataMock;
        private Mock<IDictionariesService> _dictionaryServiceMock;

        private DataUpdateEvent _dataUpdateEvent; //for subscriber
        private ProgressBarEvent _progressBarEvent; //for subscriber
        private LoadDataEvent _loadDataEvent; //for subscriber

        Mock<LoadDataEvent> _loadDataEventMock; //for publisher

        public MainViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _messageDialogServicesMock = new Mock<IMessageService>();
            _dataServicesMock = new Mock<IFileService>();
            _raceModelProviderMock = new Mock<IRaceProvider>();
            _updateDataMock = new Mock<IUpdateService>();
            _simulateDataMock = new Mock<ISimulateService>();
            _dictionaryServiceMock = new Mock<IDictionariesService>();

            _dataUpdateEvent = new DataUpdateEvent();
            _progressBarEvent = new ProgressBarEvent();
            _loadDataEvent = new LoadDataEvent();

            _loadDataEventMock = new Mock<LoadDataEvent>();

            _eventAggregatorMock.Setup(ea => ea.GetEvent<DataUpdateEvent>())
              .Returns(_dataUpdateEvent);
            _eventAggregatorMock.Setup(ea => ea.GetEvent<ProgressBarEvent>())
              .Returns(_progressBarEvent);
            _eventAggregatorMock.Setup(ea => ea.GetEvent<LoadDataEvent>())
              .Returns(_loadDataEvent);

            //SetupAllProperties credits: https://stackoverflow.com/a/24036457/11027921
            _raceModelProviderMock.SetupAllProperties();

            //moq setup
            _dataServicesMock.Setup(ds => ds.GetAllHorsesAsync())
                .Returns(Task.FromResult(new List<LoadedHorse>
                {
                    new LoadedHorse
                    {
                        Name = "Trim",
                        Age = 7,
                        AllChildren = { },
                        AllRaces = { },
                        Father = "Belenus",
                        FatherLink = "https://koniewyscigowe.pl/horse/14474",
                        Link = "https://koniewyscigowe.pl/horse/260"
                    },
                    new LoadedHorse
                    {
                        Name = "Belenus",
                        Age = 99,
                        AllChildren = { },
                        AllRaces = { },
                        Father = "-",
                        FatherLink = "",
                        Link = "https://koniewyscigowe.pl/horse/14474"
                    },
                    new LoadedHorse
                    {
                        Name = "",
                        Age = 99,
                        AllChildren = { },
                        AllRaces = { },
                        Father = "-",
                        FatherLink = "",
                        Link = "https://koniewyscigowe.pl/horse/14474"
                    }
                }));

            _dataServicesMock.Setup(ds => ds.GetAllJockeysAsync())
                .Returns(Task.FromResult(new List<LoadedJockey>
                {
                    new LoadedJockey
                    {
                        Name = "N. Hendzel",
                        Link = "https://koniewyscigowe.pl/dzokej?d=4",
                        AllRaces = { }
                    },
                    new LoadedJockey
                    {
                        Name = "V. Popov",
                        Link = "https://koniewyscigowe.pl/dzokej?d=54",
                        AllRaces = { }
                    },
                    new LoadedJockey
                    {
                        Name = "",
                        Link = "https://koniewyscigowe.pl/dzokej?d=666",
                        AllRaces = { }
                    }
                }));
            _dataServicesMock.Setup(ds => ds.GetAllRacesAsync())
                .Returns(Task.FromResult(new List<RaceDetails>
                {
                    new RaceDetails
                    {
                       HorseList = new List<HorseDataWrapper>()
                        {
                            new HorseDataWrapper()
                            {
                                HorseName = "Trim",
                                Age = 7,
                                Father = "Some Father 1"
                            },
                            new HorseDataWrapper()
                            {
                                HorseName = "Saba",
                                Age = 8,
                                Father = "Some Father 2"
                            }
                        },
                        RaceCategory = "I",
                        RaceDate = new DateTime(2018, 11, 11),
                        RaceDistance = 1600,
                        RaceLink = "somelink"
                    },
                    new RaceDetails
                    {
                        HorseList = new List<HorseDataWrapper>()
                        {
                            new HorseDataWrapper()
                            {
                                HorseName = "Dora",
                                Age = 9,
                                Father = "Some Father 3"
                            },
                            new HorseDataWrapper()
                            {
                                HorseName = "Bari",
                                Age = 10,
                                Father = "Some Father 4"
                            }
                        },
                        RaceCategory = "II",
                        RaceDate = new DateTime(2018, 11, 10),
                        RaceDistance = 1400,
                        RaceLink = "somelink2"
                    }
                }));

            _viewModel = new MainViewModel(_dataServicesMock.Object,
                _messageDialogServicesMock.Object,
                _raceModelProviderMock.Object,
                _updateDataMock.Object,
                _simulateDataMock.Object,
                _eventAggregatorMock.Object,
                _dictionaryServiceMock.Object);

            _simulateDataMock.Setup(ds => ds.SimulateResultsAsync(0, _viewModel.Races.Count, _viewModel.Races, _viewModel.Horses, _viewModel.Jockeys, _viewModel.RaceModelProvider))
                .Returns(Task.FromResult(new ObservableCollection<RaceDetails>
                {
                    new RaceDetails
                    {
                       HorseList = new List<HorseDataWrapper>()
                        {
                            new HorseDataWrapper()
                            {
                                HorseName = "Trim",
                                Age = 7,
                                Father = "Some Father 1",
                                WinIndex = 1.1
                            },
                            new HorseDataWrapper()
                            {
                                HorseName = "Saba",
                                Age = 8,
                                Father = "Some Father 2",
                                WinIndex = 2.2
                            }
                        },
                        RaceCategory = "I",
                        RaceDate = new DateTime(2018, 11, 11),
                        RaceDistance = 1600,
                        RaceLink = "somelink"
                    },
                    new RaceDetails
                    {
                        HorseList = new List<HorseDataWrapper>()
                        {
                            new HorseDataWrapper()
                            {
                                HorseName = "Dora",
                                Age = 9,
                                Father = "Some Father 3",
                                WinIndex = 3.3
                            },
                            new HorseDataWrapper()
                            {
                                HorseName = "Bari",
                                Age = 10,
                                Father = "Some Father 4",
                                WinIndex = 4.4
                            }
                        },
                        RaceCategory = "II",
                        RaceDate = new DateTime(2018, 11, 10),
                        RaceDistance = 1400,
                        RaceLink = "somelink2"
                    }
                }));
        }

        [Fact]
        public void MainViewModelInstance_SubscribesToLoadDataEventAndCallsGetAllMethods_True()
        {
            _dataServicesMock.Verify(ds => ds.GetAllHorsesAsync(), Times.Once);
            _dataServicesMock.Verify(ds => ds.GetAllJockeysAsync(), Times.Once);
            _dataServicesMock.Verify(ds => ds.GetAllRacesAsync(), Times.Once);
        }

        [Fact]
        public void MainViewModelInstance_ShouldLoadHorses_True()
        {
            Assert.Equal(3, _viewModel.Horses.Count); //counts horses
            Assert.Equal("Trim", _viewModel.Horses[0].Name);
            Assert.Equal(7, _viewModel.Horses[0].Age);
            Assert.Equal("Belenus", _viewModel.Horses[0].Father);
            Assert.Equal("https://koniewyscigowe.pl/horse/14474", _viewModel.Horses[0].FatherLink);
            Assert.Equal("https://koniewyscigowe.pl/horse/260", _viewModel.Horses[0].Link);
        }

        [Fact]
        public void MainViewModelInstance_ShouldLoadJockeys_True()
        {
            Assert.Equal(3, _viewModel.Jockeys.Count); //counts jockeys
            Assert.Equal("N. Hendzel", _viewModel.Jockeys[0].Name);
            Assert.Equal("https://koniewyscigowe.pl/dzokej?d=4", _viewModel.Jockeys[0].Link);
        }

        [Fact]
        public void MainViewModelInstance_ShouldLoadHistoricRaces_True()
        {
            Assert.Equal(2, _viewModel.Races.Count); //counts races
            Assert.Equal("I", _viewModel.Races[0].RaceCategory);
            Assert.Equal(1600, _viewModel.Races[0].RaceDistance);
            Assert.Equal("somelink", _viewModel.Races[0].RaceLink);
        }

        [Fact]
        public void MainViewModelInstance_SubscribesToDataUpdateEvent_True()
        {
            UpdateModules update = new UpdateModules()
            {
                RacesPl = true,
                HCzFrom = 20
            };

            _dataUpdateEvent.Publish(update);

            Assert.True(_viewModel.DataUpdateModules.RacesPl);
            Assert.Equal(20, _viewModel.DataUpdateModules.HCzFrom);
            Assert.Equal(update, _viewModel.DataUpdateModules);
        }

        [Fact]
        public void MainViewModelInstance_SubscribesToProgressBarEvent()
        {
            ProgressBarData bar = new ProgressBarData()
            {
                JobType = "test job",
                LoopCouner = 100,
                FromId = 10,
                ToId = 1010
            };

            _progressBarEvent.Publish(bar);

            Assert.Equal(10, _viewModel.UpdateStatusBar);
            Assert.Equal("test job", _viewModel.WorkStatus);
            Assert.Equal("100 / 1000", _viewModel.ProgressDisplay);
        }

        [Fact]
        public void CancellationExecuteMethods_CallsCancelSimulation_True()
        {
            Mock<MainViewModel> mockVm = new Mock<MainViewModel>();

            _viewModel.SimulateCancellationCommand.Execute(null);
            _viewModel.UpdateCancellationCommand.Execute(null);

            _simulateDataMock.Verify(sd => sd.CancelSimulation(), Times.Once);
            _updateDataMock.Verify(sd => sd.CancelUpdates(), Times.Once);
        }
        [Fact]
        public void SimulateCancellationCommand_ChangesVisibilityProps_True()
        {
            _viewModel.VisibilityTestingBtn = false;

            _viewModel.SimulateCancellationCommand.Execute(null);

            Assert.True(_viewModel.VisibilityTestingBtn);
        }

        [Fact]
        public void UpdateCancellationCommand_ChangesVisibilityProps_True()
        {
            _viewModel.VisibilityTestingBtn = false;

            _viewModel.UpdateCancellationCommand.Execute(null);

            Assert.True(_viewModel.VisibilityTestingBtn);
        }
        //MOQ SETUP FOR _simulateDataMock.SimulateResultsAsync
        [Fact]
        public void OnSimulateResultsExecuteAsync_CallSimulateResultsAsync_True()
        {
            _viewModel.Category = "I";
            _viewModel.City = "Wro";
            _viewModel.Distance = "1600";
            _viewModel.RaceNo = "1";
            _viewModel.RaceDate = new DateTime(2018, 11, 11);
            _viewModel.SimulateResultsCommand.Execute(null);

            _simulateDataMock.Verify(sd => sd.SimulateResultsAsync(0, _viewModel.Races.Count, _viewModel.Races, _viewModel.Horses, _viewModel.Jockeys, _viewModel.RaceModelProvider), Times.Once);

            Assert.Equal(1.1, _viewModel.Races[0].HorseList[0].WinIndex);
            Assert.Equal(2.2, _viewModel.Races[0].HorseList[1].WinIndex);
            Assert.Equal(3.3, _viewModel.Races[1].HorseList[0].WinIndex);
            Assert.Equal(4.4, _viewModel.Races[1].HorseList[1].WinIndex);
        }

        [Fact]
        public void OnClearDataExecute_ClearsRaceProps_True()
        {
            _viewModel.Category = "I";
            _viewModel.City = "Wro";
            _viewModel.Distance = "1600";
            _viewModel.RaceNo = "1";
            _viewModel.RaceDate = new DateTime(2018, 11, 11);
            _viewModel.HorseList.Clear();
            _viewModel.HorseList.Add(new HorseDataWrapper { HorseName = "Eugeniusz", Age = 1, Father = "Żytomir", Comments = "No comment" });
            _viewModel.HorseList.Add(new HorseDataWrapper { HorseName = "Eustachy", Age = 2, Father = "Dobrosława", Comments = "Some comment" });

            _viewModel.ClearDataCommand.Execute(null);

            Assert.Empty(_viewModel.HorseList);
            Assert.Equal("fill up", _viewModel.Category);
            Assert.Equal("-", _viewModel.City);
            Assert.Equal("0", _viewModel.Distance);
            Assert.Equal("0", _viewModel.RaceNo);
            Assert.Equal(DateTime.Now.Date, _viewModel.RaceDate.Date);
            Assert.False(_viewModel.IsSaveEnabled);
        }

        [Fact]
        public void OnNewHorseExecute_AddsNewHorse_True()
        {
            _viewModel.NewHorseCommand.Execute(null);
            _viewModel.NewHorseCommand.Execute(null);

            Assert.Equal(2, _viewModel.HorseList.Count);
            Assert.False(_viewModel.HorseList[0] == null);
        }

        [Theory]
        [InlineData("--Not found--", "")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData(null, "")]
        [InlineData("hereajam666", "Hereajam666")]
        [InlineData("hereajam", "Hereajam")]
        [InlineData("here aj am", "Here Aj Am")]
        [InlineData("here aj am 666", "Here Aj Am 666")]
        [InlineData("666", "666")]
        public void MakeTitleCase_ShouldReturnString_True(string name, string expected)
        {
            var result = _viewModel.MakeTitleCase(name);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ResetControls_ChangesVisibilityProps_True()
        {
            _viewModel.VisibilityTestingBtn = false;
            _viewModel.VisibilityCancellingMsg = false;
            _viewModel.AllControlsEnabled = false;

            _viewModel.SimulateResultsCommand.Execute(null);

            Assert.True(_viewModel.VisibilityTestingBtn);
            Assert.False(_viewModel.VisibilityCancellingMsg);
            Assert.True(_viewModel.AllControlsEnabled);
        }
    }
}
