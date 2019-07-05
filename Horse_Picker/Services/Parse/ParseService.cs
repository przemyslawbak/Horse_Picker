using Horse_Picker.Models;
using Horse_Picker.Services.Dictionary;
using Horse_Picker.Wrappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Services.Parse
{
    /// <summary>
    /// node parsing service
    /// </summary>
    public class ParseService : IParseService
    {
        private IDictionariesService _dictionaryService;

        DateTime _dateToday = DateTime.Now;

        public ParseService(IDictionariesService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        public string FormatName(string name, string jobType)
        {
            if (name.Contains(" "))
            {
                //format jockey name (X. Xxxx)
                if (jobType.Contains("JockeysPl") || jobType.Contains("HorsesPl"))
                {
                    char letter = name[0];
                    name = name.Split(' ')[1].Trim(' ');
                    name = letter + ". " + name;
                }
                else if (jobType.Contains("JockeysCz"))
                {
                    char letter = name.Split(' ')[1][0];
                    name = name.Split(' ')[0].Trim(' ');
                    name = letter + ". " + name;
                }
                else if (jobType.Contains("Historic"))
                {
                    return name;
                }
                else if (jobType.Contains("HorsesCz"))
                {
                    string toReplace = name.Split(' ')[1].Trim(' ');
                    name = toReplace + " " + name.Split(' ')[0].Trim(' ');
                }
            }

            return name;
        }

        public string FilterLetters(string name)
        {
            var filtered = name.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

            return new string(filtered.ToArray());
        }

        public HorseDataWrapper SplitHorseNodeString(string jobType, string nodeString, string propertyName, DateTime raceDate)
        {
            HorseDataWrapper horse = new HorseDataWrapper();

            string name = SplitName(jobType, nodeString, typeof(HorseDataWrapper), null);
            string age = SplitAge(jobType, nodeString, raceDate, propertyName);
            string racer = SplitName(jobType, nodeString, typeof(LoadedJockey), null);
            string link = "";

            horse = ParseHorseData(raceDate, name, age, link, racer, jobType);

            return horse;
        }
        public HorseChildDetails SplitChildNodeString(string jobType, string nodeString, string propertyName, DateTime raceDate)
        {
            HorseDataWrapper wrapper = new HorseDataWrapper();
            HorseChildDetails child = new HorseChildDetails();

            string name = SplitName(jobType, nodeString, typeof(LoadedHorse), propertyName);
            string age = SplitAge(jobType, nodeString, _dateToday, propertyName);
            string link = SplitLink(nodeString, jobType, propertyName);
            string jockey = "";

            wrapper = ParseHorseData(_dateToday, name, age, link, jockey, jobType);

            child.ChildAge = wrapper.Age;
            child.ChildLink = wrapper.Link;
            child.ChildName = wrapper.HorseName;

            return child;
        }

        public RaceDetails SplitRaceNodeString(string jobType, string stringNode, string propertyName)
        {
            RaceDetails race = new RaceDetails();

            string date = SplitDate(jobType, stringNode);
            string distance = SplitDistance(jobType, stringNode);
            string horseName = SplitName(jobType, stringNode, typeof(LoadedHorse), propertyName);
            string place = SplitPlace(jobType, stringNode);
            string competitors = SplitCompetitors(jobType, stringNode);
            string category = SplitCategory(jobType, stringNode);
            string racersLink = SplitRacersLink(jobType, stringNode);
            string racersName = SplitRacersName(jobType, stringNode);

            return race = ParseRaceData(date,
                distance,
                horseName,
                place,
                competitors,
                category,
                racersLink,
                racersName);
        }

        public string SplitLink(string nodeString, string jobType, string propertyName)
        {
            string link = "";

            if (jobType.Contains("HorsesPl"))
            {
                if (propertyName == "FatherLink")
                {
                    link = nodeString.Split('"')[1].Split('-')[0].Trim(' ');
                }
                else if (propertyName == "AllChildren")
                {
                    link = nodeString.Split('"')[3].Split('-')[0].Trim(' ');
                }

                link = "https://koniewyscigowe.pl" + link;
            }
            if (jobType.Contains("HorsesCz"))
            {
                if (propertyName == "FatherLink")
                {
                    link = "N/A";
                }
                else if (propertyName == "AllChildren")
                {
                    if (nodeString.Contains("kun.php?ID"))
                    {
                        link = nodeString.Split('?')[1].Split('\'')[0].Trim(' ');
                        link = "http://dostihyjc.cz/kun.php?" + link;
                    }
                }
            }

            return link;
        }

        public string SplitRacersName(string jobType, string stringNode)
        {
            string racersName = "";

            if (jobType.Contains("JockeysPl") || jobType.Contains("JockeysCz"))
            {
                racersName = "N/A";
            }
            else if (jobType.Contains("HistoricPl"))
            {
                racersName = "N/A";
            }
            else if (jobType.Contains("HorsesPl"))
            {
                racersName = SplitName(jobType, stringNode, typeof(LoadedJockey), null);
            }
            else if (jobType.Contains("HorsesCz"))
            {
                racersName = SplitName(jobType, stringNode, typeof(LoadedJockey), null);
            }

            return racersName;
        }

        public string SplitRacersLink(string jobType, string stringNode)
        {
            string racersLink = "";

            if (jobType.Contains("JockeysPl"))
            {
                racersLink = "N/A";
            }
            else if (jobType.Contains("JockeysCz"))
            {
                racersLink = "N/A";
            }
            else if (jobType.Contains("HistoricPl"))
            {
                racersLink = "N/A";
            }
            else if (jobType.Contains("HorsesPl"))
            {
                racersLink = stringNode.Split('"')[13].Split('-')[0].Trim(' ');
                racersLink = "https://koniewyscigowe.pl" + racersLink;
            }
            else if (jobType.Contains("HorsesCz"))
            {
                if (stringNode.Contains("jezdec.php?IDTR"))
                {
                    racersLink = stringNode.Split('?')[2].Split('\'')[0].Trim(' ');
                    racersLink = "http://dostihyjc.cz/jezdec.php?" + racersLink;
                }
                else
                {
                    racersLink = "";
                }
            }

            return racersLink;
        }

        public string SplitCategory(string jobType, string stringNode)
        {
            string category = "";

            if (jobType.Contains("JockeysPl"))
            {
                category = "N/A";
            }
            else if (jobType.Contains("JockeysCz"))
            {
                category = "N/A";
            }
            else if (jobType.Contains("HistoricPl"))
            {
                if (stringNode.Contains("sulki")) category = "sulki";
                else if (stringNode.Contains("gonitwa z płotami")) category = "płoty";
                else if (stringNode.Contains("z przeszkodami")) category = "steeple";
                else category = stringNode.Split(new string[] { "&nbsp;&#8226;&nbsp;" }, StringSplitOptions.None)[2].Split(new[] { "\t\t\n\t\t" }, StringSplitOptions.None)[0].Trim(' ');
            }
            else if (jobType.Contains("HorsesPl"))
            {
                category = stringNode.Split('>')[12].Split('<')[0].Trim(' ');
            }
            else if (jobType.Contains("HorsesCz"))
            {
                category = stringNode.Split('>')[15].Split('<')[0].Trim(' ');
            }

            return category;
        }

        public string SplitCompetitors(string jobType, string stringNode)
        {
            string competitors = "";

            if (jobType.Contains("JockeysPl"))
            {
                string raceScore = stringNode.Split('>')[12].Split('<')[0].Trim(' ');
                competitors = raceScore.Split('/')[1].Trim(' ');
            }
            else if (jobType.Contains("JockeysCz"))
            {
                string raceScore = stringNode.Split('>')[31].Split('<')[0].Trim(' ');
                competitors = competitors = raceScore.Split(' ')[2].Trim(' ');
            }
            else if (jobType.Contains("HistoricPl"))
            {
                competitors = "N/A";
            }
            else if (jobType.Contains("HorsesPl"))
            {
                string raceScore = stringNode.Split('>')[24].Split('<')[0].Trim(' ');
                competitors = raceScore.Split('/')[1].Trim(' ');
            }
            else if (jobType.Contains("HorsesCz"))
            {
                string raceScore = stringNode.Split('>')[31].Split('<')[0].Trim(' ');
                competitors = raceScore.Split('/')[1].Trim(' ');
            }

            return competitors;
        }

        public string SplitPlace(string jobType, string stringNode)
        {
            string place = "";

            if (jobType.Contains("JockeysPl"))
            {
                string raceScore = stringNode.Split('>')[12].Split('<')[0].Trim(' ');
                place = raceScore.Split('/')[0].Trim(' ');
            }
            else if (jobType.Contains("JockeysCz"))
            {
                string raceScore = stringNode.Split('>')[31].Split('<')[0].Trim(' ');
                place = raceScore.Split(' ')[0].Trim(' ');
            }
            else if (jobType.Contains("HistoricPl"))
            {
                place = "N/A";
            }
            else if (jobType.Contains("HorsesPl"))
            {
                string raceScore = stringNode.Split('>')[24].Split('<')[0].Trim(' ');
                place = raceScore.Split('/')[0].Trim(' ');
            }
            else if (jobType.Contains("HorsesCz"))
            {
                string raceScore = stringNode.Split('>')[31].Split('<')[0].Trim(' ');
                place = raceScore.Split('/')[0].Trim(' ');
            }

            return place;
        }

        public string SplitName(string jobType, string nodeString, Type type, string propertyName)
        {
            string name = "";
            if (jobType.Contains("JockeysPl"))
            {
                if (type == typeof(LoadedJockey))
                {
                    name = nodeString.Split(new string[] { " - " }, StringSplitOptions.None)[1].Split('<')[0].Trim(' ');
                }
                else if (type == typeof(LoadedHorse))
                {
                    name = nodeString.Split('>')[10].Split('<')[0].Trim(' ');
                }
            }
            else if (jobType.Contains("JockeysCz"))
            {
                if (type == typeof(LoadedJockey))
                {
                    name = nodeString.Split(new string[] { "<b>" }, StringSplitOptions.None)[3].Split('<')[0].Trim(' ');
                }
                else if (type == typeof(LoadedHorse))
                {
                    name = nodeString.Split('>')[23].Split('<')[0].Trim(' ');
                    if (name.Contains("("))
                    {
                        name = name.Split('(')[0].Trim(' ');
                    }
                }
            }
            else if (jobType.Contains("HistoricPl"))
            {
                if (type == typeof(HorseDataWrapper))
                {
                    name = nodeString.Split('>')[7].Split('<')[0].Trim(' ');
                }
                else if (type == typeof(LoadedJockey))
                {
                    name = nodeString.Split('>')[13].Split('<')[0].Trim(' ');
                    string toReplace = name.Split(' ')[0];
                    name = name.Replace(toReplace, "").Trim(' ');
                }
                else if (type == typeof(LoadedHorse))
                {
                    name = "N/A";
                }
            }
            else if (jobType.Contains("HorsesPl"))
            {
                if (type == typeof(LoadedHorse))
                {
                    if (propertyName == "AllRaces") //**leave blank**
                    {
                        name = "";
                    }
                    else if (propertyName == "AllChildren") //child name
                    {
                        name = nodeString.Split('>')[3].Split('<')[0].Trim(' ');
                    }
                    else //horse name
                    {
                        name = nodeString.Split(new string[] { "<title>" }, StringSplitOptions.None)[1].Split('(')[0].Trim(' ');
                        if (name.Contains(" - "))
                        {
                            name = name.Split(new string[] { " - " }, StringSplitOptions.None)[0].Trim(' ');
                        }
                    }
                }
                else if (type == typeof(LoadedJockey))
                {
                    name = nodeString.Split('>')[15].Split('<')[0].Trim(' ');
                    string toReplace = name.Split(' ')[0];
                    name = name.Replace(toReplace, "").Trim(' ');
                }
                else if (type == null) //father
                {
                    name = nodeString.Split('>')[1].Split('<')[0].Trim(' ');
                    if (name.Contains('('))
                    {
                        name = name.Split('(')[0].Trim(' ');
                    }
                }
            }
            else if (jobType.Contains("HorsesCz"))
            {
                if (type == typeof(LoadedHorse))
                {
                    if (propertyName == "AllRaces") //**leave blank**
                    {
                        name = "";
                    }
                    else if (propertyName == "AllChildren") //child name
                    {
                        if (!nodeString.Contains("potomek") && !nodeString.Contains("kun<"))
                        {
                            name = nodeString.Split('>')[3].Split(',')[0].Trim(' ');
                            if (name.Contains("("))
                            {
                                name = name.Split('(')[0].Trim(' ');
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else //horse name
                    {
                        name = nodeString.Split(new string[] { "<b>" }, StringSplitOptions.None)[3].Split('(')[0].Trim(' ');
                        if (name.Contains('<'))
                        {
                            name = name.Split('<')[0].Trim(' ');
                        }
                        if (name.Contains('.'))
                        {
                            name = name.Replace(".", "").Trim(' ');
                        }
                        if (name.Contains(" - "))
                        {
                            name = name.Split(new string[] { " - " }, StringSplitOptions.None)[0].Trim(' ');
                        }
                    }
                }
                else if (type == typeof(LoadedJockey))
                {
                    name = nodeString.Split('>')[23].Split('<')[0].Trim(' ');
                }
                else if (type == null) //father
                {
                    name = nodeString.Split('>')[9].Split('(')[0].Trim(' ').Replace("\n", "");
                    if (name.Contains('-'))
                    {
                        name = name.Split('-')[0].Trim(' ');
                    }
                }
            }
            if (type == typeof(LoadedJockey))
            {
                if (name.StartsWith("dż. ")) name = name.Replace("dż. ", "");
                if (name.StartsWith("u. ")) name = name.Replace("u. ", "");
                if (name.StartsWith("st. ")) name = name.Replace("st. ", "");
                if (name.StartsWith("ž. ")) name = name.Replace("ž. ", "");
                if (name.StartsWith("žk.")) name = name.Replace("žk.", "");
                if (name.StartsWith("am. ")) name = name.Replace("am. ", "");
                if (name.StartsWith("žk ")) name = name.Replace("žk ", "");
                if (name.StartsWith("ž ")) name = name.Replace("ž ", "");
                if (name.StartsWith("am ")) name = name.Replace("am ", "");
            }

            if (type != typeof(LoadedHorse) && type != typeof(HorseDataWrapper) && type != null)
            {
                name = FormatName(name, jobType);
            }

            name = MakeTitleCase(name);

            name = FilterLetters(name.Normalize(NormalizationForm.FormD));

            return name;
        }

        public string SplitAge(string jobType, string nodeString, DateTime raceDate, string propertyName)
        {
            string age = "N/A";

            if (jobType.Contains("JockeysPl"))
            {
                age = "N/A";
            }
            else if (jobType.Contains("JockeysCz"))
            {
                age = "N/A";
            }
            else if (jobType.Contains("HistoricPl"))
            {
                age = nodeString.Split('>')[10].Split('<')[0].Trim(' ');
            }
            else if (jobType.Contains("HorsesPl"))
            {
                if (propertyName == "Age")
                {
                    age = nodeString.Split(new string[] { "<strong>" }, StringSplitOptions.None)[1].Split('<')[0];
                    int count = age.Count(s => s == '.');
                    bool containsInt = age.Any(char.IsDigit);
                    if (age.Contains("lat"))
                    {
                        age = age.Split('(')[1].Split('l')[0].Trim(' ');
                    }
                    else if (containsInt)
                    {
                        if (count > 2)
                        {
                            age = age.Split('.')[3].Split('<')[0].Trim(' ');
                        }
                        else
                        {
                            age = age.Split('.')[1].Split('(')[0].Trim(' ');
                        }
                        if (age.Contains('<'))
                        {
                            age = age.Split('<')[0].Trim(' ');
                        }
                    }
                }
                else if (propertyName == "AllChildren")
                {
                    age = nodeString.Split('>')[8].Split('<')[0].Trim(' ');
                }
            }
            else if (jobType.Contains("HorsesCz"))
            {
                if (propertyName == "Age")
                {
                    age = nodeString.Split(new string[] { "<b>" }, StringSplitOptions.None)[4].Split('/')[1].Split('<')[0].Trim(' ');
                }
                else if (propertyName == "AllChildren")
                {
                    if (nodeString.Contains(","))
                    {
                        age = nodeString.Split(',')[1].Split('<')[0].Trim(' ');
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return age;
        }

        public string SplitDistance(string jobType, string stringNode)
        {
            string distance = "";

            if (jobType.Contains("JockeysPl"))
            {
                distance = stringNode.Split('>')[8].Split(' ')[0].Trim(' ');
            }
            else if (jobType.Contains("JockeysCz"))
            {
                distance = stringNode.Split('>')[19].Split('<')[0].Trim(' ');
            }
            else if (jobType.Contains("HistoricPl"))
            {
                distance = stringNode.Split(new string[] { "Dystans " }, StringSplitOptions.None)[1].Split('m')[0].Trim(' ');
            }
            else if (jobType.Contains("HorsesPl"))
            {
                distance = stringNode.Split('>')[10].Split('&')[0].Trim(' ');
            }
            else if (jobType.Contains("HorsesCz"))
            {
                distance = stringNode.Split('>')[19].Split('<')[0].Trim(' ');
            }

            return distance;
        }

        public string SplitDate(string jobType, string stringNode)
        {
            Dictionary<string, int> monthDict = _dictionaryService.GetMonthDictionary();
            string date = "";

            if (jobType.Contains("JockeysPl"))
            {
                date = stringNode.Split('>')[3].Split('<')[0].Trim(' ');
            }
            else if (jobType.Contains("JockeysCz"))
            {
                date = stringNode.Split('>')[3].Split('<')[0].Trim(' ');
            }
            else if (jobType.Contains("HistoricPl"))
            {
                date = stringNode.Split(new string[] { "Wynik gonitwy " }, StringSplitOptions.None)[1].Split(new string[] { "\n\t\t\n" }, StringSplitOptions.None)[0].Split('-').Last().Split('<')[0].Trim(' ');
                string day = date.Split(' ')[1];
                string month = date.Split(' ')[2];
                month = monthDict[month].ToString();
                string year = date.Split(' ')[3];
                StringBuilder sbDate = new StringBuilder();
                sbDate.Append(day);
                sbDate.Append(".");
                sbDate.Append(month);
                sbDate.Append(".");
                sbDate.Append(year);
                date = sbDate.ToString();
            }
            else if (jobType.Contains("HorsesPl"))
            {
                date = stringNode.Split('>')[4].Split('<')[0].Trim(' ');
            }
            else if (jobType.Contains("HorsesCz"))
            {
                date = stringNode.Split('>')[3].Split('<')[0].Trim(' ');
            }

            return date;
        }

        public string MakeTitleCase(string name)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
            {
                TextInfo myCI = new CultureInfo("en-US", false).TextInfo; //creates CI
                name = name.ToLower().Trim(' '); //takes to lower, to take to TC later
                name = myCI.ToTitleCase(name); //takes to TC
            }
            else
            {
                return "";
            }

            return name;
        }

        public RaceDetails ParseRaceData(string raceDate,
            string raceDistance,
            string horsesName,
            string racePlace,
            string raceCompetitors,
            string raceCategory,
            string racersLink,
            string racersName)
        {
            RaceDetails race = new RaceDetails();

            int n;
            bool parseTest;
            DateTime t;

            parseTest = int.TryParse(raceDistance, out n);
            if (parseTest)
            {
                race.RaceDistance = int.Parse(raceDistance); //race distance
            }
            else
            {
                race.RaceDistance = 2000; //race distance
            }

            parseTest = int.TryParse(raceCompetitors, out n);
            if (parseTest)
            {
                race.RaceCompetition = int.Parse(raceCompetitors); //race qty of horses
            }
            else
            {
                race.RaceCompetition = 10; //race qty of horses
            }

            if (racePlace != "0" && racePlace != "D" && racePlace != "PN" && racePlace != "Z" && racePlace != "ZN" && racePlace != "SN")
            {
                parseTest = int.TryParse(racePlace, out n);
                if (parseTest)
                {
                    race.WonPlace = int.Parse(racePlace); //won place
                }
                else
                {
                    if (racePlace == "N/A")
                    {
                        race.WonPlace = 0;
                    }
                    else
                    {
                        race.WonPlace = race.RaceCompetition; //won place
                    }
                }
            }
            else
            {
                race.WonPlace = race.RaceCompetition;
            }

            parseTest = DateTime.TryParse(raceDate, out t);
            if (parseTest)
            {
                race.RaceDate = DateTime.Parse(raceDate); //day of race
            }
            else
            {
                race.RaceDate = _dateToday; //day of race
            }

            if (!string.IsNullOrEmpty(horsesName))
            {
                race.HorseName = horsesName; //rided horse
            }
            else
            {
                race.HorseName = "-"; //rided horse
            }

            if (raceCategory.Contains("Grupa"))
            {
                raceCategory = raceCategory.Replace("Grupa ", "");
                race.RaceCategory = raceCategory; // category name
            }
            else
            {
                race.RaceCategory = raceCategory; // category name
            }

            if (!string.IsNullOrEmpty(racersLink))
            {
                race.RacersLink = racersLink; //link tho the racer
            }
            else
            {
                race.RaceLink = "-";
            }

            if (!string.IsNullOrEmpty(racersName))
            {
                race.RacersName = racersName; //name of the racer
            }
            else
            {
                race.RaceLink = "-";
            }



            return race;
        }

        public HorseDataWrapper ParseHorseData(DateTime raceDate, string name, string age, string link, string jockey, string jobType)
        {
            HorseDataWrapper horse = new HorseDataWrapper();
            int n;

            if (!string.IsNullOrEmpty(name))
            {
                horse.HorseName = name; //rided horse
            }
            else
            {
                horse.HorseName = "-"; //rided horse
            }

            if (!string.IsNullOrEmpty(link))
            {
                horse.Link = link; //rided horse
            }
            else
            {
                horse.Link = "-"; //rided horse
            }

            bool parseTest = int.TryParse(age, out n);
            if (parseTest && jobType.Contains("HistoricPl"))
            {
                horse.Age = int.Parse(age) + (_dateToday.Year - raceDate.Year);
            }
            else if (parseTest)
            {
                horse.Age = int.Parse(age); //age
                if (horse.Age > 199)
                {
                    horse.Age = _dateToday.Year - horse.Age; //age
                }
            }
            else
            {
                horse.Age = 99;
            }

            if (!string.IsNullOrEmpty(jockey))
            {
                horse.Jockey = jockey; //name of the racer
            }
            else
            {
                horse.Jockey = "-";
            }

            return horse;
        }

        public LoadedJockey ParseJockeyData(string link, string name)
        {
            LoadedJockey jockey = new LoadedJockey();

            if (!string.IsNullOrEmpty(link))
            {
                jockey.Link = link; //name of the racer
            }
            else
            {
                jockey.Link = "-";
            }

            if (!string.IsNullOrEmpty(name))
            {
                jockey.Name = name; //name of the racer
            }
            else
            {
                jockey.Name = "-";
            }

            return jockey;
        }

        //TODO: dictionary
        public bool VerifyNodeCondition(string node, string propertyName, string jobType)
        {
            List<string> positivePhrases = new List<string>();
            List<string> negativePhrases = new List<string>();
            bool verify = false;

            if (jobType.Contains("JockeysPl"))
            {
                if (propertyName != "AllRaces" && propertyName != "AllChildren")
                {
                    positivePhrases.Add("Jeździec"); //for JockeysPl profile
                }
                if (propertyName == "AllRaces")
                {
                    positivePhrases.Add("&nbsp;zł"); //for HorsesPl race row
                }
            }
            if (jobType.Contains("JockeysCz"))
            {
                positivePhrases.Add("Licence jezdce"); //for JockeysCz profile
                positivePhrases.Add("vysledky.php?id_dostih="); //for JockeysCz race row
            }
            if (jobType.Contains("HorsesPl"))
            {
                if (propertyName != "AllRaces" && propertyName != "AllChildren")
                {
                    positivePhrases.Add("Rasa:"); //for HorsesPl profile
                }
                if (propertyName == "Father" || propertyName == "FatherLink")
                {
                    positivePhrases.Add("<a href="); //for HorsesPl father
                }
                if (propertyName == "AllRaces")
                {
                    positivePhrases.Add("&nbsp;m"); //for HorsesPl race row
                }
                if (propertyName == "AllChildren")
                {
                    positivePhrases.Add("ogier"); //for HorsesPl child row
                    positivePhrases.Add("klacz"); //for HorsesPl child row
                    positivePhrases.Add("wałach"); //for HorsesPl child row
                }
            }
            if (jobType.Contains("HorsesCz"))
            {
                if (propertyName != "AllRaces" && propertyName != "AllChildren")
                {
                    positivePhrases.Add("barva, pohlavi:"); //for HorsesCz profile
                }
                if (propertyName == "AllRaces")
                {
                    positivePhrases.Add("Kč"); //for HorsesCz race row
                    negativePhrases.Add("nebyl nalezen žádný záznam"); //for HorsesCz race row
                    negativePhrases.Add("Nebyli nalezeni žádní potomci"); //for HorsesCz race row
                }
                if (propertyName == "AllChildren")
                {
                    positivePhrases.Add("td align='left' width='180' class='trs'"); //for HorsesCz child row
                    negativePhrases.Add("Nebyli nalezeni žádní potomci"); //for HorsesCz child row
                    negativePhrases.Add("nebyl nalezen žádný záznam"); //for HorsesCz child row
                }
            }

            if (jobType.Contains("HistoricPl"))
            {
                positivePhrases.Add("Pula nagród"); //for HistoricPl (not sent prop name)
                positivePhrases.Add("zł"); //for HistoricPl (not sent prop name)
                if (propertyName == "HorseList")
                {
                    negativePhrases.Add(">rekord</"); //because of some reason can not get race node list for 'sulki'
                }
            }

            if (positivePhrases.Any(node.Contains))
            {
                verify = true;
            }
            if (negativePhrases.Any(node.Contains))
            {
                verify = false;
            }

            return verify;
        }
    }
}
