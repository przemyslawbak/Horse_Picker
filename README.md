## Purpose

General purpose of the solution is to compute potentially best horse in a horse racing competition based on historic data available in subject related web services, and compare it with other horses starting in the race. The application is created for personal use and for educational purposes to practice different frameworks and approaches, and should not be use by ANYONE for making any betting decisions based on this application output. Author is taking no responsibility if someone loses any money when making decisions on the output of this solution.

## Features

1. Data for computations is provided by parsing HTML documents.
2. Parsed data collections are saved in JSON format files.
3. Horse_Picker computes several factors of the horse performance in the past:
  - factor based on horses age,
  - jockeys previous races,
  - win index computed on horse wins in the past,
  - index of siblings based on their win indexes,
  - results of the horse other than wins,
  - indexes based on horses rest time after last race and how often was racing.
4. The application allows to simulate all factors for horses in historic races and compare them with their results in the past.
5. Covering view models with unit tests.


# Technology

1. Approaches:
  - MVVM pattern,
  - async commands,
  - events (Prism and event handlers),
  - IoC (Autofac),
  - resolving view models in ViewModelLocator class,
  - model data wrapper,
  - generic service methods for various data types,
  - async methods,
  - parallel tasks triggering with using of SemaphoreSlim,
2. Application is using:
  - C#, WPF,
  - Autofac,
  - DotNetProjects.Wpf.Toolkit,
  - HtmlAgilityPack,
  - Newtonsoft.Json,
  - Prism.Core,
  - System.Windows.Interactivity.WPF
