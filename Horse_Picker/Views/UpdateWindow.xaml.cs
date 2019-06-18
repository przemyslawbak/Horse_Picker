using System.Windows;

namespace Horse_Picker.Views
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
            DataContext = Application.Current.MainWindow.DataContext;
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
