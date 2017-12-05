using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MoviesParserr.Model;
using System.Threading;
using OpenQA.Selenium.Interactions;
using System.Linq;

namespace MoviesParserr
{
    static class Parser
    {
        public static Thread thread;

        public static IWebDriver driver;

        public static WebDriverWait wb;

        static string[] lines = System.IO.File.ReadAllLines(@"C:\MoviesParser\vod_encode.txt");

        public static List<string> resultLines = new List<string>();


        static string pattern = "Mentovskie_vojny";//паттерн для опеределения в списке залитых фильмов(часть имени файла)

        static string nameOfSerial = "Криминальная Россия";//

        public static string lastSeria = System.IO.File.ReadAllText(@"C:\MoviesParser\DebugLog.txt");//последняя серия с которой работал парсер


        public static Regex IdMatch = new Regex(@"^[^\d]*(\d+)");

        public static Regex seasonNumMatch = new Regex(@"(S\d+E\d+)");

        //public static Regex NameMatch = new Regex(@"/(.*?)("); 

        static void Parse()
        {



            List<Tuple<int, List<MovieInfo>>> list = GetMovies();

            var opt = new FirefoxOptions
            {
                BrowserExecutableLocation = @"c:\program files\mozilla firefox\firefox.exe"
            };

            driver = new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), opt, new TimeSpan(0, 1, 0));

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            wb = new WebDriverWait(driver, new TimeSpan(10000000))/*.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"))*/;
            wb.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            driver.Navigate().GoToUrl("http://stavideo:mediaadmin@37.208.111.236:88/video/vod/");

            Extensions.Extensions.getPage();

            bool flag = false;

            if (String.IsNullOrEmpty(lastSeria))
            {
                lastSeria = list[0].Item2[0].fullName;
                flag = true;
            }

            foreach (var season in list)
            {
                for (int i = 0; i < season.Item2.Count; i++)
                {
                    if (flag)
                    {
                        Extensions.Extensions.SubmitMovie(season.Item2[i]);
                        System.IO.File.WriteAllText(@"C:\MoviesParser\DebugLog.txt", season.Item2[i].fullName);
                    }
                    if (season.Item2[i].fullName.Equals(lastSeria))
                    {
                        flag = true;
                    }
                }
            }

        }

        static List<Tuple<int, List<MovieInfo>>> GetMovies()
        {
            List<MovieInfo> moviesInfo = new List<MovieInfo>();

            List<int> ids = Extensions.Extensions.GetEmptyFieldsInGoogleTab(6502, 6730);

            bool flag = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 3500)
                {

                    int id = Convert.ToInt32(IdMatch.Match(lines[i]).Groups[1].Value);
                    if (ids.Contains(id))
                    {
                        flag = true;
                        resultLines.Add(lines[i]);
                    }
                    if (flag && id > ids[ids.Count - 1] && id != 495)
                    {
                        resultLines.Add(lines[i]);
                    }
                }

            }

            //foreach (string x in lines)
            //{
            //    if (x.Contains(pattern))
            //    {
            //        resultLines.Add(x);
            //    }

            //}
            foreach (string x in resultLines)
            {

                Match idMatch = IdMatch.Match(x);
                int[] numbers = Regex.Matches(seasonNumMatch.Match(x).Groups[0].Value, "(-?[0-9]+)").OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();
                string[] forResult = x.Split(new string[] { "-->" }, StringSplitOptions.None);
                Match nameMatch = new Regex(@"(?<=/in/)(.*)(?=.avi)").Match(forResult[0]);
                MovieInfo info = new MovieInfo(x, Convert.ToInt32(idMatch.Groups[1].Value), nameOfSerial, numbers[0], numbers[1], "/hlsvodnew/" + forResult[1].Trim() + "/playlist.m3u8", nameMatch.Groups[1].Value);
                moviesInfo.Add(info);
            }

            var list = new List<Tuple<int, List<MovieInfo>>>();
            //moviesInfo.Sort((x, y) =>
            //    x.Season.CompareTo(y.Season));
            var maxSeason = moviesInfo.OrderByDescending(item => item.Season).First().Season;

            for (int i = 1; i < maxSeason; i++)
            {
                var tempList = moviesInfo.Where(a => a.Season == i).ToList();
                tempList.Sort((x, y) => x.Episode.CompareTo(y.Episode));
                //var count = 1;
                //for (int j = 0; j < tempList.Count; j++)
                //{
                //    tempList[j].Episode = count;
                //    count++;
                //}

                list.Add(new Tuple<int, List<MovieInfo>>(i, tempList));
            }

            return list;
        }

        static void Main(string[] args)
        {

            thread = new Thread(new ThreadStart(Parse));
            thread.Start();
        }
    }
}