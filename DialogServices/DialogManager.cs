using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Horse_Picker.Dialogs
{
    public class MessageBoxDisplay
    {
        public void ShowMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    class WindowDialog : FrameworkElement
    {
        public void ShowWindow(object viewModel, int height, int width, string title)
        {
            var win = new Window();
            win.Content = viewModel;
            win.MinHeight = height;
            win.MaxHeight = height;
            win.MinWidth = width;
            win.MaxWidth = width;
            win.Title = title;
            win.ShowDialog();
        }
    }
}
