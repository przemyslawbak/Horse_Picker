using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Horse_Picker.Models;

namespace Horse_Picker.Services
{
    public class ComputeDataService : IComputeDataService
    {
        //compute fields
        private int _dictValue;
        private double _finalResult;
        private double _result;
        private double _siblingIndex;
        private int _counter;
        private LoadedHorse _childFromList = new LoadedHorse();
        //compute loop fields
        private double _placeFactor = 0;
        private double _distRaceIndex = 0;
        private double _distFactor = 0;

        private void ResetComputeVariables()
        {
            _dictValue = 1;
            _finalResult = 0;
            _result = 0;
            _siblingIndex = 0;
            _counter = 0;
            _childFromList = new LoadedHorse();
        }

        private void ResetLoopVariables()
        {
            _placeFactor = 0;
            _distRaceIndex = 0;
            _distFactor = 0;
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
        public double ComputeCategoryIndex(LoadedHorse horseFromList, DateTime date, IRaceModelProvider raceModelProvider, Dictionary<string, int> racecategoryDictionary)
        {
            Dictionary<string, int> raceDictionary = racecategoryDictionary;

            ResetComputeVariables();

            if (horseFromList.AllRaces.Count > 0)
            {
                for (int i = 0; i < horseFromList.AllRaces.Count; i++)
                {
                    ResetLoopVariables();

                    if (horseFromList.AllRaces[i].WonPlace > 0 && horseFromList.AllRaces[i].RaceDate < date)
                    {
                        _placeFactor = (double)horseFromList.AllRaces[i].WonPlace / horseFromList.AllRaces[i].RaceCompetition * 10;

                        //increase factor for races over 12 horses and place between 1-4
                        if (horseFromList.AllRaces[i].RaceCompetition > 12 && horseFromList.AllRaces[i].WonPlace < 5)
                        {
                            _placeFactor = _placeFactor * 1.5;
                        }

                        bool foundKey = raceDictionary.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            _dictValue = raceDictionary[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            _dictValue = 5;
                        }

                        _distFactor = (double)(horseFromList.AllRaces[i].RaceDistance - int.Parse(raceModelProvider.Distance)) / 10000 * _dictValue;
                        _distFactor = Math.Abs(_distFactor);

                        _distRaceIndex = _placeFactor * _dictValue / 10;

                        _result = _result + _distRaceIndex - _distFactor;
                    }
                }

                _finalResult = _result / horseFromList.AllRaces.Count;

                return _finalResult;
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
                            _placeFactor = 1;
                        if (jockeyFromList.AllRaces[i].WonPlace == 2)
                            _placeFactor = 0.7;

                        _distRaceIndex = _placeFactor * jockeyFromList.AllRaces[i].RaceCompetition / 10;

                        _result = _result + _distRaceIndex;
                    }
                }

                _finalResult = _result / jockeyFromList.AllRaces.Count;

                return _finalResult;
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
                        _result++;
                    }
                }

                double percentage = (double)_result / horseFromList.AllRaces.Count * 100;

                if (percentage > 20)
                {
                    _finalResult = 1 + (percentage - 20) * 0.1;
                }
                else
                {
                    _finalResult = 1;
                }

                return _finalResult;
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

                _result = (date - horseFromList.AllRaces[0].RaceDate).TotalDays;

                if (_result > 90)
                {
                    _finalResult = 1 + (_result - 90) / 100;
                }
                else if (_result < 60)
                {
                    _finalResult = 1 + _result / 50;
                }
                else
                {
                    _finalResult = 1;
                }

                return _finalResult;
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
            IRaceModelProvider raceModelProvider,
            ObservableCollection<LoadedHorse> horses,
            Dictionary<string, int> raceCategoryDictionary)
        {
            ResetComputeVariables();

            for (int i = 0; i < fatherFromList.AllChildren.Count; i++)
            {
                horses = new ObservableCollection<LoadedHorse>(horses.OrderBy(l => l.Age)); //from smallest to biggest
                HorseChildDetails child = fatherFromList.AllChildren[i];

                if (child.ChildAge == 0)
                {
                    _childFromList = horses
                                .Where(h => h.Name.ToLower() == child
                                .ChildName.ToLower())
                                .FirstOrDefault();
                }
                else
                {
                    _childFromList = horses
                                .Where(h => h.Name.ToLower() == child.ChildName.ToLower())
                                .Where(h => h.Age == child.ChildAge)
                                .FirstOrDefault();
                }

                if (_childFromList != null && _childFromList.AllRaces.Count > 0)
                {
                    _siblingIndex = ComputeWinIndex(_childFromList, date, null, raceModelProvider, raceCategoryDictionary);
                    _counter++;
                }
                else
                {
                    _siblingIndex = 0;
                }

                _result = _result + _siblingIndex;
            }

            if (_counter != 0)
            {
                _finalResult = _result / _counter;
            }
            else
            {
                _finalResult = 0;
            }

            return _finalResult;
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
                        _counter++;
                    }
                }

                if (_counter > 12)
                {
                    _finalResult = 1 + (_counter - 12) * 0.1;
                }
                else
                {
                    return 1;
                }

                return _finalResult;
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
            IRaceModelProvider raceModelProvider,
            Dictionary<string, int> racecategoryDictionary)
        {
            Dictionary<string, int> raceDictionary = racecategoryDictionary;

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
                            _placeFactor = 1;
                        if (horseFromList.AllRaces[i].WonPlace == 2)
                            _placeFactor = 0.7;

                        if (jockeyFromList != null)
                        {
                            //bonus if was the same jockey as in current race
                            if (!string.IsNullOrEmpty(jockeyFromList.Name) && !string.IsNullOrEmpty(horseFromList.AllRaces[i].RacersName))
                            {
                                if (horseFromList.AllRaces[i].RacersName.Contains(jockeyFromList.Name))
                                    _placeFactor = _placeFactor * 1.5;
                            }
                        }

                        //bonus for place factor if won race in last 3 races
                        if (i < 3)
                        {
                            _placeFactor = _placeFactor * 1.5;
                        }

                        //increase factor for races over 12 horses and place between 1-4
                        if (horseFromList.AllRaces[i].RaceCompetition > 12 && horseFromList.AllRaces[i].WonPlace < 5)
                        {
                            _placeFactor = _placeFactor * 1.5;
                        }

                        bool foundKey = raceDictionary.Keys.Any(k => k.Equals(horseFromList.AllRaces[i].RaceCategory,
                                      StringComparison.CurrentCultureIgnoreCase)
                        );

                        if (foundKey)
                        {
                            _dictValue = raceDictionary[horseFromList.AllRaces[i].RaceCategory];
                        }
                        else
                        {
                            _dictValue = 5;
                        }

                        _distFactor = (double)(horseFromList.AllRaces[i].RaceDistance - int.Parse(raceModelProvider.Distance)) / 10000 * _dictValue;
                        _distFactor = Math.Abs(_distFactor);

                        _distRaceIndex = _placeFactor * horseFromList.AllRaces[i].RaceCompetition * _dictValue / 10;

                        _result = _result + _distRaceIndex - _distFactor;
                    }
                }

                _finalResult = _result / horseFromList.AllRaces.Count;

                return _finalResult;
            }
            else
            {
                return 0;
            }
        }
    }
}
