﻿using Autofac;
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

            builder.RegisterType<FileDataServices>()
              .As<IFileDataServices>().SingleInstance();

            builder.RegisterType<ComputeDataServices>()
              .As<IComputeDataServices>().SingleInstance();

            builder.RegisterType<ScrapDataServices>()
              .As<IScrapDataServices>().SingleInstance();

            builder.RegisterType<MessageDialogService>()
              .As<IMessageDialogService>().SingleInstance();

            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
