using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Horse_Picker.Models;
using Horse_Picker.Services.Dictionary;
using Horse_Picker.Wrappers;
using HtmlAgilityPack;

namespace Horse_Picker.Services.Scrap
{
    public class ScrapService : IScrapService
    {
        int _yearMin = 2013;
        int _yearMax = DateTime.Now.Year;
        DateTime _dateToday = DateTime.Now;

        private IDictionariesService _dictionaryService;

        public ScrapService(IDictionariesService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        public async Task<T> ScrapGenericObject<T>(int id, string jobType)
        {
            LoadedJockey jockey = new LoadedJockey();
            LoadedHorse horse = new LoadedHorse();
            RaceDetails race = new RaceDetails();

            Dictionary<string, string> nodeDictionary = new Dictionary<string, string>();
            List<HtmlDocument> raceHtmlAgilityList = new List<HtmlDocument>();

            string linkBase = "";

            if (jobType.Contains("JockeysPl"))
            {
                linkBase = "https://koniewyscigowe.pl/dzokej?d=";
                nodeDictionary = _dictionaryService.GetJockeyPlNodeDictionary();
            }
            else if (jobType.Contains("JockeysCz"))
            {
                linkBase = "http://dostihyjc.cz/jezdec.php?IDTR=";
                nodeDictionary = _dictionaryService.GetJockeyCzNodeDictionary();
            }
            else if (jobType.Contains("HistoricPl"))
            {
                linkBase = "https://koniewyscigowe.pl/wyscig?w=";
                nodeDictionary = _dictionaryService.GetRacePlNodeDictionary();
            }
            else if (jobType.Contains("HorsesPl"))
            {
                linkBase = "https://koniewyscigowe.pl/horse/";
                nodeDictionary = _dictionaryService.GetHorsePlNodeDictionary();
            }
            else if (jobType.Contains("HorsesCz"))
            {
                linkBase = "http://dostihyjc.cz/kun.php?ID=";
                nodeDictionary = _dictionaryService.GetHorseCzNodeDictionary();
            }

            string link = GetLink(linkBase, id);

            string html = await GetHtmlDocumentAsync(link);

            HtmlDocument htmlAgility = GetHtmlAgility(html);

            string nodeString = htmlAgility.DocumentNode.OuterHtml.ToString();

            bool nodeConditions = VerifyNodeCondition(nodeString, null);

            if (typeof(T) == typeof(LoadedJockey))
            {
                if (nodeConditions)
                {
                    string name = SplitName(jobType, nodeString, typeof(T), null);

                    raceHtmlAgilityList = await GetRaceHtmlAgilityAsync(jobType, link);

                    jockey = ParseJockeyData(link, name);
                    jockey.AllRaces = GetGenericList<RaceDetails>(raceHtmlAgilityList, nameof(jockey.AllRaces), nodeDictionary, jobType, _dateToday);
                }
                else
                {
                    jockey = null;
                }

                return (T)Convert.ChangeType(jockey, typeof(T));
            }
            else if (typeof(T) == typeof(RaceDetails))
            {
                if (nodeConditions)
                {
                    string horsesNode = nodeDictionary[nameof(race.HorseList)];

                    raceHtmlAgilityList.Add(htmlAgility);

                    HtmlNode[] raceHorseTable = htmlAgility.DocumentNode.SelectNodes(horsesNode).ToArray();

                    race = SplitRaceNodeString(jobType, nodeString, null);
                    race.RaceLink = link;
                    race.HorseList = GetGenericList<HorseDataWrapper>(raceHtmlAgilityList, nameof(race.HorseList), nodeDictionary, jobType, race.RaceDate);
                    race.RaceCompetition = race.HorseList.Count;
                }
                else
                {
                    race = null;
                }

                return (T)Convert.ChangeType(race, typeof(T));
            }
            else if (typeof(T) == typeof(LoadedHorse))
            {
                if (nodeConditions)
                {
                    HorseDataWrapper wrapper = new HorseDataWrapper();

                    raceHtmlAgilityList.Add(htmlAgility);

                    string name = SplitName(jobType, nodeString, typeof(T), null);
                    string age = SplitAge(jobType, nodeString, _dateToday, nameof(horse.Age));
                    string racer = "";

                    wrapper = ParseHorseData(_dateToday, name, age, link, racer, jobType);

                    horse.Link = link;
                    horse.Name = wrapper.HorseName;
                    horse.Age = wrapper.Age;
                    horse.Father = GetFather(jobType, htmlAgility, nameof(horse.Father), nodeDictionary);
                    horse.FatherLink = GetFather(jobType, htmlAgility, nameof(horse.FatherLink), nodeDictionary);
                    horse.AllRaces = GetGenericList<RaceDetails>(raceHtmlAgilityList, nameof(horse.AllRaces), nodeDictionary, jobType, _dateToday);
                    horse.AllChildren = GetGenericList<HorseChildDetails>(raceHtmlAgilityList, nameof(horse.AllChildren), nodeDictionary, jobType, _dateToday);
                }
                else
                {
                    horse = null;
                }

                return (T)Convert.ChangeType(horse, typeof(T));
            }
            else { throw new ArgumentException(); }
        }

        private List<T> GetGenericList<T>(List<HtmlDocument> raceHtmlAgilityList, string propertyName, Dictionary<string, string> nodeDictionary, string jobType, DateTime raceDate)
        {
            List<HorseChildDetails> children = new List<HorseChildDetails>();

            List<RaceDetails> races = new List<RaceDetails>();

            List<HorseDataWrapper> horses = new List<HorseDataWrapper>();

            string node = nodeDictionary[propertyName];

            foreach (var raceHtmlAgility in raceHtmlAgilityList)
            {
                HtmlNode[] tableRows = raceHtmlAgility.DocumentNode.SelectNodes(node).ToArray();

                if (tableRows.Length > 0)
                {
                    foreach (var row in tableRows)
                    {
                        string nodeString = row.OuterHtml.ToString();

                        bool nodeConditions = VerifyNodeCondition(nodeString, propertyName);

                        if (nodeConditions)
                        {
                            if (typeof(T) == typeof(HorseChildDetails))
                            {
                                HorseChildDetails child = new HorseChildDetails();

                                child = SplitChildNodeString(jobType, nodeString, propertyName, raceDate);

                                children.Add(child);
                            }
                            else if (typeof(T) == typeof(RaceDetails))
                            {
                                RaceDetails race = new RaceDetails();

                                race = SplitRaceNodeString(jobType, nodeString, propertyName);

                                races.Add(race);
                            }
                            else if (typeof(T) == typeof(HorseDataWrapper))
                            {

                                HorseDataWrapper horse = new HorseDataWrapper();

                                horse = SplitHorseNodeString(jobType, nodeString, propertyName, raceDate);

                                horses.Add(horse);
                            }
                        }
                    }
                }
            }

            if (typeof(T) == typeof(HorseChildDetails))
            {
                return (List<T>)Convert.ChangeType(children, typeof(List<T>));
            }
            else if (typeof(T) == typeof(RaceDetails))
            {
                return (List<T>)Convert.ChangeType(races, typeof(List<T>));
            }
            else if (typeof(T) == typeof(HorseDataWrapper))
            {

                return (List<T>)Convert.ChangeType(horses, typeof(List<T>));
            }
            else { throw new ArgumentException(); }
        }

        public string GetLink(string linkBase, int id)
        {
            return linkBase + id;
        }

        public async Task<List<HtmlDocument>> GetRaceHtmlAgilityAsync(string jobType, string link)
        {
            List<HtmlDocument> raceHtmlAgilityList = new List<HtmlDocument>();
            StringBuilder sb;
            int year = _yearMax;
            List<string> links = new List<string>();

            for (year = _yearMax; year > _yearMin; year--)
            {
                sb = new StringBuilder();

                if (jobType.Contains("JockeysPl"))
                {
                    sb.Append(link);
                    sb.Append("&sezon=");
                    sb.Append(year.ToString());
                    sb.Append("#wyniki_koni");
                }
                else if (jobType.Contains("JockeysCz"))
                {
                    sb.Append(link);
                    sb.Append("&rok=");
                    sb.Append(year.ToString());
                    sb.Append("&pod=kariera");
                }

                links.Add(sb.ToString());
            }

            foreach (string item in links)
            {
                string html = await GetHtmlDocumentAsync(item);

                HtmlDocument newDoc = GetHtmlAgility(html);

                raceHtmlAgilityList.Add(newDoc);
            }

            return raceHtmlAgilityList;
        }

        private static async Task<string> GetHtmlDocumentAsync(string link)
        {
            string result = "";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(link);

                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }

        private string GetFather(string jobType, HtmlDocument htmlAgility, string propertyName, Dictionary<string, string> nodeDictionary)
        {
            string father = "";

            string fatherNode = nodeDictionary[propertyName];

            HtmlNode node = htmlAgility.DocumentNode.SelectSingleNode(fatherNode);

            string nodeString = node.OuterHtml.ToString();

            bool nodeConditions = VerifyNodeCondition(nodeString, propertyName);

            if (nodeConditions) //check for anchor
            {
                fatherNode = fatherNode + "/a";
                node = htmlAgility.DocumentNode.SelectSingleNode(fatherNode);
                nodeString = node.OuterHtml.ToString();
            }

            if (propertyName == "Father")
            {
                father = SplitName(jobType, nodeString, null, null); //father name
            }
            else if (propertyName == "FatherLink" && nodeConditions)
            {
                father = SplitLink(nodeString, jobType, propertyName); //father link
            }
            else if (propertyName == "FatherLink" && !nodeConditions)
            {
                father = ""; //father link
            }

            return father;
        }

        public HtmlDocument GetHtmlAgility(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        //TODO: dictionary
        public bool VerifyNodeCondition(string node, string propertyName)
        {
            List<string> conditionWords = new List<string>();

            if (propertyName == null)
            {
                conditionWords.Add("Jeździec"); //for JockeysPl
                conditionWords.Add("Licence jezdce"); //for JockeysCz profile
                conditionWords.Add("Rasa"); //for HorsesPl
                conditionWords.Add("celkem"); //for HorsesCz
                conditionWords.Add("vysledky.php?id_dostih="); //for JockeysCz race row
                conditionWords.Add("Pula nagród"); //for HistoricPl (not sent prop name)
                conditionWords.Add("zł"); //for HistoricPl (not sent prop name)
            }
            if (propertyName == "Father" || propertyName == "FatherLink")
            {
                conditionWords.Add("<a href="); //for HorsesPl father
            }
            if (propertyName == "AllRaces")
            {
                conditionWords.Add("&nbsp;m"); //for HorsesPl race row
            }
            if (propertyName == "AllChildren")
            {
                conditionWords.Add("ogier"); //for HorsesPl child row
                conditionWords.Add("klacz"); //for HorsesPl child row
                conditionWords.Add("wałach"); //for HorsesPl child row
            }

            bool verify = conditionWords.Any(node.Contains);

            return verify;
        }

        public string FormatName(string name, string jobType)
        {
            if (name.Contains(" "))
            {
                //format jockey name (X. Xxxx)
                if (jobType.Contains("JockeysPl"))
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
                else if (jobType.Contains("Horses") || jobType.Contains("Historic"))
                {
                    return name;
                }
            }

            return name;
        }

        public string FilterLetters(string name)
        {
            var filtered = name.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

            return new string(filtered.ToArray());
        }

        private HorseDataWrapper SplitHorseNodeString(string jobType, string nodeString, string propertyName, DateTime raceDate)
        {
            HorseDataWrapper horse = new HorseDataWrapper();

            string name = SplitName(jobType, nodeString, typeof(HorseDataWrapper), null);
            string age = SplitAge(jobType, nodeString, raceDate, propertyName);
            string racer = SplitName(jobType, nodeString, typeof(LoadedJockey), null);
            string link = "";

            horse = ParseHorseData(raceDate, name, age, link, racer, jobType);

            return horse;
        }

        private HorseChildDetails SplitChildNodeString(string jobType, string nodeString, string propertyName, DateTime raceDate)
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

        private string SplitLink(string nodeString, string jobType, string propertyName)
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

            return place;
        }

        public string SplitName(string jobType, string nameNodeString, Type type, string propertyName)
        {
            string name = "";
            if (jobType.Contains("JockeysPl"))
            {
                if (type == typeof(LoadedJockey))
                {
                    name = nameNodeString.Split(new string[] { " - " }, StringSplitOptions.None)[1].Split('<')[0].Trim(' ');
                }
                else if (type == typeof(LoadedHorse))
                {
                    name = nameNodeString.Split('>')[10].Split('<')[0].Trim(' ');
                }
            }
            else if (jobType.Contains("JockeysCz"))
            {
                if (type == typeof(LoadedJockey))
                {
                    name = nameNodeString.Split(new string[] { "<b>" }, StringSplitOptions.None)[3].Split('<')[0].Trim(' ');
                }
                else if (type == typeof(LoadedHorse))
                {
                    name = nameNodeString.Split('>')[23].Split('<')[0].Trim(' ');
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
                    name = nameNodeString.Split('>')[7].Split('<')[0].Trim(' ');
                }
                else if (type == typeof(LoadedJockey))
                {
                    name = nameNodeString.Split('>')[13].Split('<')[0].Trim(' ');
                    string toReplace = name.Split(' ')[0];
                    name = name.Replace(toReplace, "").Trim(' ');
                }
            }
            else if (jobType.Contains("HorsesPl"))
            {
                if (type == typeof(LoadedHorse))
                {
                    if (propertyName == "AllRaces")
                    {
                        name = "";
                    }
                    else if (propertyName == "AllChildren")
                    {
                        name = nameNodeString.Split('>')[3].Split('<')[0].Trim(' ');
                    }
                    else
                    {
                        name = nameNodeString.Split(new string[] { "<title>" }, StringSplitOptions.None)[1].Split('(')[0].Trim(' ');
                    }
                }
                else if (type == typeof(LoadedJockey))
                {
                    name = nameNodeString.Split('>')[15].Split('<')[0].Trim(' ');
                    string toReplace = name.Split(' ')[0];
                    name = name.Replace(toReplace, "").Trim(' ');
                }
                else if (type == null)
                {
                    name = nameNodeString.Split('>')[1].Split('<')[0].Trim(' ');
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
                    //
                }
            }

            if (type != typeof(LoadedHorse) && type != typeof(HorseDataWrapper) && type != null)
            {
                name = FormatName(name, jobType);
            }
            if (type == typeof(LoadedJockey))
            {
                if (name.StartsWith("dż. ")) name = name.Replace("dż. ", "");
                if (name.StartsWith("u. ")) name = name.Replace("u. ", "");
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

                return date;
        }

        private string MakeTitleCase(string name)
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
                    race.WonPlace = race.RaceCompetition; //won place
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
                race.RaceDate = DateTime.Now; //day of race
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
                horse.Age = int.Parse(age) + (DateTime.Now.Year - raceDate.Year);
            }
            else if (parseTest && jobType.Contains("HorsesPl"))
            {
                horse.Age = int.Parse(age); //age
                if (horse.Age > 199)
                {
                    horse.Age = DateTime.Now.Year - horse.Age; //age
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

        private LoadedJockey ParseJockeyData(string link, string name)
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

    }
}