using Horse_Picker.DataProvider;
using Horse_Picker.Dialogs;
using Horse_Picker.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Horse_Picker.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private MainViewModel _viewModel;
        private Mock<IMessageDialogService> _messageDialogServiceMock;
        private Mock<IFileDataServices> _dataServices;
        private Mock<IScrapDataServices> _scrapServices;

        public MainViewModelTests()
        {
            _messageDialogServiceMock = new Mock<IMessageDialogService>();
            _dataServices = new Mock<IFileDataServices>();
            _scrapServices = new Mock<IScrapDataServices>();
            _viewModel = new MainViewModel(_dataServices.Object, _scrapServices.Object, _messageDialogServiceMock.Object);
        }
    }
}
