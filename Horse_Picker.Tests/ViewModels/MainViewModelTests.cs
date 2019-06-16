using Horse_Picker.DataProvider;
using Horse_Picker.Dialogs;
using Horse_Picker.Models;
using Horse_Picker.NewModels;
using Horse_Picker.ViewModels;
using Horse_Picker.Wrappers;
using Moq;
using System;
using System.Collections.Generic;
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
        private Mock<IMessageDialogService> _messageDialogServicesMock;
        private Mock<IFileDataServices> _dataServices;
        private Mock<IScrapDataServices> _scrapServices;

        public MainViewModelTests()
        {
            _messageDialogServicesMock = new Mock<IMessageDialogService>();
            _dataServices = new Mock<IFileDataServices>();
            _scrapServices = new Mock<IScrapDataServices>();

            //moq setup
            _dataServices.Setup(ds => ds.GetAllHorses())
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
                    }
                });

            _dataServices.Setup(ds => ds.GetAllJockeys())
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
                    }
                });
            _dataServices.Setup(ds => ds.GetAllRaces())
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

            _viewModel = new MainViewModel(_dataServices.Object, _scrapServices.Object, _messageDialogServicesMock.Object);
        }

        [Fact]
        public void LoadAllData_ShouldLoadHorses_True()
        {
            _viewModel.LoadAllData();

            Assert.Equal(2, _viewModel.Horses.Count); //counts horses
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

            Assert.Equal(2, _viewModel.Jockeys.Count); //counts jockeys
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

            await Task.Run(() => _viewModel.TestResultsCommand.Execute(null));

            Assert.Equal(cancellationToken.CanBeCanceled, _viewModel.CancellationToken.CanBeCanceled);
            Assert.Equal(cancellationToken.IsCancellationRequested, _viewModel.CancellationToken.IsCancellationRequested);
        }

        [Fact]
        public void OnPickHorseDataExecute_ShouldAssignHorseWrapper_True()
        {
            HorseDataWrapper horse = new HorseDataWrapper(){ HorseName = "Trim, 7", Jockey = "N. Hendzel" };

            _viewModel.PickHorseDataCommand.Execute(horse);

            Assert.Equal(horse.HorseName, _viewModel.HorseWrapper.HorseName);
            Assert.Equal(horse.Jockey, _viewModel.HorseWrapper.Jockey);
        }

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

        // <- test OnUpdateDataExecuteAsync CONTINUE!

        [Fact]
        public void OnTaskCancellationExecute_CancelTask_True()
        {
            _viewModel.TaskCancellation = true;
            _viewModel.TokenSource = new CancellationTokenSource();

            _viewModel.TaskCancellationCommand.Execute(null);

            Assert.True(_viewModel.TokenSource.IsCancellationRequested);
            Assert.True(!_viewModel.TaskCancellation);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.True(_viewModel.ProgressDisplay == "");
            Assert.True(_viewModel.WorkStatus == "");
        }

        [Fact]
        public void CommandCompletedControlsSetup_ChangesCancellationProps_True()
        {
            _viewModel.CommandCompletedControlsSetup();

            Assert.True(!_viewModel.TaskCancellation);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.Equal(0, _viewModel.UpdateStatusBar);
            Assert.True(_viewModel.ProgressDisplay == "");
            Assert.True(_viewModel.WorkStatus == "");
        }

        [Fact]
        public void OnClearDataExecute_ClearsRaceProps_True()
        {
            _viewModel.HorseList.Add(new HorseDataWrapper { HorseName = "Eugeniusz", Age = 1, Father = "Żytomir", Comments = "No comment" });
            _viewModel.HorseList.Add(new HorseDataWrapper { HorseName = "Eustachy", Age = 1, Father = "Dobrosława", Comments = "No comment" });
            _viewModel.Category = "I";
            _viewModel.City = "Waw";
            _viewModel.Distance = "1600";
            _viewModel.RaceNo = "1";
            _viewModel.RaceDate = new DateTime(2018, 11, 11);

            _viewModel.ClearDataCommand.Execute(null);

            Assert.Empty(_viewModel.HorseList);
            Assert.Equal("fill up", _viewModel.Category);
            Assert.Equal("-", _viewModel.City);
            Assert.Equal("0", _viewModel.Distance);
            Assert.Equal("0", _viewModel.RaceNo);
            Assert.Equal(DateTime.Now, _viewModel.RaceDate);

        }
    }
}
