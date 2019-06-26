using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Horse_Picker.Models;
using Horse_Picker.Wrappers;
using HtmlAgilityPack;
using PuppeteerSharp;

namespace Horse_Picker.Services.Scrap
{
    public class ScrapService : IScrapService
    {
        public async Task<T> ScrapGenericObject<T>(int id, string jobType)
        {
            LoadedJockey jockey = new LoadedJockey();
            LoadedHorse horse = new LoadedHorse();
            LoadedHistoricalRace race = new LoadedHistoricalRace();

            string linkBase = "";
            string html = "";

            if (jobType.Contains("JockeysPl"))
            {
                linkBase = "https://koniewyscigowe.pl/dzokej?d=";
            }
            if (jobType.Contains("JockeysCz"))
            {
                linkBase = "http://dostihyjc.cz/jezdec.php?IDTR=";
            }
            //add horse + race

            if (typeof(T) == typeof(LoadedJockey))
            {
                jockey.Link = GetLink(linkBase, id);

                html = await GetHtmlDocumentAsync(jockey.Link);

                jockey.Name = GetJockeyName(html);

                //jockey.AllRaces = GetJockeyAllRaces();
            }


            if (typeof(T) == typeof(LoadedJockey))
            {
                return (T)Convert.ChangeType(jockey, typeof(LoadedJockey));
            }
            else if (typeof(T) == typeof(LoadedHorse))
            {
                horse = new LoadedHorse();//XXXXXXXXXXXXX
                return (T)Convert.ChangeType(horse, typeof(LoadedHorse));
            }
            else if (typeof(T) == typeof(LoadedHistoricalRace))
            {
                race = new LoadedHistoricalRace();//XXXXXXXXXXXXX
                return (T)Convert.ChangeType(race, typeof(LoadedHistoricalRace));
            }
            else { throw new ArgumentException(); }
        }

        /////////////////////////////////OK///////////////////////////////////////////

        private string GetLink(string linkBase, int id)
        {
            return linkBase + id;
        }

        private string GetJockeyName(string html)
        {
            throw new NotImplementedException();
        }

        private List<JockeyRaceDetails> GetJockeyAllRaces()
        {
            throw new NotImplementedException();
        }

        ////////////////////////////////////////////////////////////////////////////

