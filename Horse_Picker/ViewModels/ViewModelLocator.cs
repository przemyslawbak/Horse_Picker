using Autofac;
using Horse_Picker.Startup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.ViewModels
{
    public class ViewModelLocator
    {
        IContainer container = BootStrapper.BootStrap();

        public MainViewModel MainViewModel
        {
            get { return container.Resolve<MainViewModel>(); }
        }

        public UpdateViewModel UpdateViewModel
        {
            get { return container.Resolve<UpdateViewModel>(); }
        }
    }
}
