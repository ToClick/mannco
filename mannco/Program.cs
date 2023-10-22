using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace mannco
{
    internal class Program
    {
        static void Main(string[] args)
        {
            float steam = getSteamPrice();
            float garant = getGarantPrice();
            float market = getMarketPrice(5);
            float garantCourse = calcCrossCourse(steam, garant);
            float marketCourse = calcCrossCourse(steam, market);


            Console.WriteLine("\t| -STEAM- PRICE: {0}€", steam);
            Console.WriteLine("\t| GARANT COURSE: {0} ({1})", garantCourse, garant);
            Console.WriteLine("\t| MARKET COURSE: {0} ({1})", marketCourse, market);


            Console.ReadLine();
        }


        static float getSteamPrice()
        {
            var uri = "https://steamcommunity.com/market/itemordershistogram?country=DE&language=russian&currency=3&item_nameid=1&two_factor=0";
            var json = new WebClient().DownloadString(uri);
            var item = new JavaScriptSerializer().Deserialize<SteamItem>(json);

            return (float)int.Parse(item.lowest_sell_order) / 100;
        }

        static float getGarantPrice(float tax = 0)
        {
            var html = new WebClient().DownloadString("https://garantkeytf2.ru/sell");
            var match = Regex.Match(html, @"(\d+)\sRUB");
            var value = 0;

            if (match.Success)
            {
                var t1 = match.Groups[0].Value;
                var t2 = match.Groups[1].Value;
                value = int.Parse(match.Groups[1].Value);

                return calcPercentOf(100 - tax, value);
            }

            return 0;
        }

        static float getMarketPrice(float tax = 0)
        {

            string html = new WebClient().DownloadString("https://tf2.tm/?search=%D0%9A%D0%BB%D1%8E%D1%87%20%D0%BE%D1%82%20%D1%8F%D1%89%D0%B8%D0%BA%D0%B0%20%D0%9C%D0%B0%D0%BD%D0%BD%20%D0%9A%D0%BE");
            var matches = Regex.Matches(html, @"Mann%20Co.%20Supply%20Crate%20Key[^>]+>[^>]+>[^>]+>([\d\.]+)", RegexOptions.Multiline);
            float lower = 0;

            foreach (Match match in matches)
            {
                var price = float.Parse(match.Groups[1].Value.Replace(".", ","));

                if (lower == 0 || price < lower)
                {
                    lower = price;
                }
            }

            return calcPercentOf(100 - tax, lower);
        }


        static float calcCrossCourse(float itemEuroBuy, float itemRubSell)
        {
            float percent = calcPercent(1, 0, itemEuroBuy);
            return calcPercentOf(percent, itemRubSell);
        }

        static float calcPercent(float Input, float Min, float Max)
        {
            return ((Input - Min) * 100) / (Max - Min);
        }

        static float calcPercentOf(float Input, float Of)
        {
            return (Input / 100) * Of;
        }
    }
}
