using Horse_Picker.Models;
using Horse_Picker.NewModels;
using Horse_Picker.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Wrappers
{
    public class HorseDataWrapper : ViewModelBase
    {
        private LoadedHorse _horse;
        private LoadedJockey _jockey;
        private int _totalRaces;
        private double _winIndex;
        private double _siblingsIndex;
        private double _jockeyIndex;
        private double _categoryIndex;
        private string _comments;

        public HorseDataWrapper()
        {
            _horse = new LoadedHorse();
            _jockey = new LoadedJockey();
        }

        public double HorseScore
        {
            get { return WinIndex; }
        }

        public string HorseName
        {
            get
            {
                _horse.Name = MakeTitleCase(_horse.Name);
                return _horse.Name;
            }
            set
            {
                _horse.Name = MakeTitleCase(value);
                OnPropertyChanged();
            }
        }

        public int Age
        {
            get { return _horse.Age; }
            set
            {
                _horse.Age = value;
                OnPropertyChanged();
            }
        }

        public string Father
        {
            get
            {
                _horse.Father = MakeTitleCase(_horse.Father);
                return _horse.Father;
            }
            set
            {
                _horse.Father = MakeTitleCase(value);
                OnPropertyChanged();
            }
        }

        public string Jockey
        {
            get
            {
                _jockey.Name = TrimTheName(_jockey.Name);
                _jockey.Name = MakeTitleCase(_jockey.Name);
                return _jockey.Name;
            }
            set
            {
                _jockey.Name = TrimTheName(_jockey.Name);
                _jockey.Name = MakeTitleCase(value);
                OnPropertyChanged();
            }
        }

        public int TotalRaces
        {
            get { return _totalRaces; }
            set
            {
                _totalRaces = value;
                OnPropertyChanged();
            }
        }

        public double WinIndex
        {
            get { return _winIndex; }
            set
            {
                _winIndex = value;
                OnPropertyChanged();
            }
        }

        public double SiblingsIndex
        {
            get { return _siblingsIndex; }
            set
            {
                _siblingsIndex = value;
                OnPropertyChanged();
            }
        }

        public double JockeyIndex
        {
            get { return _jockeyIndex; }
            set
            {
                _jockeyIndex = value;
                OnPropertyChanged();
            }
        }

        public double CategoryIndex
        {
            get { return _categoryIndex; }
            set
            {
                _categoryIndex = value;
                OnPropertyChanged();
            }
        }

        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }
        private string MakeTitleCase(string name)
        {
            if (!string.IsNullOrEmpty(name) && name != "--Not found--")
            {
                TextInfo myCI = new CultureInfo("en-US", false).TextInfo; //creates CI
                name = name.ToLower().Trim(' '); //takes to lower, to take to TC later
                name = myCI.ToTitleCase(name); //takes to TC
            }

            return name;
        }
        private string TrimTheName(string name)
        {
            if (!string.IsNullOrEmpty(name) && name != "--Not found--")
            {
                name = name.Trim(' ');
                if (name.Contains(" "))
                {
                    char letter = name[0];
                    name = name.Split(' ')[1].Trim(' ');
                    name = letter + ". " + name;
                }
            }

            return name;
        }
    }
}
