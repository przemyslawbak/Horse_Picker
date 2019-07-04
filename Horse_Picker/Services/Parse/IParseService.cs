using System;
using Horse_Picker.Models;
using Horse_Picker.Wrappers;

namespace Horse_Picker.Services.Parse
{
    public interface IParseService
    {
        string SplitName(string jobType, string nodeString, Type type, string propertyName);
        string SplitLink(string nodeString, string jobType, string propertyName);
        HorseDataWrapper SplitHorseNodeString(string jobType, string nodeString, string propertyName, DateTime raceDate);
        RaceDetails SplitRaceNodeString(string jobType, string nodeString, string propertyName);
        HorseChildDetails SplitChildNodeString(string jobType, string nodeString, string propertyName, DateTime raceDate);
        HorseDataWrapper ParseHorseData(DateTime raceDate, string name, string age, string link, string jockey, string jobType);
        string SplitAge(string jobType, string nodeString, DateTime raceDate, string propertyName);
        LoadedJockey ParseJockeyData(string link, string name);
        bool VerifyNodeCondition(string nodeString, string propertyName, string jobType);
    }
}