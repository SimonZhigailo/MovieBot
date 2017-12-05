using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using Newtonsoft.Json;
using MoviesParserr.Model;

namespace MoviesParserr.Extensions
{
    public static class Extensions
    {
        /*1054079301182-v4c3qn4mb8f6673651kiinnlsc979gp1.apps.googleusercontent.com*/
        /*63Rvr3uCmZUzNWhYyKwfVWyo*/

        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Google-Sheets-API-Quickstart";
        static int tryCount;
        public static void SaveToGoogleDrive(string fullName, string name, int seriaCount)
        {
            UserCredential credential;

            using (var stream =
                new FileStream(Directory.GetFiles(@"C:\MoviesParser")[0], FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });



            String spreadsheetId = "1SRMlO18VipcSsekYMlsP2UMSKaNX7GOEXHjIulEeEsw";
            int rowNum = 6080 + seriaCount;
            String range = "B" + rowNum + ":D" + rowNum;
            //String rangeGet = "A1:E1";

            IList<object> valueToWrite = new List<object>()
        {
            fullName,
            name,
            "Жигайло"
        };


            ValueRange requestBody = new ValueRange()
            {
                MajorDimension = "ROWS",
                Range = range,
                Values = new List<IList<object>>() { valueToWrite }
            };

            //SpreadsheetsResource.ValuesResource.GetRequest requestGet = service.Spreadsheets.Values.Get(spreadsheetId, rangeGet);
            //ValueRange response = requestGet.Execute();
            //IList<IList<Object>> values = response.Values;

            BatchUpdateValuesRequest batchUpdate = new BatchUpdateValuesRequest();
            batchUpdate.Data = new List<ValueRange>() { requestBody };
            batchUpdate.ValueInputOption = "RAW";

            SpreadsheetsResource.ValuesResource.BatchUpdateRequest request =
                    service.Spreadsheets.Values.BatchUpdate(batchUpdate, spreadsheetId);

            request.Execute();
        }

        public static List<int> GetEmptyFieldsInGoogleTab(int bRange, int dRange)
        {

            UserCredential credential;

            using (var stream =
                new FileStream(Directory.GetFiles(@"C:\MoviesParser")[0], FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });



            String spreadsheetId = "1SRMlO18VipcSsekYMlsP2UMSKaNX7GOEXHjIulEeEsw";
            String range = "B" + bRange + ":D" + dRange;
            SpreadsheetsResource.ValuesResource.GetRequest requestGet = service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = requestGet.Execute();
            IList<IList<Object>> values = response.Values;

            var a = 421;

            List<int> ids = new List<int>();

            for (int i = 0; i < values.Count; i++)
            {
                a++;
                if (values[i].Count == 0)
                {
                    ids.Add(a);
                }
            }
            return ids;
        }

        public static void WriteToJournal(string info, int seriaNum)
        {
            System.IO.File.WriteAllText(@"C:\MoviesParser\DebugLog.txt", System.Environment.NewLine + "[" + seriaNum + "]" + "Серия: " + Parser.lastSeria + System.Environment.NewLine + "Инфо: " + info);

        }

