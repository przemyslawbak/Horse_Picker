using Horse_Picker.DataProvider;
using Horse_Picker.Dialogs;
using Horse_Picker.Models;
using Horse_Picker.NewModels;
using Horse_Picker.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
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
                        FatherLink = "https://koniewyscigowe.pl/horse/14474-belenus",
                        Link = "https://koniewyscigowe.pl/horse/260-trim"
                    },
                    new LoadedHorse
                    {
                        Name = "Belenus",
                        Age = 99,
                        AllChildren = { },
                        AllRaces = { },
                        Father = "-",
                        FatherLink = "",
                        Link = "https://koniewyscigowe.pl/horse/14474-belenus"
                    }
                });

            _dataServices.Setup(ds => ds.GetAllJockeys())
                .Returns(new List<LoadedJockey>
                {
                    new LoadedJockey
                    {
                        Name = "N. Hendzel",
                        Link = "https://koniewyscigowe.pl/dzokej?d=4-natalia-hendzel",
                        AllRaces = { }
                    },
                    new LoadedJockey
                    {
                        Name = "V. Popov",
                        Link = "https://koniewyscigowe.pl/dzokej?d=54-victor-popov",
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
        }

        [Fact]
        public void LoadAllData_ShouldLoadJockeys_True()
        {
            _viewModel.LoadAllData();
            Assert.Equal(2, _viewModel.Jockeys.Count); //counts jockeys
        }

        [Fact]
        public void LoadAllData_ShouldLoadHistoricRaces_True()
        {
            _viewModel.LoadAllData();
            Assert.Equal(2, _viewModel.Races.Count); //counts races
        }

        [Fact]//testing async void
        public async Task OnTestResultExecuteAsync_ShouldCreateCancellationTokenSource_True()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;
            await Task.Run(() => _viewModel.TestResultsCommand.Execute(null));
            Assert.Equal(cancellationToken.CanBeCanceled, _viewModel.CancellationToken.CanBeCanceled);
            Assert.Equal(cancellationToken.IsCancellationRequested, _viewModel.CancellationToken.IsCancellationRequested);
        }
    }
}
