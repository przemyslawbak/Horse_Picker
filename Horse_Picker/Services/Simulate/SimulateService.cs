using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Horse_Picker.Models;
using System.Threading;
using Horse_Picker.Wrappers;
using Horse_Picker.Events;
using Horse_Picker.Services.Update;
using Horse_Picker.Services.Files;

namespace Horse_Picker.Services.Simulate
{
    public class SimulateService : ISimulateService
    {
        private IFileService _dataServices;
        private IUpdateService _updateDataService;
        int _idTo;
        int _idFrom;
        int _loopCounter;
        string _jobType;
        int _degreeOfParallelism;

        public SimulateService(IUpdateService updateDataService, IFileService dataServices)
        {
            _degreeOfParallelism = 100;

            _updateDataService = updateDataService;
            _dataServices = dataServices;
        }

        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public event EventHandler<UpdateBarEventArgs> SimulateProgressEventHandler;

        /// <summary>
        /// simulates score index for all horses in all races in certain year from race database
        /// CPU operations
        /// creates loads of parallel tasks to improve performance speed
        /// use of SemaphoreSlim to control tasks
        /// mode details for credits in UpdateService
        /// </summary>
        /// <param name="fromId">object id limitation</param>
        /// <param name="toId">object id limitation</param>
        /// <param name="races">collection of races</param>
        /// <param name="horses">collection of horses</param>
        /// <param name="jockeys">collection of jockey</param>
        /// <param name="raceModelProvider">race data</param>
        /// <returns></returns>
        public async Task<ObservableCollection<RaceDetails>> SimulateResultsAsync(int fromId,
            int toId,
            ObservableCollection<RaceDetails> races,
            ObservableCollection<LoadedHorse> horses,
            ObservableCollection<LoadedJockey> jockeys,
            IRaceProvider raceModelProvider)
        {
            //variables
            SemaphoreSlim throttler = new SemaphoreSlim(_degreeOfParallelism);
            List<Task> tasks = new List<Task>();
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;
            _loopCounter = 0;
            _idFrom = fromId;
            _idTo = toId;
            _jobType = "Simulating historic races";
            OnProgressBarTick();

            //run loop
            for (int i = 0; i < races.Count; i++)
            {
                int id = i;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        if (CancellationToken.IsCancellationRequested)
                            return;

                        await throttler.WaitAsync(TokenSource.Token);

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
                    catch (Exception e)
                    {
                        //
                    }
                    finally
                    {
                        _loopCounter++;

                        EventHandler<UpdateBarEventArgs> progressBarTick = SimulateProgressEventHandler;

                        OnProgressBarTick();

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

        /// <summary>
        /// updating progress bar event in main window
        /// </summary>
        protected void OnProgressBarTick()
        {
            SimulateProgressEventHandler(this, new UpdateBarEventArgs(_jobType, _loopCounter, _idTo, _idFrom));
        }

        /// <summary>
        /// cancellation token for simulations
        /// </summary>
        public void CancelUpdates()
        {
            TokenSource.Cancel();
        }
    }
}
