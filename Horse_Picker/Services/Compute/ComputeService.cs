using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Horse_Picker.Models;

namespace Horse_Picker.Services.Compute
{
    public class ComputeService : IComputeService
    {
        //compute props
        public int Counter { get; set; }
        public int DictValue { get; set; }
        public double FinalResult { get; set; }
        public double Result { get; set; }
        public double SiblingIndex { get; set; }
        public LoadedHorse ChildFromList { get; set; }

        //compute loop fields
        public double PlaceFactor { get; set; }
        public double DistanceRaceIndex { get; set; }
        public double DistanceFactor { get; set; }

        /// <summary>
        /// resets properties used in various methods
        /// </summary>
        public void ResetComputeVariables()
        {
            DictValue = 1;
            FinalResult = 0;
            Result = 0;
            SiblingIndex = 0;
            Counter = 0;
            ChildFromList = new LoadedHorse();
        }

        /// <summary>
        /// resets properties used in various loops
        /// </summary>
        public void ResetLoopVariables()
        {
            PlaceFactor = 0;
            DistanceRaceIndex = 0;
            DistanceFactor = 0;
        }

        /// <summary>
        /// calculated AI factor
        /// </summary>
        /// <param name="horseFromList"></param>
        /// <param name="date"></param>
        /// <returns>returns AI</returns>
        public double ComputeAgeIndex(LoadedHorse horseFromList, DateTime date)
        {
            int yearsDifference = DateTime.Now.Year - date.Year; //how many years passed since race

            int horsesRaceAge = horseFromList.Age - yearsDifference; //how old was horse

            if (horsesRaceAge > 5)
            {
                int multiplier = horsesRaceAge - 4;
                double result = 1 + 0.3 * multiplier;
                return result;
            }
            else
            {
                return 1;
            }

        }

