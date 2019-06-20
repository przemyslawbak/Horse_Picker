using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Horse_Picker.Models;
using System.Threading;
using Horse_Picker.Wrappers;

namespace Horse_Picker.Services
{
    public class SimulateDataService : ISimulateDataService
    {
        private IFileDataService _dataServices;
        private IUpdateDataService _updateDataService;
        int _degreeOfParallelism;

        public SimulateDataService(IUpdateDataService updateDataService, IFileDataService dataServices)
        {
            _degreeOfParallelism = 100;

            _updateDataService = updateDataService;
            _dataServices = dataServices;
        }

        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public async Task<ObservableCollection<LoadedHistoricalRace>> SimulateResultsAsync(int fromId,
            int toId,
            ObservableCollection<LoadedHistoricalRace> races,
            ObservableCollection<LoadedHorse> horses,
            ObservableCollection<LoadedJockey> jockeys,
            IRaceModelProvider raceModelProvider)
        {
            //variables
            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            int loopCounter = 0;
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;

            //run loop
            for (int i = 0; i < races.Count; i++)
            {
                int id = i;

                await throttler.WaitAsync(TokenSource.Token);

                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        CancellationToken.ThrowIfCancellationRequested();

                        for (id = 0; id < races.Count; id++)
                        {
                            if (races[id].RaceDate.Year == 2018)
                            {
                                raceModelProvider.Category = races[id].RaceCategory;
                                raceModelProvider.Distance = races[id].RaceDistance.ToString();

                                //for all horses in the race
                                for (int h = 0; h < races[id].HorseList.Count; h++)
                                {
                                    HorseDataWrapper horse = new HorseDataWrapper();
                                    horse = _updateDataService.GetParsedHorseData(races[id].HorseList[h], races[id].RaceDate, horses, jockeys, raceModelProvider);
                                    races[id].HorseList[h] = horse; //get all indexes
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //
                    }
                    finally
                    {
                        loopCounter++;

                        //ProgressBarTick(jobDescription, loopCounter, idTo, idFrom); //????????????????????

                        throttler.Release();
                    }
                }));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                //
            }
            finally
            {
                await _dataServices.SaveRaceSimulatedResultsAsync(races.ToList());

                throttler.Dispose();
            }

            return races;
        }

        public void CancelUpdates()
        {
            TokenSource.Cancel();
        }
    }
}
