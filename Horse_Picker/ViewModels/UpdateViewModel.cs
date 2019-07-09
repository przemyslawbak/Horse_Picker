using Horse_Picker.Events;
using Horse_Picker.Models;
using Horse_Picker.Views;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Horse_Picker.ViewModels
{
    public class UpdateViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;

        public UpdateViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            UpdateModulesModel = new UpdateModules();

            RunTheUpdate();
        }

        /// <summary>
        /// initializing properties and publishes update props to the event
        /// </summary>
        public void RunTheUpdate()
        {
            UpdateHorsesCz = false;
            UpdateHorsesPl = false;
            UpdateJockeysCz = false;
            UpdateJockeysPl = false;
            UpdateRacesPl = false;

            //default values
            JPlFrom = 1;
            JPlTo = 1049;
            JCzFrom = 4000;
            JCzTo = 31049;
            HPlFrom = 1;
            HPlTo = 25049;
            HCzFrom = 8000;
            HCzTo = 150049;
            HistPlFrom = 1;
            HistPlTo = 17049;

            _eventAggregator.GetEvent<DataUpdateEvent>().Publish(UpdateModulesModel);
        }

        public UpdateModules UpdateModulesModel { get; set; }

        //prop for scrap PL jockeys from ID int
        public int JPlFrom
        {
            get
            {
                return UpdateModulesModel.JPlFrom;
            }
            set
            {
                UpdateModulesModel.JPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL jockeys to ID int
        public int JPlTo
        {
            get
            {
                return UpdateModulesModel.JPlTo;
            }
            set
            {
                UpdateModulesModel.JPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ jockeys from ID int
        public int JCzFrom
        {
            get
            {
                return UpdateModulesModel.JCzFrom;
            }
            set
            {
                UpdateModulesModel.JCzFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ jockeys to ID int
        public int JCzTo
        {
            get
            {
                return UpdateModulesModel.JCzTo;
            }
            set
            {
                UpdateModulesModel.JCzTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL horses from ID int
        public int HPlFrom
        {
            get
            {
                return UpdateModulesModel.HPlFrom;
            }
            set
            {
                UpdateModulesModel.HPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL horses to ID int
        public int HPlTo
        {
            get
            {
                return UpdateModulesModel.HPlTo;
            }
            set
            {
                UpdateModulesModel.HPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ horses from ID int
        public int HCzFrom
        {
            get
            {
                return UpdateModulesModel.HCzFrom;
            }
            set
            {
                UpdateModulesModel.HCzFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ horses to ID int
        public int HCzTo
        {
            get
            {
                return UpdateModulesModel.HCzTo;
            }
            set
            {
                UpdateModulesModel.HCzTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL historic races from ID int
        public int HistPlFrom
        {
            get
            {
                return UpdateModulesModel.HistPlFrom;
            }
            set
            {
                UpdateModulesModel.HistPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL historic races to ID int
        public int HistPlTo
        {
            get
            {
                return UpdateModulesModel.HistPlTo;
            }
            set
            {
                UpdateModulesModel.HistPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for update jockeys PL checkbox
        public bool UpdateJockeysPl
        {
            get
            {
                return UpdateModulesModel.JockeysPl;
            }
            set
            {
                UpdateModulesModel.JockeysPl = value;
                OnPropertyChanged();
            }
        }

        //prop for update jockeys CZ checkbox
        public bool UpdateJockeysCz
        {
            get
            {
                return UpdateModulesModel.JockeysCz;
            }
            set
            {
                UpdateModulesModel.JockeysCz = value;
                OnPropertyChanged();
            }
        }

        //prop for update horses CZ checkbox
        public bool UpdateHorsesCz
        {
            get
            {
                return UpdateModulesModel.HorsesCz;
            }
            set
            {
                UpdateModulesModel.HorsesCz = value;
                OnPropertyChanged();
            }
        }

        //prop for update horses PL checkbox
        public bool UpdateHorsesPl
        {
            get
            {
                return UpdateModulesModel.HorsesPl;
            }
            set
            {
                UpdateModulesModel.HorsesPl = value;
                OnPropertyChanged();
            }
        }

        //prop for update historic data PL checkbox
        public bool UpdateRacesPl
        {
            get
            {
                return UpdateModulesModel.RacesPl;
            }
            set
            {
                UpdateModulesModel.RacesPl = value;
                OnPropertyChanged();
            }
        }
    }
}
