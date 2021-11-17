using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Frontier.Methods
{
    class Currencies
    {
        public static async Task<Dictionary<string, decimal>> DownloadNBP()
        {
            Dictionary<string, decimal> CurrencyData = new Dictionary<string, decimal>();
            try
            {
                await Task.Run(async() =>
                {
                    string url = "https://api.nbp.pl/api/exchangerates/tables/a?format=json";
                    WebClient webClient = new WebClient { };
                    string response = webClient.DownloadString(url);
                    if (response != null)
                    {
                        JObject values = JObject.Parse(response.TrimStart('[').TrimEnd(']'));
                        for (int i = 0; i < values["rates"].Count(); i++)
                        {
                            if (values["rates"][i]["code"].ToString() == "USD" || values["rates"][i]["code"].ToString() == "EUR" || values["rates"][i]["code"].ToString() == "GBP")
                            {
                                CurrencyData.Add(values["rates"][i]["code"].ToString(), Math.Round(decimal.Parse(values["rates"][i]["mid"].ToString()), 2));
                            }
                            await Task.Delay(1);
                        }
                    }
                });
            }
            catch (Exception){ CurrencyData = null; }
            return CurrencyData;
        }
    }
}
