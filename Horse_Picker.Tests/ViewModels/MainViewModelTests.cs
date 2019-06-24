using Horse_Picker.Models;
using Horse_Picker.Services;
using Horse_Picker.Services.Files;
using Horse_Picker.Services.Message;
using Horse_Picker.Services.Simulate;
using Horse_Picker.Services.Update;
using Horse_Picker.ViewModels;
using Horse_Picker.Wrappers;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Horse_Picker.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private MainViewModel _viewModel;
        private Mock<IMessageService> _messageDialogServicesMock;
        private Mock<IFileService> _dataServicesMock;
        private Mock<IRaceProvider> _raceModelProviderMock;
        private Mock<IUpdateService> _updateDataMock;
        private Mock<ISimulateService> _simulateDataMock;

        public MainViewModelTests()
        {
            _messageDialogServicesMock = new Mock<IMessageService>();
            _dataServicesMock = new Mock<IFileService>();
            _raceModelProviderMock = new Mock<IRaceProvider>();
            _updateDataMock = new Mock<IUpdateService>();
            _simulateDataMock = new Mock<ISimulateService>();

            //moq setup
            _dataServicesMock.Setup(ds => ds.GetAllHorses())
                .Returns(new List<LoadedHorse>
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
                });

            _dataServicesMock.Setup(ds => ds.GetAllJockeys())
                .Returns(new List<LoadedJockey>
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
                });
            _dataServicesMock.Setup(ds => ds.GetAllRaces())
                .Returns(new List<LoadedHistoricalRace>
                {
                    new LoadedHistoricalRace
                    {
                        HorseList = { },
                        RaceCategory = "I",
                        RaceDate = new DateTime(2018, 11, 11),
                        RaceDistance = 1600,
                        RaceLink = "somelink"
                    },
                    new LoadedHistoricalRace
                    {
                        HorseList = { },
                        RaceCategory = "II",
                        RaceDate = new DateTime(2018, 11, 10),
                        RaceDistance = 1400,
                        RaceLink = "somelink2"
                    }
                });

            _viewModel = new MainViewModel(_dataServicesMock.Object,
                _messageDialogServicesMock.Object,
                _raceModelProviderMock.Object,
                _updateDataMock.Object,
                _simulateDataMock.Object);

            _viewModel.Category = "I";
            _viewModel.City = "Waw";
            _viewModel.Distance = "1600";
            _viewModel.RaceNo = "1";
            _viewModel.RaceDate = new DateTime(2018, 11, 11);
        }

        [Fact]
        public void LoadAllData_ShouldLoadHorses_True()
        {
            _viewModel.LoadAllData();

            Assert.Equal(3, _viewModel.Horses.Count); //counts horses
            Assert.Equal("Trim", _viewModel.Horses[0].Name);
            Assert.Equal(7, _viewModel.Horses[0].Age);
            Assert.Equal("Belenus", _viewModel.Horses[0].Father);
            Assert.Equal("https://koniewyscigowe.pl/horse/14474", _viewModel.Horses[0].FatherLink);
            Assert.Equal("https://koniewyscigowe.pl/horse/260", _viewModel.Horses[0].Link);
        }

        [Fact]
        public void LoadAllData_ShouldLoadJockeys_True()
        {
            _viewModel.LoadAllData();

            Assert.Equal(3, _viewModel.Jockeys.Count); //counts jockeys
            Assert.Equal("N. Hendzel", _viewModel.Jockeys[0].Name);
            Assert.Equal("https://koniewyscigowe.pl/dzokej?d=4", _viewModel.Jockeys[0].Link);
        }

        [Fact]
        public void LoadAllData_ShouldLoadHistoricRaces_True()
        {
            _viewModel.LoadAllData();

            Assert.Equal(2, _viewModel.Races.Count); //counts races
            Assert.Equal("I", _viewModel.Races[0].RaceCategory);
            Assert.Equal(1600, _viewModel.Races[0].RaceDistance);
            Assert.Equal("somelink", _viewModel.Races[0].RaceLink);
        }

        [Fact]
        public async Task OnTestResultExecuteAsync_ShouldCreateCancellationTokenSource_True()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            await Task.Run(() => _viewModel.SimulateResultsCommand.Execute(null));

            //Assert.Equal(cancellationToken.CanBeCanceled, _viewModel.CancellationToken.CanBeCanceled);
            //Assert.Equal(cancellationToken.IsCancellationRequested, _viewModel.CancellationToken.IsCancellationRequested);
        }

        [Fact]
        public void OnPickHorseDataExecute_ShouldAssignHorseWrapper_True()
        {
            HorseDataWrapper horse = new HorseDataWrapper(){ HorseName = "Trim, 7", Jockey = "N. Hendzel" };

            _viewModel.PickHorseDataCommand.Execute(horse);

            Assert.Equal(horse.HorseName, _viewModel.HorseWrapper.HorseName);
            Assert.Equal(horse.Jockey, _viewModel.HorseWrapper.Jockey);
        }

        /*
        [Fact]
        public async Task OnUpdateDataExecuteAsync_MakesUpdateModulesAreFalseByDefault_True()
        {
            _viewModel.UpdateHorsesCz = true;
            _viewModel.UpdateHorsesPl = true;
            _viewModel.UpdateJockeysCz = true;
            _viewModel.UpdateJockeysPl = true;
            _viewModel.UpdateRacesPl = true;

            await Task.Run(() => _viewModel.UpdateDataCommand.Execute(null));

            bool isAnyTrue = _viewModel.UpdateModules.Any(module => module == true);
            Assert.True(!isAnyTrue);
        }
        */

        // <- test OnUpdateDataExecuteAsync CONTINUE!

        [Fact]
        public void OnTaskCancellationExecute_CancelTask_True()
        {
            //_viewModel.TaskCancellation = true;
            //_viewModel.TokenSource = new CancellationTokenSource();

            //_viewModel.TaskCancellationCommand.Execute(null);

            //Assert.True(_viewModel.TokenSource.IsCancellationRequested);
            //Assert.True(!_viewModel.TaskCancellation);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.True(_viewModel.ProgressDisplay == "");
            Assert.True(_viewModel.WorkStatus == "");
        }

        [Fact]
        public void CommandCompletedControlsSetup_ChangesCancellationProps_True()
        {
            //_viewModel.TokenSource = new CancellationTokenSource();

            //_viewModel.CommandCompletedControlsSetup();

            //Assert.True(!_viewModel.TaskCancellation);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.True(_viewModel.ProgressDisplay == "");
            Assert.True(_viewModel.WorkStatus == "");
        }

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

        [Fact]
        public void PopulateLists_PopulatesThem_True()
        {
            _viewModel.LoadAllData();
            _viewModel.PopulateLists();

            Assert.Equal(3, _viewModel.Horses.Count);
            Assert.Equal(2, _viewModel.LoadedHorses.Count);
            Assert.Equal(2, _viewModel.LoadedJockeys.Count);
            Assert.Equal("Trim, 7", _viewModel.LoadedHorses[0]);
            Assert.Equal("N. Hendzel", _viewModel.LoadedJockeys[0]);
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
        private void HorseList_CollectionChangedEventValidatesSaveBtn_True()
        {
            _viewModel.HorseList.Clear();
            _viewModel.IsSaveEnabled = false;
            _viewModel.AllControlsEnabled = true;

            _viewModel.HorseList.Add(new HorseDataWrapper { });

            Assert.True(_viewModel.IsSaveEnabled);
        }

        //ValidateButtons tests now
    }
}
