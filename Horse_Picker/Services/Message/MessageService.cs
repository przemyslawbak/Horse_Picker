using Horse_Picker.ViewModels;
using Horse_Picker.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace Horse_Picker.Services.Message
{
    public class MessageService : IMessageService
    {
        public MessageDialogResult ShowUpdateWindow()
        {
            return new UpdateWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = App.Current.MainWindow
            }.ShowDialog().GetValueOrDefault()
              ? MessageDialogResult.Update //if DialogResult = true;
              : MessageDialogResult.Cancel; //if DialogResult = false;
        }
    }
}