        /// <summary>
        /// calculated CI factor
        /// </summary>
        /// <param name="horseFromList">horse data</param>
        /// <param name="date">day of the race</param>
        /// <returns>returns CI</returns>
        public double ComputeCategoryIndex(LoadedHorse horseFromList, DateTime date, RaceModel raceModelProvider, Dictionary<string, int> raceCategoryDictionary)
        {
            Dictionary<string, int> raceDictionary = raceCategoryDictionary;

            ResetComputeVariables();

            if (horseFromList.AllRaces.Count > 0)
            {
                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    ResetLoopVariables();

                    if (horseFromList.AllRaces[i].WonPlace > 0 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        PlaceFactor = (double)horseFromList.AllRaces[i].WonPlace / horseFromList.AllRaces[i].RaceCompetition * 10;

                        //increase factor for races over 12 horses and place between 1-4
                        if (horseFromList.AllRaces[i].RaceCompetition > 12 && horseFromList.AllRaces[i].WonPlace < 5)
                        {
                            PlaceFactor = PlaceFactor * 1.5;
                        }

                        bool foundKey = raceDictionary.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            DictValue = raceDictionary[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            DictValue = 5;
                        }

                        DistanceFactor = (double)(horseFromList.AllRaces[i].RaceDistance - int.Parse(raceModelProvider.Distance)) / 10000 * DictValue;
                        DistanceFactor = Math.Abs(DistanceFactor);

                        DistanceRaceIndex = PlaceFactor * DictValue / 10;

                        Result = Result + DistanceRaceIndex - DistanceFactor;
                    }
                }

                FinalResult = Result / horseFromList.AllRaces.Count;

                return FinalResult;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// calculated JI factor
        /// </summary>
        /// <param name="jockeyFromList">name of the jockey</param>
        /// <param name="date">day of the race</param>
        /// <returns>returns JI</returns>
        public double ComputeJockeyIndex(LoadedJockey jockeyFromList, DateTime date)
        {
            ResetComputeVariables();

            if (jockeyFromList.AllRaces.Count > 0)
            {
                for (int i = 0; i < jockeyFromList.AllRaces.Count; i++)
                {
                    ResetLoopVariables();

                    if (jockeyFromList.AllRaces[i].WonPlace > 0 && jockeyFromList.AllRaces[i].RaceDate < date)
                    {
                        if (jockeyFromList.AllRaces[i].WonPlace == 1)
                            PlaceFactor = 1;
                        if (jockeyFromList.AllRaces[i].WonPlace == 2)
                            PlaceFactor = 0.7;

                        DistanceRaceIndex = PlaceFactor * jockeyFromList.AllRaces[i].RaceCompetition / 10;

                        Result = Result + DistanceRaceIndex;
                    }
                }

                FinalResult = Result / jockeyFromList.AllRaces.Count;

                return FinalResult;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// calculated PI factor
        /// </summary>
        /// <param name="horseFromList"></param>
        /// <param name="date"></param>
        /// <returns>returns PI</returns>
        public double ComputePercentageIndex(LoadedHorse horseFromList, DateTime date)
        {
            ResetComputeVariables();

            if (horseFromList.AllRaces.Count > 0)
            {

                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    if (horseFromList.AllRaces[i].WonPlace == 1 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        Result++;
                    }
                }

                double percentage = (double)Result / horseFromList.AllRaces.Count * 100;

                if (percentage > 20)
                {
                    FinalResult = 1 + (percentage - 20) * 0.1;
                }
                else
                {
                    FinalResult = 1;
                }

                return FinalResult;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// calculated RI factor
        /// </summary>
        /// <param name="horseFromList"></param>
        /// <param name="date"></param>
        /// <returns>returns RI</returns>
        public double ComputeRestIndex(LoadedHorse horseFromList, DateTime date)
        {
            ResetComputeVariables();

            if (horseFromList.AllRaces.Count > 0)
            {
                //get races only before race date, sort by race date from biggest to smallest
                horseFromList.AllRaces = horseFromList.AllRaces.Where(l => l.RaceDate < date).OrderByDescending(l => l.RaceDate).ToList();

                if (horseFromList.AllRaces.Count == 0)
                    return 1;

                Result = (date - horseFromList.AllRaces[0].RaceDate).TotalDays;

                if (Result > 90)
                {
                    FinalResult = 1 + (Result - 90) / 100;
                }
                else if (Result < 60)
                {
                    FinalResult = 1 + Result / 50;
                }
                else
                {
                    FinalResult = 1;
                }

                return FinalResult;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// calculated SI factor
        /// </summary>
        /// <param name="fatherFromList">data of horses father</param>
        /// <param name="date">day of the race</param>
        /// <returns>returns SI</returns>
        public double ComputeSiblingsIndex(LoadedHorse fatherFromList,
            DateTime date,
            RaceModel raceModelProvider,
            List<LoadedHorse> horses,
            Dictionary<string, int> raceCategoryDictionary)
        {
            ResetComputeVariables();

            for (int i = 0; i < fatherFromList.AllChildren.Count; i++)
            {
                horses = new List<LoadedHorse>(horses.OrderBy(l => l.Age)); //from smallest to biggest
                HorseChildDetails child = fatherFromList.AllChildren[i];

                if (child.ChildAge == 0)
                {
                    ChildFromList = horses
                                .Where(h => h.Name.ToLower() == child
                                .ChildName.ToLower())
                                .FirstOrDefault();
                }
                else
                {
                    ChildFromList = horses
                                .Where(h => h.Name.ToLower() == child.ChildName.ToLower())
                                .Where(h => h.Age == child.ChildAge)
                                .FirstOrDefault();
                }

                if (ChildFromList != null && ChildFromList.AllRaces.Count > 0)
                {
                    SiblingIndex = ComputeWinIndex(ChildFromList, date, null, raceModelProvider, raceCategoryDictionary);
                    Counter++;
                }
                else
                {
                    SiblingIndex = 0;
                }

                Result = Result + SiblingIndex;
            }

            if (Counter != 0)
            {
                FinalResult = Result / Counter;
            }
            else
            {
                FinalResult = 0;
            }

            return FinalResult;
        }

        /// <summary>
        /// calculated TI factor
        /// </summary>
        /// <param name="horseFromList"></param>
        /// <param name="date"></param>
        /// <returns>returns TI</returns>
        public double ComputeTiredIndex(LoadedHorse horseFromList, DateTime date)
        {
            ResetComputeVariables();

            if (horseFromList.AllRaces.Count > 0)
            {
                //get races only before race date, sort by race date from biggest to smallest
                horseFromList.AllRaces = horseFromList.AllRaces.Where(l => l.RaceDate < date).OrderByDescending(l => l.RaceDate).ToList();

                if (horseFromList.AllRaces.Count == 0)
                    return 1;

                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    DateTime twoYearsBack = date.AddYears(-2);

                    //for all races 2 years back from this race
                    if (horseFromList.AllRaces[i].RaceDate < date && horseFromList.AllRaces[i].RaceDate > twoYearsBack)
                    {
                        Counter++;
                    }
                }

                if (Counter > 12)
                {
                    FinalResult = 1 + (Counter - 12) * 0.1;
                }
                else
                {
                    return 1;
                }

                return FinalResult;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// calculated WI factor
        /// </summary>
        /// <param name="horseFromList">horse data</param>
        /// <param name="date">day of the race</param>
        /// <returns>returns WI</returns>
        public double ComputeWinIndex(LoadedHorse horseFromList,
            DateTime date, LoadedJockey jockeyFromList,
            RaceModel raceModelProvider,
            Dictionary<string, int> raceCategoryDictionary)
        {
            Dictionary<string, int> raceDictionary = raceCategoryDictionary;

            ResetComputeVariables();

            if (horseFromList.AllRaces.Count > 0)
            {

                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    ResetLoopVariables();

                    //get races only before race date, sort by race date from biggest to smallest
                    horseFromList.AllRaces = horseFromList.AllRaces.Where(l => l.RaceDate < date).OrderByDescending(l => l.RaceDate).ToList();

                    if (horseFromList.AllRaces.Count == 0)
                        return 0;

                    if (horseFromList.AllRaces[i].WonPlace < 3 && horseFromList.AllRaces[i].WonPlace > 0 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        if (horseFromList.AllRaces[i].WonPlace == 1)
                            PlaceFactor = 1;
                        if (horseFromList.AllRaces[i].WonPlace == 2)
                            PlaceFactor = 0.7;

                        if (jockeyFromList != null)
                        {
                            //bonus if was the same jockey as in current race
                            if (!string.IsNullOrEmpty(jockeyFromList.Name) && !string.IsNullOrEmpty(horseFromList.AllRaces[i].RacersName))
                            {
                                if (horseFromList.AllRaces[i].RacersName.Contains(jockeyFromList.Name))
                                    PlaceFactor = PlaceFactor * 1.5;
                            }
                        }

                        //bonus for place factor if won race in last 3 races
                        if (i < 3)
                        {
                            PlaceFactor = PlaceFactor * 1.5;
                        }

                        //increase factor for races over 12 horses and place between 1-4
                        if (horseFromList.AllRaces[i].RaceCompetition > 12 && horseFromList.AllRaces[i].WonPlace < 5)
                        {
                            PlaceFactor = PlaceFactor * 1.5;
                        }

                        bool foundKey = raceDictionary.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            DictValue = raceDictionary[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            DictValue = 5;
                        }

                        DistanceFactor = (double)(horseFromList.AllRaces[i].RaceDistance - int.Parse(raceModelProvider.Distance)) / 10000 * DictValue;
                        DistanceFactor = Math.Abs(DistanceFactor);

                        DistanceRaceIndex = PlaceFactor * horseFromList.AllRaces[i].RaceCompetition * DictValue / 10;

                        Result = Result + DistanceRaceIndex - DistanceFactor;
                    }
                }

                FinalResult = Result / horseFromList.AllRaces.Count;

                return FinalResult;
            }
            else
            {
                return 0;
            }
        }
    }
}