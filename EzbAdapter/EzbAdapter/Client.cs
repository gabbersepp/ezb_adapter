using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Common.Logging;
using EzbAdapter.Contracts;
using RestSharp;

namespace EzbAdapter
{
    public class Client : IClient
    {
        private static ILog log = LogManager.GetLogger<Client>();

        // format of date: YYY-MM-DD
        private static string url = "service/data/EXR/D.{fx}.EUR.SP00.A/ECB?startPeriod={start}&endPeriod={end}&detail=dataonly";

        public virtual string GetContent(DateTime start, DateTime end, List<Currency> currency)
        {
            var startString = start.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var endString = end.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            var client = new RestClient("https://sdw-wsrest.ecb.europa.eu");

            var request = new RestRequest(url);
            request.AddUrlSegment("start", startString);
            request.AddUrlSegment("end", endString);
            request.AddUrlSegment("fx", currency.Skip(1).Aggregate(currency[0].ToString(), (x, y) => $"{x}+{y}"));

            request.Method = Method.GET;

            var response = client.Execute(request);
            return response.Content;
        }

        public ICurrencyConverter BuildFromEzbFile(string ezbFile)
        {
            string ezbContent = File.ReadAllText(ezbFile);
            return Build(ezbContent);
        }

        public ICurrencyConverter BuildForDate(DateTime start, DateTime end, List<Currency> currencies)
        {
            log.Debug("Start ecb adapter");
            var response = GetContent(start, end, currencies);

            return Build(response);
        }

        private ICurrencyConverter Build(string content)
        {
            content = content.Replace("message:", "").Replace("generic:", "");

            var xdoc = XDocument.Parse(content);

            var list = xdoc.Descendants("GenericData")
                .First().Descendants("Series").Select(series =>
                {
                    var currency = series
                            .Descendants("SeriesKey").First()
                            .Descendants("Value")
                            .First(x => x.Attribute("id").Value == "CURRENCY").Attribute("value").Value;

                    var curParsed = (Currency)Enum.Parse(typeof(Currency), currency);

                    var bundle = new ExchangeRateBundle { Currency = curParsed };

                    var rates = series.Descendants("Obs")
                        .Select(x =>
                        {
                            return new ExchangeRate
                            {
                                Date = DateTime.Parse(x.Element("ObsDimension").Attribute("value").Value),
                                Rate = float.Parse(x.Element("ObsValue").Attribute("value").Value.Replace(".", ","))
                            };
                        }).ToList();

                    bundle.Rates = rates;

                    return bundle;
                }).ToList();

            return new CurrencyConverterImpl(list);
        }

        internal class CurrencyConverterImpl : ICurrencyConverter
        {
            public CurrencyConverterImpl(List<ExchangeRateBundle> bundles)
            {
                this.bundles = bundles;
            }

            private List<ExchangeRateBundle> bundles { get; }

            public double GetEuroFrom(Currency currency, double foreignValue, DateTime day)
            {
                var rate = GetEuroFxFrom(currency, day);

                return foreignValue / rate;
            }

            public double GetEuroFxFrom(Currency currency, DateTime day)
            {
                var rates = bundles.First(x => x.Currency == currency).Rates;
                ExchangeRate lastRate = null;

                var firstPossibleDate = rates.Select(x =>
                {
                    if (x.Date.Year == day.Year && x.Date.Month == day.Month && x.Date.Day == day.Day)
                    {
                        return new { T = true, Rate = x };
                    }

                    lastRate = x;

                    return new { T = false, Rate = (ExchangeRate)null };
                }).FirstOrDefault(x => x.T);

                return (firstPossibleDate?.Rate ?? lastRate).Rate;
            }
        }
    }
}