        public static void SubmitMovie(MovieInfo movie)
        {
            PushAddButton();

            var editWindow = Parser.driver.FindElement(By.Id("ext-comp-1053"));

            while (!editWindow.Displayed || editWindow == null)
            {
                editWindow = Parser.driver.FindElement(By.Id("ext-comp-1053"));
                PushAddButton();
            }


            EnterMovieName();

            Parser.driver.FindElement(By.Id("ext-comp-1033")).Click();

            Thread.Sleep(2000);

            SelectGenre("Сериал");


            Parser.driver.FindElement(By.Id("movie_name")).Clear();

            Parser.driver.FindElement(By.Id("movie_name")).SendKeys(movie.fullName);


            Parser.driver.FindElement(By.Id("ext-gen165")).Click();//клик на тарифы

            IWebElement tableElement = Parser.driver.FindElement(By.Id("tariff"));

            SelectTarifs(tableElement);

            Save();

            Parser.driver.Navigate().GoToUrl("http://stavideo:mediaadmin@37.208.111.236:88/video/vod/");


            Thread.Sleep(3000);

            SearchMovie(movie.fullName);

            Thread.Sleep(3000);

            Parser.driver.FindElement(By.Id("ext-gen167")).Click();//клик на сервисы

            Thread.Sleep(1000);

            var serv = Parser.driver.FindElement(By.Id("serv"));
            serv.FindElements(By.ClassName("x-btn-mc")).First(i => i.Text.Equals("Добавить")).Click();
            Thread.Sleep(1000);
            Parser.driver.FindElement(By.Id("ext-comp-1002")).Clear();
            Parser.driver.FindElement(By.Id("ext-comp-1002")).SendKeys("vod_GB");

            Parser.driver.FindElement(By.Id("ext-comp-1005")).SendKeys(movie.ServerPath);

            Parser.driver.FindElement(By.Id("ext-gen359")).Click();//клик сохранить настройки сервиса

            Thread.Sleep(1000);

            Save();

            SaveToGoogleDrive(movie.NameForExcel, movie.fullName, movie.Id);

            Thread.Sleep(2000);
        }


