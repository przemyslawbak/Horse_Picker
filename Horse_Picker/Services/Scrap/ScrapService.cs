using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Horse_Picker.Models;
using Horse_Picker.Services.Dictionary;
using Horse_Picker.Services.Parse;
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
        private IParseService _parseService;

        public ScrapService(IDictionariesService dictionaryService,
            IParseService parseService)
        {
            _dictionaryService = dictionaryService;
            _parseService = parseService;
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

            bool nodeConditions = _parseService.VerifyNodeCondition(nodeString, null, jobType);

            if (typeof(T) == typeof(LoadedJockey))
            {
                if (nodeConditions)
                {
                    string name = _parseService.SplitName(jobType, nodeString, typeof(T), null);

                    raceHtmlAgilityList = await GetRaceHtmlAgilityListAsync(jobType, link);

                    jockey = _parseService.ParseJockeyData(link, name);
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

                    raceHtmlAgilityList = await GetRaceHtmlAgilityListAsync(jobType, link);

                    race = _parseService.SplitRaceNodeString(jobType, nodeString, null);
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

                    raceHtmlAgilityList = await GetRaceHtmlAgilityListAsync(jobType, link);

                    string name = _parseService.SplitName(jobType, nodeString, typeof(T), null);
                    string age = _parseService.SplitAge(jobType, nodeString, _dateToday, nameof(horse.Age));
                    string racer = "";

                    wrapper = _parseService.ParseHorseData(_dateToday, name, age, link, racer, jobType);

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

        public List<T> GetGenericList<T>(List<HtmlDocument> raceHtmlAgilityList, string propertyName, Dictionary<string, string> nodeDictionary, string jobType, DateTime raceDate)
        {
            List<HorseChildDetails> children = new List<HorseChildDetails>();

            List<RaceDetails> races = new List<RaceDetails>();

            List<HorseDataWrapper> horses = new List<HorseDataWrapper>();

            string xpath = nodeDictionary[propertyName];

            bool nodeConditions = false;

            foreach (var raceHtmlAgility in raceHtmlAgilityList)
            {
                nodeConditions = _parseService.VerifyNodeCondition(raceHtmlAgility.DocumentNode.OuterHtml.ToString(), propertyName, jobType);

                if (nodeConditions) //check complete page
                {
                    HtmlNode[] tableRowsNode = raceHtmlAgility.DocumentNode.SelectNodes(xpath).ToArray();

                    if (tableRowsNode.Length > 0)
                    {
                        foreach (var row in tableRowsNode)
                        {
                            string nodeString = row.OuterHtml.ToString();

                            nodeConditions = _parseService.VerifyNodeCondition(nodeString, propertyName, jobType);

                            if (nodeConditions) //check single row
                            {
                                if (typeof(T) == typeof(HorseChildDetails))
                                {
                                    HorseChildDetails child = new HorseChildDetails();

                                    child = _parseService.SplitChildNodeString(jobType, nodeString, propertyName, raceDate);

                                    children.Add(child);
                                }
                                else if (typeof(T) == typeof(RaceDetails))
                                {
                                    RaceDetails race = new RaceDetails();

                                    race = _parseService.SplitRaceNodeString(jobType, nodeString, propertyName);

                                    races.Add(race);
                                }
                                else if (typeof(T) == typeof(HorseDataWrapper))
                                {

                                    HorseDataWrapper horse = new HorseDataWrapper();

                                    horse = _parseService.SplitHorseNodeString(jobType, nodeString, propertyName, raceDate);

                                    horses.Add(horse);
                                }
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

        public async Task<List<HtmlDocument>> GetRaceHtmlAgilityListAsync(string jobType, string link)
        {
            List<HtmlDocument> raceHtmlAgilityList = new List<HtmlDocument>();
            StringBuilder sb;
            int year = _yearMax;
            List<string> links = new List<string>();

            if (jobType.Contains("Jockeys"))
            {
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
            }
            else if (jobType.Contains("HorsesPl"))
            {
                links.Add(link);
            }
            else if (jobType.Contains("HorsesCz"))
            {
                links.Add(link);
                links.Add(link + "&pod=potomci");
            }
            else if (jobType.Contains("HistoricPl"))
            {
                links.Add(link);
            }

            foreach (string item in links)
            {
                string html = await GetHtmlDocumentAsync(item);

                HtmlDocument newDoc = GetHtmlAgility(html);

                raceHtmlAgilityList.Add(newDoc);
            }

            return raceHtmlAgilityList;
        }

        public static async Task<string> GetHtmlDocumentAsync(string link)
        {
            string result = "";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(link);

                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }

        public string GetFather(string jobType, HtmlDocument htmlAgility, string propertyName, Dictionary<string, string> nodeDictionary)
        {
            string father = "";

            string xpath = nodeDictionary[propertyName];

            HtmlNode node = htmlAgility.DocumentNode.SelectSingleNode(xpath);

            string nodeString = node.OuterHtml.ToString();

            bool nodeConditions = _parseService.VerifyNodeCondition(nodeString, propertyName, jobType);

            if (jobType.Contains("HorsesPl"))
            {
                if (nodeConditions) //check for anchor
                {
                    xpath = xpath + "/a";
                    node = htmlAgility.DocumentNode.SelectSingleNode(xpath);
                    nodeString = node.OuterHtml.ToString();
                }
            }

            if (propertyName == "Father")
            {
                father = _parseService.SplitName(jobType, nodeString, null, null); //father name
            }
            else if (propertyName == "FatherLink" && nodeConditions)
            {
                father = _parseService.SplitLink(nodeString, jobType, propertyName); //father link
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
    }
}