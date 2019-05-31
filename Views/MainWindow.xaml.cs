using Autofac;
using Horse_Picker.Startup;
using Horse_Picker.ViewModels;
using System.Windows;

namespace Horse_Picker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var bootStrapper = new BootStrapper();
            var container = bootStrapper.BootStrap();
            MainViewModel vm = container.Resolve<MainViewModel>();
            InitializeComponent();
            DataContext = vm;
        }
    }
}
