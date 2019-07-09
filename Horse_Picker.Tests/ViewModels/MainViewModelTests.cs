using Horse_Picker.Events;
using Horse_Picker.Models;
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
using System.Threading.Tasks;
using Xunit;

namespace Horse_Picker.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private MainViewModel _viewModel;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IMessageService> _messageDialogServicesMock;
        private Mock<IFileService> _dataServicesMock;
        private Mock<IUpdateService> _updateDataMock;
        private Mock<ISimulateService> _simulateDataMock;
        private Mock<IDictionariesService> _dictionaryServiceMock;

        private DataUpdateEvent _dataUpdateEvent; //for subscriber
        private ProgressBarEvent _progressBarEvent; //for subscriber
        private LoadDataEvent _loadDataEvent; //for subscriber

        private List<RaceDetails> _races = new List<RaceDetails>() { new RaceDetails {
                        HorseList = new List<HorseDataWrapper>() { new HorseDataWrapper() { HorseName = "Dora", Age = 9, Father = "Some Father 3" } },
                        RaceCategory = "II", RaceDate = new DateTime(2019, 11, 10), RaceDistance = 1400, RaceLink = "somelink2" } };
        private List<RaceDetails> _racesSimulated = new List<RaceDetails>() { new RaceDetails {
                        HorseList = new List<HorseDataWrapper>() { new HorseDataWrapper() { HorseName = "Dora", Age = 9, Father = "Some Father 3", WinIndex = 1.1 } },
                        RaceCategory = "II", RaceDate = new DateTime(2019, 11, 10), RaceDistance = 1400, RaceLink = "somelink2" } };
        private List<LoadedHorse> _horses = new List<LoadedHorse>() { new LoadedHorse {
            Name = "Trim", Age = 7, AllChildren = { }, AllRaces = { }, Father = "Belenus", FatherLink = "https://koniewyscigowe.pl/horse/14474", Link = "https://koniewyscigowe.pl/horse/260" } };
        private List<LoadedHorse> _horsesSimulated = new List<LoadedHorse>() { new LoadedHorse {
            Name = "Some Other Horse", Age = 7, AllChildren = { }, AllRaces = { }, Father = "Belenus", FatherLink = "https://koniewyscigowe.pl/horse/14474", Link = "https://koniewyscigowe.pl/horse/260" } };
        private List<LoadedJockey> _jockeys = new List<LoadedJockey>() { new LoadedJockey {
            Name = "N. Hendzel", Link = "https://koniewyscigowe.pl/dzokej?d=4", AllRaces = { } } };
        private List<LoadedJockey> _jockeysSimulated = new List<LoadedJockey>() { new LoadedJockey {
            Name = "Some Other Jockey", Link = "https://koniewyscigowe.pl/dzokej?d=4", AllRaces = { } } };
        private HorseDataWrapper _horsePicked = new HorseDataWrapper() { Age = 7, HorseName = "Trim", Father = "Some Father" };


        public MainViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _messageDialogServicesMock = new Mock<IMessageService>();
            _dataServicesMock = new Mock<IFileService>();
            _updateDataMock = new Mock<IUpdateService>();
            _simulateDataMock = new Mock<ISimulateService>();
            _dictionaryServiceMock = new Mock<IDictionariesService>();

            _dataUpdateEvent = new DataUpdateEvent();
            _progressBarEvent = new ProgressBarEvent();
            _loadDataEvent = new LoadDataEvent();

            _eventAggregatorMock.Setup(ea => ea.GetEvent<DataUpdateEvent>())
              .Returns(_dataUpdateEvent);
            _eventAggregatorMock.Setup(ea => ea.GetEvent<ProgressBarEvent>())
              .Returns(_progressBarEvent);
            _eventAggregatorMock.Setup(ea => ea.GetEvent<LoadDataEvent>())
              .Returns(_loadDataEvent);

            //moq setup
            _dataServicesMock.Setup(ds => ds.GetAllHorsesAsync())
                .ReturnsAsync(_horses);

            _dataServicesMock.Setup(ds => ds.GetAllJockeysAsync())
                .ReturnsAsync(_jockeys);

            _dataServicesMock.Setup(ds => ds.GetAllRacesAsync())
                .ReturnsAsync(_races);

            _simulateDataMock.Setup(ds => ds.SimulateResultsAsync(It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<RaceDetails>>(),
                It.IsAny<List<LoadedHorse>>(),
                It.IsAny<List<LoadedJockey>>(),
                It.IsAny<RaceModel>()))
                .ReturnsAsync(_racesSimulated);

            _updateDataMock.Setup(ud => ud.GetParsedHorseData(It.IsAny<HorseDataWrapper>(),
                It.IsAny<DateTime>(),
                It.IsAny<List<LoadedHorse>>(),
                It.IsAny<List<LoadedJockey>>(),
                It.IsAny<RaceModel>()))
                .Returns(_horsePicked);

            _updateDataMock.Setup(ud => ud.UpdateDataAsync(It.IsAny<List<LoadedJockey>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_jockeysSimulated);
            _updateDataMock.Setup(ud => ud.UpdateDataAsync(It.IsAny<List<LoadedHorse>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_horsesSimulated);
            _updateDataMock.Setup(ud => ud.UpdateDataAsync(It.IsAny<List<RaceDetails>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_racesSimulated);

            _viewModel = new MainViewModel(_dataServicesMock.Object,
                _messageDialogServicesMock.Object,
                _updateDataMock.Object,
                _simulateDataMock.Object,
                _eventAggregatorMock.Object,
                _dictionaryServiceMock.Object);
        }

        [Fact]
        public void DataUpdateEvent_Publish_UpdatesDataUpdateModulesProperty()
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
        public void LoadDataEvent_Publish_LoadsData()
        {
            _viewModel.Horses.Clear();
            _viewModel.Jockeys.Clear();
            _viewModel.Races.Clear();

            _loadDataEvent.Publish();

            Assert.NotEmpty(_viewModel.Horses);
            Assert.NotEmpty(_viewModel.Jockeys);
            Assert.NotEmpty(_viewModel.Races);
            Assert.NotEmpty(_viewModel.LoadedHorses);
            Assert.NotEmpty(_viewModel.LoadedJockeys);
        }

        [Fact]
        public void ProgressBarEvent_Publish_UpdatesProperties()
        {
            ProgressBarData bar = new ProgressBarData()
            {
                JobType = "unit test",
                LoopCouner = 100,
                FromId = 10,
                ToId = 1010
            };

            _progressBarEvent.Publish(bar);

            Assert.Equal(10, _viewModel.UpdateStatusBar);
            Assert.Equal("unit test", _viewModel.WorkStatus);
            Assert.Equal("100 / 1000", _viewModel.ProgressDisplay);

        }

        [Fact]
        public void OnSimulateCancellationExecute_CallsCancelSimulation()
        {
            _viewModel.SimulateCancellationCommand.Execute(null);

            _simulateDataMock.Verify(sd => sd.CancelSimulation(), Times.Once);
        }

        [Fact]
        public void OnUpdateCancellationExecute_CallsCancelUpdates()
        {
            _viewModel.UpdateCancellationCommand.Execute(null);

            _updateDataMock.Verify(sd => sd.CancelUpdates(), Times.Once);
        }

        [Fact]
        public async Task OnSimulateResultsExecuteAsync_CallsSimulateResultsAsync()
        {
            await _viewModel.SimulateResultsCommand.ExecuteAsync(null);

            _simulateDataMock.Verify(sd => sd.SimulateResultsAsync(It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<RaceDetails>>(),
                It.IsAny<List<LoadedHorse>>(),
                It.IsAny<List<LoadedJockey>>(),
                It.IsAny<RaceModel>()), Times.Once);
        }

        [Fact]
        public async Task OnSimulateResultsExecuteAsync_UpdatesRacesCollection()
        {
            await _viewModel.SimulateResultsCommand.ExecuteAsync(null);

            Assert.Equal(1.1, _viewModel.Races[0].HorseList[0].WinIndex);
        }

        [Fact]
        public void OnPickHorseDataExecute_CallsGetParsedHorseData()
        {
            HorseDataWrapper horse = new HorseDataWrapper();

            _viewModel.PickHorseDataCommand.Execute(horse);

            _updateDataMock.Verify(sd => sd.GetParsedHorseData(It.IsAny<HorseDataWrapper>(),
                It.IsAny<DateTime>(),
                It.IsAny<List<LoadedHorse>>(),
                It.IsAny<List<LoadedJockey>>(),
                It.IsAny<RaceModel>()), Times.Once);
        }

        [Fact]
        public void OnPickHorseDataExecute_UpdatesHorseObject()
        {
            HorseDataWrapper horse = new HorseDataWrapper();

            _viewModel.PickHorseDataCommand.Execute(horse);

            Assert.Equal("Trim", _viewModel.HorseWrapper.HorseName);
            Assert.Equal(7, _viewModel.HorseWrapper.Age);
            Assert.Equal("Some Father", _viewModel.HorseWrapper.Father);
        }

        [Fact]
        public void OnUpdateDataExecuteAsync_PopulatesUpdateModulesCollection()
        {
            _viewModel.UpdateDataCommand.Execute(null);

            Assert.Equal(5, _viewModel.UpdateModules.Count);
        }

        [Theory]
        [InlineData(MessageDialogResult.Update, 1)]
        [InlineData(MessageDialogResult.Cancel, 0)]
        public void OnUpdateDataExecuteAsync_IfResultIsUpdate_CallsUpdateDataAsync(MessageDialogResult result, int expectedMethodCalls)
        {
            _messageDialogServicesMock.Setup(md => md.ShowUpdateWindow()).Returns(result);
            _viewModel.DataUpdateModules.JockeysPl = true;
            _viewModel.DataUpdateModules.HorsesPl = true;
            _viewModel.DataUpdateModules.RacesPl = true;

            _viewModel.UpdateDataCommand.Execute(null);

            _updateDataMock.Verify(ud => ud.UpdateDataAsync(It.IsAny<List<LoadedJockey>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(expectedMethodCalls));
            _updateDataMock.Verify(ud => ud.UpdateDataAsync(It.IsAny<List<LoadedHorse>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(expectedMethodCalls));
            _updateDataMock.Verify(ud => ud.UpdateDataAsync(It.IsAny<List<RaceDetails>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(expectedMethodCalls));
        }

        [Fact]
        public void OnUpdateDataExecuteAsync_IfResultIsUpdate_UpdatesCollections()
        {
            _messageDialogServicesMock.Setup(md => md.ShowUpdateWindow()).Returns(MessageDialogResult.Update);
            _viewModel.DataUpdateModules.JockeysPl = true;
            _viewModel.DataUpdateModules.HorsesPl = true;
            _viewModel.DataUpdateModules.RacesPl = true;

            _viewModel.UpdateDataCommand.Execute(null);

            Assert.Equal("Some Other Jockey", _viewModel.Jockeys[0].Name);
            Assert.Equal("Some Other Horse", _viewModel.Horses[0].Name);
            Assert.Equal(1.1, _viewModel.Races[0].HorseList[0].WinIndex);
        }


    }
}