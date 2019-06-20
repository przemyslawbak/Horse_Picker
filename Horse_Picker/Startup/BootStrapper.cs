using Autofac;
using Horse_Picker.Services;
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

            builder.RegisterType<SimulateDataService>()
              .As<ISimulateDataService>().SingleInstance();

            builder.RegisterType<UpdateDataService>()
              .As<IUpdateDataService>().SingleInstance();

            builder.RegisterType<RaceModelProvider>()
              .As<IRaceModelProvider>().SingleInstance();

            builder.RegisterType<FileDataService>()
              .As<IFileDataService>().SingleInstance();

            builder.RegisterType<ComputeDataService>()
              .As<IComputeDataService>().SingleInstance();

            builder.RegisterType<ScrapDataService>()
              .As<IScrapDataService>().SingleInstance();

            builder.RegisterType<MessageDialogService>()
              .As<IMessageDialogService>().SingleInstance();

            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
