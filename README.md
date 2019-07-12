## Purpose

General purpose of the application is to compute potentially best horse in a horse racing competition based on historic data available in the internet, and compare it with other horses starting in the race. The application is ceated for personal use and for educational purposes to practice different framewors and approaches.

## Features

Horse_Picker computes several factors of the horse performance in the past:
- factor based on horses age,
- jockeys previous races,
- win index computed on horse wins in the past
- index of siblings based on their win indexes,
- results of the horse other than wins,
- indexes based on horses rest time after last race and how often was racing.

Data for computations is provided by parsing HTML documents and saving saving it to JSON format files.

The application also allows to simulate all factors for horses in historic races and compare them with their results in the past.

## Frameworks

Application is using:
- Autofac
- DotNetProjects.Wpf.Toolkit
- HtmlAgilityPack
- Newtonsoft.Json
- Prism.Core
- System.Windows.Interactivity.WPF
