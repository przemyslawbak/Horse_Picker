using Autofac;
using Horse_Picker.DataProvider;
using Horse_Picker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Startup
{
    public class BootStrapper
    {
        public IContainer BootStrap()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FileDataServices>()
              .As<IFileDataServices>().SingleInstance();

            builder.RegisterType<ScrapDataServices>()
              .As<IScrapDataServices>().SingleInstance();

            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf();

            return builder.Build();
        }
    }
}
