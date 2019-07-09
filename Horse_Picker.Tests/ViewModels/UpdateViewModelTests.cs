using Horse_Picker.Events;
using Horse_Picker.ViewModels;
using Moq;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Horse_Picker.Tests.ViewModels
{
    public class UpdateViewModelTests
    {
        private UpdateViewModel _viewModel;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<DataUpdateEvent> _dataUpdateEventMock;

        public UpdateViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _dataUpdateEventMock = new Mock<DataUpdateEvent>();

            _eventAggregatorMock.Setup(ea => ea.GetEvent<DataUpdateEvent>())
              .Returns(_dataUpdateEventMock.Object);

            _viewModel = new UpdateViewModel(_eventAggregatorMock.Object);
        }

        [Fact]
        public void RunTheUpdate_PublishesUpdateModules()
        {
            _viewModel.RunTheUpdate();

            _dataUpdateEventMock.Verify(du => du.Publish(_viewModel.UpdateModulesModel), Times.Exactly(2)); //once for VM instance
        }
    }
}
