using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Services.Dictionary
{
    public class DictionariesService : IDictionariesService
    {
        public Dictionary<string, string> GetJockeyPlNodeDictionary()
        {
            Dictionary<string, string> nodeDictionary = new Dictionary<string, string>();

            nodeDictionary.Add("Name", "/html/body/main/section[1]/div[1]/h3");
            nodeDictionary.Add("AllRaces", "//*[@id=\"wykaz_list\"]/tbody/tr");

            return nodeDictionary;
        }

        public Dictionary<string, string> GetJockeyCzNodeDictionary()
        {
            Dictionary<string, string> nodeDictionary = new Dictionary<string, string>();

            nodeDictionary.Add("Name", "/html[1]/body[1]/div[1]/form[1]/div[1]");
            nodeDictionary.Add("Time", "/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[1]/table[1]");
            nodeDictionary.Add("AllRaces", "/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[4]/tr");

            return nodeDictionary;
        }

        public Dictionary<string, string> GetHorsePlNodeDictionary()
        {
            Dictionary<string, string> nodeDictionary = new Dictionary<string, string>();

            nodeDictionary.Add("Name", "/html/body/main/section[1]/div/div[2]/div[2]/header/div/h1");
            nodeDictionary.Add("Age", "/html/body/main/section[1]/div/div[2]/div[2]/div/table/tbody/tr[1]/th/strong");
            nodeDictionary.Add("Father", "/html/body/main/section[1]/div/div[2]/div[2]/div/table/tbody/tr[4]/td");
            nodeDictionary.Add("FatherLink", "/html/body/main/section[1]/div/div[2]/div[2]/div/table/tbody/tr[4]/td");
            nodeDictionary.Add("AllChildren", "//*[@id=\"wykaz\"]/tbody/tr");
            nodeDictionary.Add("AllRaces", "//*[@id=\"wykaz\"]/tbody/tr");

            return nodeDictionary;
        }

        public Dictionary<string, string> GetHorseCzNodeDictionary()
        {
            Dictionary<string, string> nodeDictionary = new Dictionary<string, string>();

            nodeDictionary.Add("Name", "b");
            nodeDictionary.Add("Age", "b");
            nodeDictionary.Add("Father", "/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[2]/tr[1]");
            nodeDictionary.Add("FatherLink", "/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[2]/tr[1]");
            nodeDictionary.Add("AllChildren", "/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[3]/tr");
            nodeDictionary.Add("Allraces", "/html[1]/body[1]/div[1]/form[1]/div[1]/table[1]/tr[1]/td[2]/table[4]/tr");

            return nodeDictionary;
        }

        public Dictionary<string, string> GetRacePlNodeDictionary()
        {
            Dictionary<string, string> nodeDictionary = new Dictionary<string, string>();

            nodeDictionary.Add("RaceDistance", "/html/body/main/section");
            nodeDictionary.Add("RaceCategory", "/html/body/main/section/div[3]/table/tbody/tr[2]/td");
            nodeDictionary.Add("RaceDate", "/html/body/main/section/div[1]/h3");
            nodeDictionary.Add("HorseList", "/html/body/main/section/div[4]/table/tbody/tr");

            return nodeDictionary;
        }

        public Dictionary<string, int> GetMonthDictionary()
        {
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

            return monthDict;
        }

        public Dictionary<string, int> GetRaceCategoryDictionary(IRaceProvider raceModelProvider)
        {
            Dictionary<string, int> categoryFactorDict = new Dictionary<string, int>();

            categoryFactorDict.Add("G1 A", 11);
            categoryFactorDict.Add("G3 A", 10);
            categoryFactorDict.Add("LR A", 9);
            categoryFactorDict.Add("LR B", 8);
            categoryFactorDict.Add("L A", 7);
            categoryFactorDict.Add("B", 5);
            categoryFactorDict.Add("A", 4);
            categoryFactorDict.Add("Gd 3", 11);
            categoryFactorDict.Add("Gd 1", 10);
            categoryFactorDict.Add("L", 8);
            categoryFactorDict.Add("F", 7);
            categoryFactorDict.Add("C", 6);
            categoryFactorDict.Add("D", 5);
            categoryFactorDict.Add("I", 4);
            categoryFactorDict.Add("II", 3);
            categoryFactorDict.Add("III", 2);
            categoryFactorDict.Add("IV", 1);
            categoryFactorDict.Add("V", 1);
            if (raceModelProvider.Category == "sulki" || raceModelProvider.Category == "kłusaki")
            {
                categoryFactorDict.Add("sulki", 9);
                categoryFactorDict.Add("kłusaki", 9);
            }
            else
            {
                categoryFactorDict.Add("sulki", 2);
                categoryFactorDict.Add("kłusaki", 2);
            }
            if (raceModelProvider.Category == "steeple" || raceModelProvider.Category == "płoty")
            {
                categoryFactorDict.Add("steeple", 9);
                categoryFactorDict.Add("płoty", 9);
            }
            else
            {
                categoryFactorDict.Add("steeple", 2);
                categoryFactorDict.Add("płoty", 2);
            }
            categoryFactorDict.Add("-", 6);
            categoryFactorDict.Add(" ", 6);
            categoryFactorDict.Add("", 6);

            return categoryFactorDict;
        }

    }
}