        public async Task<LoadedJockey> ScrapSingleJockeyPlAsync(int index)
        {
            LoadedJockey jockey = new LoadedJockey();
            await Task.Run(() =>
            {
                int n;
                List<JockeyRaceDetails> allRaces = new List<JockeyRaceDetails>();

                for (int year = DateTime.Now.Year; year > 2013; year--)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("https://koniewyscigowe.pl/dzokej?d=");
                    sb.Append(index);
                    sb.Append("&sezon=");
                    sb.Append(year);
                    sb.Append("#wyniki_koni");

                    string link = sb.ToString();

                    HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc = web.Load(link);

                    //gets the name of the jockey
                    if (jockey.Name == null)
                    {
                        try
                        {
                            HtmlNode singleNode = doc.DocumentNode.SelectSingleNode("/html/body/main/section[1]/div[1]/h3");
                            string racersName = singleNode.OuterHtml.ToString();
                            if (racersName.Contains("Jeździec") && racersName.Length > 65)
                            {
                                racersName = racersName.Split('>')[1].Split(new string[] { " - " }, StringSplitOptions.None)[1].Split('<')[0].Trim(' ');
                                if (racersName.Contains(" "))
                                {
                                    char letter = racersName[0];
                                    racersName = racersName.Split(' ')[1].Trim(' ');
                                    racersName = letter + ". " + racersName;
                                }

                                racersName = MakeTitleCase(racersName);

                                jockey.Name = racersName; //jockeys name
                            }
                            else
                            {
                                racersName = "";
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    //scrap races
                    HtmlNode[] tableRow = doc.DocumentNode.SelectNodes("//*[@id=\"wykaz_list\"]/tbody/tr").ToArray();
                    if (tableRow != null && tableRow.Length > 0)
                    {
                        foreach (var row in tableRow)
                        {
                            string stringTableRow = row.OuterHtml.ToString();

                            //all races
                            //if row not contains 'brak startow' and contains m (meters)
                            if (!stringTableRow.Contains("Brak danych") && (stringTableRow.Contains("&nbsp;m") || stringTableRow.Contains(" m")))
                            {
                                try
                                {
                                    JockeyRaceDetails race = new JockeyRaceDetails();

                                    string raceDate = stringTableRow.Split('>')[3].Split('<')[0].Trim(' ');
                                    string raceDistance = stringTableRow.Split('>')[8].Split(' ')[0].Trim(' ');
                                    string horsesName = stringTableRow.Split('>')[10].Split('<')[0].Trim(' ');
                                    horsesName = MakeTitleCase(horsesName);
                                    string raceScore = stringTableRow.Split('>')[12].Split('<')[0].Trim(' ');
                                    string racePlace = raceScore.Split('/')[0].Trim(' ');
                                    string raceCompetitors = raceScore.Split('/')[1].Trim(' ');

                                    race = ParseJockeyRaceData(raceDate,
                                        raceDistance,
                                        horsesName,
                                        raceScore,
                                        racePlace,
                                        raceCompetitors);

                                    allRaces.Add(race);
                                }
                                catch (Exception e)
                                {

                                }
                            }
                        }
                    }
                }
                jockey.AllRaces = allRaces; //jockeys races between 2015-2019
                jockey.Link = "https://koniewyscigowe.pl/dzokej?d=" + index; //racers link

                if (jockey.AllRaces.Count == 0)
                {
                    jockey.Name = null;
                }

            });
            return jockey;
        }

        public async Task<LoadedJockey> ScrapSingleJockeyCzAsync(int index)
        {
            LoadedJockey jockey = new LoadedJockey();
            await Task.Run(() =>
            {

                int n;
                List<JockeyRaceDetails> allRaces = new List<JockeyRaceDetails>();

                for (int year = DateTime.Now.Year; year > 2013; year--)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("http://dostihyjc.cz/jezdec.php?IDTR=");
                    sb.Append(index);
                    sb.Append("&rok=");
                    sb.Append(year);
                    sb.Append("&pod=kariera");

                    string link = sb.ToString();

                    //to be sure if jockey was riding in our time period
                    List<string> years = new List<string>();
                    for (int check = DateTime.Now.Year; check > 2009; check--)
                    {
                        years.Add(check.ToString());
                    }

                    HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc = web.Load(link);

                    //check time period
                    HtmlNode timePeriods = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[1]/table[1]");
                    string timeTable = timePeriods.OuterHtml.ToString();

                    if (years.Any(timeTable.Contains))
                    {//gets the name of the jockey
                        if (jockey.Name == null)
                        {
                            try
                            {

                                HtmlNode[] singleNode = doc.DocumentNode.Descendants("b").ToArray();
                                string racersName = singleNode[2].OuterHtml.ToString();
                                racersName = racersName.Split('>')[1].Split('<')[0].Trim(' ');
                                char letter = racersName.Split(' ')[1][0];
                                racersName = racersName.Split(' ')[0].Trim(' ');
                                racersName = letter + ". " + racersName;

                                racersName = MakeTitleCase(racersName);

                                jockey.Name = racersName; //jockeys name
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        //scrap races
                        HtmlNode[] tableRows = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[4]/tr").ToArray();
                        if (tableRows != null && tableRows.Length > 0)
                        {
                            foreach (var row in tableRows)
                            {
                                string stringTableRow = row.OuterHtml.ToString();

                                //all races
                                //if row not contains 'brak startow' and contains m (meters)
                                if (!stringTableRow.Contains("nebyl nalezen žádný záznam"))
                                {
                                    try
                                    {
                                        JockeyRaceDetails race = new JockeyRaceDetails();

                                        string raceDate = stringTableRow.Split('>')[3].Split('<')[0].Trim(' ');
                                        string raceDistance = stringTableRow.Split('>')[19].Split('<')[0].Trim(' ');
                                        string horsesName = stringTableRow.Split('>')[23].Split('<')[0].Trim(' ');
                                        if (horsesName.Contains("("))
                                        {
                                            horsesName = horsesName.Split('(')[0].Trim(' ');
                                        }
                                        horsesName = MakeTitleCase(horsesName);
                                        string raceScore = stringTableRow.Split('>')[31].Split('<')[0].Trim(' ');
                                        string racePlace = raceScore.Split(' ')[0].Trim(' ');
                                        string raceCompetitors = raceScore.Split(' ')[2].Trim(' ');

                                        race = ParseJockeyRaceData(raceDate,
                                        raceDistance,
                                        horsesName,
                                        raceScore,
                                        racePlace,
                                        raceCompetitors);

                                        allRaces.Add(race);
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
                jockey.AllRaces = allRaces; //jockeys races between 2015-2019
                jockey.Link = "http://dostihyjc.cz/jezdec.php?IDTR=" + index; //racers link

                if (jockey.AllRaces.Count == 0)
                {
                    jockey.Name = null;
                }
            });

            return jockey;
        }



        public async Task<LoadedHorse> ScrapSingleHorsePlAsync(int index)
        {
            LoadedHorse horse = new LoadedHorse();

            await Task.Run(() =>
            {
                int n;
                List<HorseChildDetails> allChildren = new List<HorseChildDetails>();
                List<HorseRaceDetails> allRaces = new List<HorseRaceDetails>();
                string link = "https://koniewyscigowe.pl/horse/" + index;

                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(link);

                if (doc.DocumentNode.OuterHtml.ToString().Contains("Rasa"))
                {
                    //horses name
                    HtmlNode horsesName = doc.DocumentNode.SelectSingleNode("/html/body/main/section[1]/div/div[2]/div[2]/header/div/h1");
                    try
                    {
                        if (horsesName != null)
                        {
                            string theName = horsesName.OuterHtml.ToString();
                            theName = theName.Split('>')[1].Split('<')[0].Trim(' ');

                            theName = MakeTitleCase(theName);

                            horse.Name = theName; //name
                            horse.Link = link; //link
                        }
                    }
                    catch (Exception e)
                    {

                    }

                    //if horse exists at all
                    if (horsesName != null)
                    {
                        try
                        {
                            //horses age
                            HtmlNode horsesAge = doc.DocumentNode.SelectSingleNode("/html/body/main/section[1]/div/div[2]/div[2]/div/table/tbody/tr[1]/th/strong");
                            if (horsesAge != null)
                            {
                                string theAge = horsesAge.OuterHtml.ToString();
                                int count = theAge.Count(s => s == '.');
                                bool containsInt = theAge.Any(char.IsDigit);
                                if (theAge.Contains("lat"))
                                {
                                    theAge = theAge.Split('(')[1].Split('l')[0].Trim(' ');
                                }
                                else if (containsInt)
                                {
                                    if (count > 2)
                                    {
                                        theAge = theAge.Split('.')[3].Split('<')[0].Trim(' ');
                                    }
                                    else
                                    {
                                        theAge = theAge.Split('.')[1].Split('(')[0].Trim(' ');
                                    }
                                    if (theAge.Contains('<'))
                                    {
                                        theAge = theAge.Split('<')[0].Trim(' ');
                                    }
                                }

                                bool parseTestN = int.TryParse(theAge, out n);
                                if (parseTestN)
                                {
                                    horse.Age = int.Parse(theAge); //age
                                    if (horse.Age > 199)
                                    {
                                        horse.Age = DateTime.Now.Year - horse.Age; //age
                                    }
                                }
                                else
                                {
                                    horse.Age = 99; //age
                                }
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        //horses father
                        HtmlNode horsesFather = doc.DocumentNode.SelectSingleNode("/html/body/main/section[1]/div/div[2]/div[2]/div/table/tbody/tr[4]/td");
                        if (horsesFather != null)
                        {
                            string fathersName = "";
                            string fathersLink = "";

                            try
                            {
                                if (horsesFather.OuterHtml.ToString().Contains("<a")) //check if there is anchor
                                {
                                    horsesFather = doc.DocumentNode.SelectSingleNode("/html/body/main/section[1]/div/div[2]/div[2]/div/table/tbody/tr[4]/td/a");
                                    fathersLink = horsesFather.OuterHtml.ToString();
                                    fathersLink = fathersLink.Split('"')[1].Split('-')[0].Trim(' ');
                                    fathersLink = "https://koniewyscigowe.pl" + fathersLink;

                                    //name of horses father
                                    fathersName = horsesFather.OuterHtml.ToString();
                                    fathersName = fathersName.Split('>')[1].Split('<')[0].Trim(' ');
                                    if (fathersName.Contains('('))
                                    {
                                        fathersName = fathersName.Split('(')[0].Trim(' ');
                                    }
                                }
                                else
                                {
                                    fathersName = horsesFather.OuterHtml.ToString();
                                    if (fathersName != "")
                                    {
                                        fathersName = fathersName.Split('>')[1].Split('<')[0].Trim(' ');
                                    }
                                    fathersLink = "";
                                }

                                fathersName = MakeTitleCase(fathersName);

                                horse.Father = fathersName; //fathers name
                                horse.FatherLink = fathersLink; //fathers link
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        //table row
                        HtmlNode[] tableRow = doc.DocumentNode.SelectNodes("//*[@id=\"wykaz\"]/tbody/tr").ToArray();

                        if (tableRow != null && tableRow.Length > 0)
                        {
                            foreach (var row in tableRow)
                            {
                                string stringTableRow = row.OuterHtml.ToString();

                                //horses children
                                if (!stringTableRow.Contains("Brak startów") && (stringTableRow.Contains("ogier") || stringTableRow.Contains("klacz") || stringTableRow.Contains("wałach")))
                                {
                                    try
                                    {
                                        HorseChildDetails child = new HorseChildDetails();

                                        string childName = stringTableRow.Split('>')[3].Split('<')[0].Trim(' ');

                                        childName = MakeTitleCase(childName);

                                        child.ChildName = childName; //child name

                                        string childLink = stringTableRow.Split('"')[3].Split('-')[0].Trim(' ');
                                        childLink = "https://koniewyscigowe.pl" + childLink;
                                        child.ChildLink = childLink; //child link

                                        string childAge = stringTableRow.Split('>')[8].Split('<')[0].Trim(' ');

                                        bool parseTestN = int.TryParse(childAge, out n);
                                        if (parseTestN)
                                        {
                                            child.ChildAge = DateTime.Now.Year - int.Parse(childAge); //child age
                                        }
                                        else
                                        {
                                            child.ChildAge = 99;
                                        }

                                        allChildren.Add(child);
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }

                                //all races
                                //if row not contains 'brak startow' and contains m (meters)
                                if (!stringTableRow.Contains("Brak startów") && stringTableRow.Contains("&nbsp;m"))
                                {
                                    try
                                    {
                                        HorseRaceDetails race = new HorseRaceDetails();

                                        string racersName = stringTableRow.Split('>')[15].Split('<')[0].Trim(' ');
                                        string toReplace = racersName.Split(' ')[0];
                                        racersName = racersName.Replace(toReplace, "").Trim(' ');
                                        if (racersName.Contains("dż. ")) racersName = racersName.Replace("dż. ", "");
                                        if (racersName.Contains("u. ")) racersName = racersName.Replace("u. ", "");
                                        racersName = MakeTitleCase(racersName);
                                        string raceDistance = stringTableRow.Split('>')[10].Split('&')[0].Trim(' ');
                                        string raceCategory = stringTableRow.Split('>')[12].Split('<')[0].Trim(' ');
                                        string raceScore = stringTableRow.Split('>')[24].Split('<')[0].Trim(' ');
                                        string racePlace = raceScore.Split('/')[0].Trim(' ');
                                        string raceCompetitors = raceScore.Split('/')[1].Trim(' ');
                                        string raceDate = stringTableRow.Split('>')[4].Split('<')[0].Trim(' ');
                                        string raceLink = stringTableRow.Split('"')[3].Split('-')[0].Trim(' ');
                                        raceLink = "https://koniewyscigowe.pl" + raceLink;
                                        string racersLink = stringTableRow.Split('"')[13].Split('-')[0].Trim(' ');
                                        racersLink = "https://koniewyscigowe.pl" + racersLink;

                                        race = ParseHorseRaceData(racePlace,
                                            raceCompetitors,
                                            raceDistance,
                                            raceDate,
                                            raceCategory,
                                            raceLink,
                                            racersLink,
                                            racersName);

                                        allRaces.Add(race);
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }
                            }
                        }
                    }
                    horse.AllChildren = allChildren; //children
                    horse.AllRaces = allRaces; //races

                    if (horse.AllChildren.Count == 0 && horse.AllRaces.Count == 0 || horse.Age == 0)
                    {
                        horse.Name = null;
                    }
                }
            });

            return horse;
        }

        public async Task<LoadedHorse> ScrapSingleHorseCzAsync(int index)
        {
            LoadedHorse horse = new LoadedHorse();

            await Task.Run(() =>
            {
                int n;
                List<HorseChildDetails> allChildren = new List<HorseChildDetails>();
                List<HorseRaceDetails> allRaces = new List<HorseRaceDetails>();
                string link = "http://dostihyjc.cz/kun.php?ID=" + index;

                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(link);
                if (doc.DocumentNode.OuterHtml.ToString().Contains("celkem"))
                {
                    //horses name
                    HtmlNode[] boldNodes = doc.DocumentNode.Descendants("b").ToArray();
                    if (boldNodes != null)
                    {
                        try
                        {

                            string theName = boldNodes[2].OuterHtml.ToString();
                            theName = theName.Split('>')[1].Split('(')[0].Trim(' ');
                            if (theName.Contains('<'))
                            {
                                theName = theName.Split('<')[0].Trim(' ');
                            }
                            if (theName.Contains('.'))
                            {
                                theName = theName.Replace(".", "").Trim(' ');
                            }
                            if (theName.Contains("celkem:"))
                            {
                                boldNodes = null;
                            }
                            else
                            {
                                theName = MakeTitleCase(theName);

                                horse.Name = theName; //name
                                horse.Link = link; //link
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    else
                    {
                        horse.Name = null;
                    }

                    if (boldNodes != null)
                    {
                        try
                        {
                            //horses age
                            string theAge = boldNodes[3].OuterHtml.ToString();
                            theAge = theAge.Split('/')[1].Split('<')[0].Trim(' ');

                            bool parseTestN = int.TryParse(theAge, out n);
                            if (parseTestN)
                            {
                                horse.Age = int.Parse(theAge); //age
                                if (horse.Age > 199)
                                {
                                    horse.Age = DateTime.Now.Year - horse.Age; //age
                                }
                            }
                            else
                            {
                                horse.Age = 99; //age
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        //father
                        HtmlNode fathersNode = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[2]/tr[1]");
                        if (fathersNode != null)
                        {
                            try
                            {
                                string fathersName = fathersNode.OuterHtml.ToString();
                                fathersName = fathersName.Split('>')[9].Split('(')[0].Trim(' ').Replace("\n", "");
                                if (fathersName.Contains('-'))
                                {
                                    fathersName = fathersName.Split('-')[0].Trim(' ');
                                }

                                fathersName = MakeTitleCase(fathersName);

                                horse.Father = fathersName; //fathers name
                                horse.FatherLink = ""; //fathers link
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        //all races
                        HtmlNode[] tableRow = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[4]/tr").ToArray();
                        if (tableRow != null)
                        {
                            foreach (var row in tableRow)
                            {
                                string stringTableRow = row.OuterHtml.ToString();
                                if (!stringTableRow.Contains("nebyl nalezen žádný záznam") && stringTableRow.Contains("Kč"))
                                {
                                    try
                                    {
                                        HorseRaceDetails race = new HorseRaceDetails();

                                        string racersName = "";
                                        string toReplace = "";
                                        if (!stringTableRow.Contains("<span class='text6'></span>"))
                                        {
                                            racersName = stringTableRow.Split('>')[23].Split('<')[0].Trim(' ');
                                            StringBuilder sb = new StringBuilder(racersName);
                                            sb.Replace("ž. ", "");
                                            sb.Replace("žk. ", "");
                                            sb.Replace("am. ", "");
                                            sb.Replace("ž ", "");
                                            sb.Replace("žk ", "");
                                            sb.Replace("am ", "");
                                            racersName = sb.ToString();
                                            if (racersName.Contains(" "))
                                            {
                                                toReplace = racersName.Split(' ')[1].Trim(' ');
                                                racersName = toReplace + " " + racersName.Split(' ')[0].Trim(' ');
                                            }
                                        }
                                        racersName = MakeTitleCase(racersName);
                                        string raceDistance = stringTableRow.Split('>')[19].Split('<')[0].Trim(' ');
                                        string raceCategory = stringTableRow.Split('>')[15].Split('<')[0].Trim(' ');
                                        string raceScore = stringTableRow.Split('>')[31].Split('<')[0].Trim(' ');
                                        string racePlace = raceScore.Split('/')[0].Trim(' ');
                                        string raceCompetitors = raceScore.Split('/')[1].Trim(' ');
                                        string raceDate = stringTableRow.Split('>')[3].Split('<')[0].Trim(' ');
                                        string raceLink = stringTableRow.Split('?')[1].Split('\'')[0].Trim(' ');
                                        raceLink = "http://dostihyjc.cz/vysledky.php?" + raceLink;
                                        string racersLink;
                                        if (stringTableRow.Contains("jezdec.php?IDTR"))
                                        {
                                            racersLink = stringTableRow.Split('?')[2].Split('\'')[0].Trim(' ');
                                            racersLink = "http://dostihyjc.cz/jezdec.php?" + racersLink;
                                        }
                                        else
                                        {
                                            racersLink = "";
                                        }

                                        race = ParseHorseRaceData(racePlace,
                                            raceCompetitors,
                                            raceDistance,
                                            raceDate,
                                            raceCategory,
                                            raceLink,
                                            racersLink,
                                            racersName);

                                        allRaces.Add(race);
                                    }
                                    catch (Exception e)
                                    {
                                    }
                                }
                            }
                        }
                    }
                    //horses children
                    link = "http://dostihyjc.cz/kun.php?ID=" + index + "&pod=potomci";

                    web = new HtmlWeb();
                    doc = web.Load(link);

                    if (!doc.DocumentNode.OuterHtml.ToString().Contains("Nebyli nalezeni žádní potomci"))
                    {
                        HtmlNode[] childRows = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[3]/tr").ToArray();
                        foreach (var row in childRows)
                        {
                            string stringChildRow = row.OuterHtml.ToString();
                            if (!stringChildRow.Contains("potomek") && !stringChildRow.Contains("kun<"))
                            {
                                try
                                {
                                    HorseChildDetails child = new HorseChildDetails();

                                    string childName = stringChildRow.Split('>')[3].Split(',')[0].Trim(' ');
                                    if (childName.Contains("("))
                                    {
                                        childName = childName.Split('(')[0].Trim(' ');
                                    }

                                    childName = MakeTitleCase(childName);

                                    child.ChildName = childName; //child name

                                    if (stringChildRow.Contains("kun.php?ID"))
                                    {
                                        string childLink = stringChildRow.Split('?')[1].Split('\'')[0].Trim(' ');
                                        childLink = "http://dostihyjc.cz/kun.php?" + childLink;
                                        child.ChildLink = childLink; //child link
                                    }
                                    if (stringChildRow.Contains(","))
                                    {
                                        string childAge = stringChildRow.Split(',')[1].Split('<')[0].Trim(' ');

                                        bool parseTestN = int.TryParse(childAge, out n);
                                        if (parseTestN)
                                        {
                                            child.ChildAge = int.Parse(childAge); //child age
                                        }
                                        else
                                        {
                                            child.ChildAge = 99; //child age
                                        }
                                    }
                                    else
                                    {
                                        child.ChildAge = 99;
                                    }

                                    allChildren.Add(child);
                                }
                                catch (Exception e)
                                {

                                }
                            }
                        }
                    }
                    horse.AllChildren = allChildren; //children
                    horse.AllRaces = allRaces; //races

                    if (horse.AllChildren.Count == 0 && horse.AllRaces.Count == 0 || horse.Age == 0)
                    {
                        horse.Name = null;
                    }
                }
            });

            return horse;
        }

        private HorseRaceDetails ParseHorseRaceData(string racePlace,
           string raceCompetitors,
           string raceDistance,
           string raceDate,
           string raceCategory,
           string raceLink,
           string racersLink,
           string racersName)
        {
            HorseRaceDetails race = new HorseRaceDetails();

            int n;
            bool parseTest;

            DateTime t;

            parseTest = int.TryParse(racePlace, out n);
            if (parseTest)
            {
                //if not disqualified
                if (racePlace != "0" && racePlace != "D" && racePlace != "PN" && racePlace != "Z" && racePlace != "ZN" && racePlace != "SN")
                {
                    race.WonPlace = int.Parse(racePlace); //won place
                }
                else
                {
                    race.WonPlace = int.Parse(raceCompetitors); //won place
                }
            }

            parseTest = int.TryParse(raceDistance, out n);
            if (parseTest)
            {
                race.RaceDistance = int.Parse(raceDistance); //race distance
            }

            parseTest = int.TryParse(raceCompetitors, out n);
            if (parseTest)
            {
                race.RaceCompetition = int.Parse(raceCompetitors); //race qty of horses
            }

            parseTest = DateTime.TryParse(raceDate, out t);
            if (parseTest)
            {
                race.RaceDate = DateTime.Parse(raceDate); //day of race
            }

            race.RaceCategory = raceCategory; //category or group of race

            race.RaceLink = raceLink; //link to the race

            race.RacersLink = racersLink; //link tho the racer

            race.RacersName = racersName; //name of the racer

            return race;
        }



        private JockeyRaceDetails ParseJockeyRaceData(string raceDate,
            string raceDistance,
            string horsesName,
            string raceScore,
            string racePlace,
            string raceCompetitors)
        {
            JockeyRaceDetails race = new JockeyRaceDetails();
            int n;
            bool parseTest;
            DateTime t;

            if (racePlace != "0" && racePlace != "D" && racePlace != "PN" && racePlace != "Z" && racePlace != "ZN" && racePlace != "SN")
            {
                parseTest = int.TryParse(racePlace, out n);
                if (parseTest)
                {
                    race.WonPlace = int.Parse(racePlace); //won place
                }
            }
            else
            {
                parseTest = int.TryParse(raceCompetitors, out n);
                if (parseTest)
                {
                    race.WonPlace = int.Parse(raceCompetitors); //won place
                }
            }

            parseTest = int.TryParse(raceDistance, out n);
            if (parseTest)
            {
                race.RaceDistance = int.Parse(raceDistance); //race distance
            }

            parseTest = int.TryParse(raceCompetitors, out n);
            if (parseTest)
            {
                race.RaceCompetition = int.Parse(raceCompetitors); //race qty of horses
            }

            parseTest = DateTime.TryParse(raceDate, out t);
            if (parseTest)
            {
                race.RaceDate = DateTime.Parse(raceDate); //day of race
            }

            race.HorsesName = horsesName; //rided horse

            return race;
        }

        public async Task<LoadedHistoricalRace> ScrapSingleRacePlAsync(int index)
        {
            LoadedHistoricalRace race = new LoadedHistoricalRace();

            await Task.Run(() =>
            {
                bool parseTest;
                int n;
                DateTime t;
                string link = "https://koniewyscigowe.pl/wyscig?w=" + index;

                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(link);

                Dictionary<string, int> monthDict = new Dictionary<string, int>();
                monthDict.Add("Kwiecień", 4);
                monthDict.Add("Maja", 5);
                monthDict.Add("Czerwca", 6);
                monthDict.Add("Lipca", 7);
                monthDict.Add("Sierpienia", 8);
                monthDict.Add("Września", 9);
                monthDict.Add("Października", 10);
                monthDict.Add("Listopada", 11);
                monthDict.Add("Grudnia", 12);

                if (doc.DocumentNode.OuterHtml.ToString().Contains("Wynik gonitwy") && !doc.DocumentNode.OuterHtml.ToString().Contains("rekord"))
                {
                    //race distance
                    HtmlNode raceDistance = doc.DocumentNode.SelectSingleNode("/html/body/main/section/div[3]/table/tbody/tr[2]/td");

                    string distance = raceDistance.OuterHtml.ToString();

                    if (distance.Contains("Pula nagród"))
                    {
                        try
                        {
                            distance = distance.Split(new string[] { "Dystans " }, StringSplitOptions.None)[1].Split('m')[0].Trim(' ');

                            parseTest = int.TryParse(distance, out n);
                            if (parseTest)
                            {
                                race.RaceDistance = int.Parse(distance);// distance data
                            }
                            else
                            {
                                race.RaceDistance = 2000;// distance data
                            }

                            string category = raceDistance.OuterHtml.ToString();

                            category = category.Split(new[] { "&nbsp;&#8226;&nbsp;" }, StringSplitOptions.None)[2].Split(new[] { "\t\t\n\t\t" }, StringSplitOptions.None)[0].Trim(' ');

                            if (category.Contains("Grupa"))
                            {
                                category = category.Replace("Grupa ", "");
                                race.RaceCategory = category; // category data
                            }
                            else if (doc.DocumentNode.OuterHtml.ToString().Contains("sulki"))
                            {
                                race.RaceCategory = "sulki";
                            }
                            else if (doc.DocumentNode.OuterHtml.ToString().Contains("gonitwa z płotami"))
                            {
                                race.RaceCategory = "płoty";
                            }
                            else if (doc.DocumentNode.OuterHtml.ToString().Contains("z przeszkodami"))
                            {
                                race.RaceCategory = "steeple";
                            }
                            else
                            {
                                race.RaceCategory = category;
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    //race date
                    HtmlNode raceDate = doc.DocumentNode.SelectSingleNode("/html/body/main/section/div[1]/h3");

                    string date = raceDate.OuterHtml.ToString();

                    if (date.Contains("Wynik gonitwy"))
                    {
                        try
                        {
                            date = date.Split('-').Last().Split('<')[0].Trim(' ');

                            string day = date.Split(' ')[1];
                            string month = date.Split(' ')[2];
                            string year = date.Split(' ')[3];

                            //parse date
                            int dayN;
                            int yearN;
                            parseTest = int.TryParse(day, out n);
                            if (parseTest)
                            {
                                dayN = int.Parse(day);// date data
                            }
                            else
                            {
                                dayN = 1;// date data
                            }

                            int monthN = monthDict[month];

                            parseTest = int.TryParse(year, out n);
                            if (parseTest)
                            {
                                yearN = int.Parse(year);// date data
                            }
                            else
                            {
                                yearN = 2018;// date data
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.Append(dayN);
                            sb.Append(".");
                            sb.Append(monthN);
                            sb.Append(".");
                            sb.Append(yearN);

                            string fullDate = sb.ToString();

                            parseTest = DateTime.TryParse(fullDate, out t);
                            if (parseTest)
                            {
                                race.RaceDate = DateTime.Parse(fullDate); //date data
                            }
                            else
                            {
                                race.RaceDate = DateTime.Now; //date data
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    race.RaceLink = link; // link data

                    //horses
                    HtmlNode[] raceHorsesRows = doc.DocumentNode.SelectNodes("/html/body/main/section/div[4]/table/tbody/tr").ToArray();
                    race.HorseList = new List<HorseDataWrapper>();
                    List<HorseDataWrapper> horseList = new List<HorseDataWrapper>();
                    foreach (var node in raceHorsesRows)
                    {
                        HorseDataWrapper horse = new HorseDataWrapper();
                        string horseRow = node.OuterHtml.ToString();

                        if (horseRow.Contains("zł"))
                        {
                            try
                            {
                                string horsesName = node.OuterHtml.ToString();
                                horsesName = horseRow.Split('>')[7].Split('<')[0].Trim(' ');

                                horsesName = MakeTitleCase(horsesName);

                                horse.HorseName = horsesName; //horse name

                                string horsesAge = node.OuterHtml.ToString();
                                horsesAge = horseRow.Split('>')[10].Split('<')[0].Trim(' ');
                                parseTest = int.TryParse(horsesAge, out n);
                                if (parseTest)
                                {
                                    horse.Age = int.Parse(horsesAge) + (DateTime.Now.Year - race.RaceDate.Year);// horse age
                                }
                                else
                                {
                                    horse.Age = 99;
                                }

                                string racersName = node.OuterHtml.ToString();
                                racersName = racersName.Split('>')[13].Split('<')[0].Trim(' ');
                                string toReplace = racersName.Split(' ')[0];
                                racersName = racersName.Replace(toReplace, "").Trim(' ');
                                if (racersName.Contains("dż. ")) racersName = racersName.Replace("dż. ", "");
                                if (racersName.Contains("u. ")) racersName = racersName.Replace("u. ", "");

                                racersName = MakeTitleCase(racersName);

                                horse.Jockey = racersName;

                                horseList.Add(horse);
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }

                    race.HorseList = horseList; //list of horses in the race
                }
            });

            return race;
        }

        /////////////////////////////////////OK///////////////////////////////////////

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

        private string MakeTitleCase(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                TextInfo myCI = new CultureInfo("en-US", false).TextInfo; //creates CI
                name = name.ToLower().Trim(' '); //takes to lower, to take to TC later
                name = myCI.ToTitleCase(name); //takes to TC
            }

            return name;
        }
    }
}