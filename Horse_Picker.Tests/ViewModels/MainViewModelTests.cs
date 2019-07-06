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

        private DataUpdateEvent _dataUpdateEvent; //for subscriber only

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

            _eventAggregatorMock.Setup(ea => ea.GetEvent<DataUpdateEvent>())
              .Returns(_dataUpdateEvent);

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
        }

        [Fact]
        public void MainViewModelInstance_CallsLoadDataEventHandler_True()
        {
            Assert.True(_viewModel.WasCalled); //testing that event was called
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
        public void CancellationExecuteMethods_CallsCancelSimulation_True()
        {
            Mock<MainViewModel> mockVm = new Mock<MainViewModel>();

            _viewModel.SimulateCancellationCommand.Execute(null);
            _viewModel.UpdateCancellationCommand.Execute(null);

            _simulateDataMock.Verify(sd => sd.CancelSimulation(), Times.Once);
            _updateDataMock.Verify(sd => sd.CancelUpdates(), Times.Once);
        }

        /*
        [Fact]
        public void SimulateCancellationCommand_ChangesVisibilityProps_True()
        {
            _viewModel.VisibilityTestingBtn = Visibility.Hidden;

            _viewModel.SimulateCancellationCommand.Execute(null);

            Assert.Equal(Visibility.Visible, _viewModel.VisibilityTestingBtn);
        }

        [Fact]
        public void UpdateCancellationCommand_ChangesVisibilityProps_True()
        {
            _viewModel.VisibilityTestingBtn = Visibility.Hidden;

            _viewModel.UpdateCancellationCommand.Execute(null);

            Assert.Equal(Visibility.Visible, _viewModel.VisibilityTestingBtn);
        }

        [Fact]
        public void OnSimulateResultsExecuteAsync_ChangesVisibilityProps_True()
        {
            _viewModel.VisibilityTestingBtn = Visibility.Hidden;
            _viewModel.VisibilityCancellingMsg = Visibility.Collapsed;
            _viewModel.AllControlsEnabled = false;

            _viewModel.SimulateResultsCommand.Execute(null);

            Assert.Equal(Visibility.Visible, _viewModel.VisibilityTestingBtn);
            Assert.Equal(Visibility.Collapsed, _viewModel.VisibilityCancellingMsg);
            Assert.True(_viewModel.AllControlsEnabled);
        }
        */
        [Fact]
        public void OnClearDataExecute_ClearsRaceProps_True()
        {
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
    }
}