        public static void getPage(int num = 0)
        {
            try
            {
                int a;
                IWebElement input;
                do
                {
                    input = Parser.driver.FindElement(By.Id("ext-comp-1012"));
                    input.Clear();
                    a = Convert.ToInt32(Regex.Match(Parser.driver.FindElement(By.Id("ext-gen44")).Text, @"\d+").Value);

                } while (a == 1);

                if (num != 0)
                {
                    input.SendKeys(num.ToString());
                    input.SendKeys(Keys.Return);
                }

                else
                {
                    input.SendKeys(a.ToString());
                    input.SendKeys(Keys.Return);
                }

                do
                {
                    input = Parser.driver.FindElement(By.Id("ext-gen43"));
                    a = Convert.ToInt32(Regex.Match(Parser.driver.FindElement(By.Id("ext-gen44")).Text, @"\d+").Value);

                } while (a == 1);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static void SelectGenre(string type)
        {
            IList<IWebElement> comboItems = Parser.driver.FindElements(By.CssSelector(".x-combo-list-item"));
            foreach (var item in comboItems)
            {
                if (item.Text.Trim().Equals(type))
                {
                    item.Click();
                }
            }
        }

        public static void SelectTarifs(IWebElement tableElement)
        {
            IList<IWebElement> tableRow = tableElement.FindElements(By.TagName("tr"));
            foreach (IWebElement row in tableRow)
            {
                if (row.Text.Trim().Equals("Базовый") || row.Text.Trim().Equals("Видеотека") || row.Text.Trim().Equals("Федеральный"))
                {
                    IList<IWebElement> tds = row.FindElements(By.XPath(".//*"));
                    tds[1].Click();
                }
            }
        }

        public static bool OpenTabMovieName(string movieName)
        {
            var roo1t = Parser.wb.Until(ExpectedConditions.ElementExists(By.Id("ext-gen26"))).FindElements(By.XPath(".//*"));
            foreach (var row in roo1t)
            {
                string[] name = row.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                if (name[0].Equals(movieName))
                {
                    roo1t[0].Click();
                    roo1t[0].SendKeys(Keys.End);//в самый низ
                    Thread.Sleep(2000);
                    row.Click();
                    PushEditButton();
                    return true;
                }
            }
            return false;
        }

        public static void SearchMovie(string movieName)
        {
            getPage();
            Parser.wb.Until(d => d.FindElement(By.Id("ext-gen26")).FindElements(By.XPath(".//*"))[0].Text != string.Empty);
            Thread.Sleep(1000);
            bool res = OpenTabMovieName(movieName);


            while (!res)
            {
                Parser.driver.FindElement(By.Id("ext-gen39")).Click();
                Thread.Sleep(2000);
                res = OpenTabMovieName(movieName);
            }
        }

        //public static void Save()
        //{
        //    Parser.driver.FindElement(By.Id("ext-gen144")).Click();
        //}

        public static void Save()
        {
            Parser.driver.FindElements(By.ClassName("x-btn-mc")).First(i => i.Text.Equals("Сохранить")).Click();
        }


        public static void PushAddButton()
        {

            Parser.driver.FindElements(By.ClassName("x-btn-mc")).First(i => i.Text.Equals("Добавить")).Click();

            Thread.Sleep(500);



        }

        public static void PushEditButton()
        {
            IWebElement element = null;
            Parser.driver.FindElements(By.ClassName("x-btn-mc")).First(i => i.Text.Equals("Редактировать")).Click();

            Thread.Sleep(1000);

            if (TryFindElement(By.Id("ext-comp-1053"), out element))
            {
                bool visible = IsElementVisible(element);
                if (!visible)
                {
                    Thread.Sleep(3000);

                    Parser.driver.FindElements(By.ClassName("x-btn-mc")).First(i => i.Text.Equals("Редактировать")).Click();
                }
            }
        }

        public static void EnterMovieInfo()
        {
            var editWindow = Parser.driver.FindElement(By.Id("ext-comp-1053"));

            Parser.driver.FindElement(By.Id("movie_name")).SendKeys("Криминальная Россия");

            Parser.driver.FindElement(By.Id("actor")).SendKeys("Криминальная Россия");

            Parser.driver.FindElement(By.Id("regessur")).SendKeys("Криминальная Россия");

            Parser.driver.FindElement(By.Id("year")).SendKeys("Криминальная Россия");

        }

        public static void EnterMovieName()
        {

            bool repeat = true;
            while (repeat)
            {
                var flagConfirm = false;
                var flagBox = false;

                var editWindow = Parser.driver.FindElement(By.Id("ext-comp-1053"));

                Thread.Sleep(2000);

                Parser.driver.FindElement(By.Id("movie_name")).SendKeys("Ментов");

                Thread.Sleep(1000);

                Parser.wb.Until(ExpectedConditions.ElementIsVisible(By.Id("movie_name"))).SendKeys("ские");

                Thread.Sleep(6000);

                try
                {
                    //var innerbox = Parser.driver.FindElement(By.Id("ext-gen393")).FindElements(By.XPath(".//*")); //опасно, хз как найти
                    var a = Parser.driver.FindElements(By.ClassName("search-item"));
                    if (a[0].Text.Equals("Ментовские войны (2004) Павел Мальков"))
                    {
                        flagBox = true;
                    }

                }
                catch (Exception ex)
                {
                }
                try
                {
                    var confirmWindow = Parser.driver.FindElements(By.ClassName("x-window-header-text")).First(i => i.Text.Equals("Импорт данных с GetMovie"));
                    flagConfirm = confirmWindow == null;
                }
                catch (Exception ex)
                {
                    flagConfirm = true;
                }

                if (flagConfirm && flagBox)
                {
                    editWindow.SendKeys(Keys.Return);

                    Thread.Sleep(1000);

                    editWindow.SendKeys(Keys.Return);
                    repeat = false;
                    break;
                }

                else
                {
                    Parser.driver.FindElement(By.Id("movie_name")).Clear();
                }


            }
        }
        public static bool TryFindElement(By by, out IWebElement element)
        {

            try
            {
                element = Parser.driver.FindElement(by);
            }
            catch (NoSuchElementException ex)
            {
                element = null;
                return false;
            }
            return true;
        }

        public static bool IsElementVisible(IWebElement element)
        {
            return element.Displayed && element.Enabled;
        }
    }
}
