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
        private UpdateModules _updateModulesModel;
        public UpdateViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _updateModulesModel = new UpdateModules();

            RunTheUpdate();
        }

        /// <summary>
        /// initializing properties and publishes update props to the event
        /// </summary>
        private void RunTheUpdate()
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

            _eventAggregator.GetEvent<DataUpdateEvent>().Publish(_updateModulesModel);
        }

        //prop for scrap PL jockeys from ID int
        public int JPlFrom
        {
            get
            {
                return _updateModulesModel.JPlFrom;
            }
            set
            {
                _updateModulesModel.JPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL jockeys to ID int
        public int JPlTo
        {
            get
            {
                return _updateModulesModel.JPlTo;
            }
            set
            {
                _updateModulesModel.JPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ jockeys from ID int
        public int JCzFrom
        {
            get
            {
                return _updateModulesModel.JCzFrom;
            }
            set
            {
                _updateModulesModel.JCzFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ jockeys to ID int
        public int JCzTo
        {
            get
            {
                return _updateModulesModel.JCzTo;
            }
            set
            {
                _updateModulesModel.JCzTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL horses from ID int
        public int HPlFrom
        {
            get
            {
                return _updateModulesModel.HPlFrom;
            }
            set
            {
                _updateModulesModel.HPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL horses to ID int
        public int HPlTo
        {
            get
            {
                return _updateModulesModel.HPlTo;
            }
            set
            {
                _updateModulesModel.HPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ horses from ID int
        public int HCzFrom
        {
            get
            {
                return _updateModulesModel.HCzFrom;
            }
            set
            {
                _updateModulesModel.HCzFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap CZ horses to ID int
        public int HCzTo
        {
            get
            {
                return _updateModulesModel.HCzTo;
            }
            set
            {
                _updateModulesModel.HCzTo = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL historic races from ID int
        public int HistPlFrom
        {
            get
            {
                return _updateModulesModel.HistPlFrom;
            }
            set
            {
                _updateModulesModel.HistPlFrom = value;
                OnPropertyChanged();
            }
        }

        //prop for scrap PL historic races to ID int
        public int HistPlTo
        {
            get
            {
                return _updateModulesModel.HistPlTo;
            }
            set
            {
                _updateModulesModel.HistPlTo = value;
                OnPropertyChanged();
            }
        }

        //prop for update jockeys PL checkbox
        public bool UpdateJockeysPl
        {
            get
            {
                return _updateModulesModel.JockeysPl;
            }
            set
            {
                _updateModulesModel.JockeysPl = value;
                OnPropertyChanged();
            }
        }

        //prop for update jockeys CZ checkbox
        public bool UpdateJockeysCz
        {
            get
            {
                return _updateModulesModel.JockeysCz;
            }
            set
            {
                _updateModulesModel.JockeysCz = value;
                OnPropertyChanged();
            }
        }

        //prop for update horses CZ checkbox
        public bool UpdateHorsesCz
        {
            get
            {
                return _updateModulesModel.HorsesCz;
            }
            set
            {
                _updateModulesModel.HorsesCz = value;
                OnPropertyChanged();
            }
        }

        //prop for update horses PL checkbox
        public bool UpdateHorsesPl
        {
            get
            {
                return _updateModulesModel.HorsesPl;
            }
            set
            {
                _updateModulesModel.HorsesPl = value;
                OnPropertyChanged();
            }
        }

        //prop for update historic data PL checkbox
        public bool UpdateRacesPl
        {
            get
            {
                return _updateModulesModel.RacesPl;
            }
            set
            {
                _updateModulesModel.RacesPl = value;
                OnPropertyChanged();
            }
        }
    }
}
