using Autofac;
using Horse_Picker.Services;
using Horse_Picker.Services.Compute;
using Horse_Picker.Services.Files;
using Horse_Picker.Services.Message;
using Horse_Picker.Services.Scrap;
using Horse_Picker.Services.Simulate;
using Horse_Picker.Services.Update;
using Horse_Picker.ViewModels;
using Horse_Picker.Views;
using Prism.Events;

namespace Horse_Picker.Startup
{
    public class BootStrapper
    {
        public static IContainer BootStrap()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<EventAggregator>()
              .As<IEventAggregator>().SingleInstance();

            builder.RegisterType<SimulateService>()
              .As<ISimulateService>().SingleInstance();

            builder.RegisterType<UpdateService>()
              .As<IUpdateService>().SingleInstance();

            builder.RegisterType<RaceProvider>()
              .As<IRaceProvider>().SingleInstance();

            builder.RegisterType<FileService>()
              .As<IFileService>().SingleInstance();

            builder.RegisterType<ComputeService>()
              .As<IComputeService>().SingleInstance();

            builder.RegisterType<ScrapService>()
              .As<IScrapService>().SingleInstance();

            builder.RegisterType<MessageService>()
              .As<IMessageService>().SingleInstance();

            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<UpdateWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<UpdateViewModel>().AsSelf();

            return builder.Build();
        }
    }
}
