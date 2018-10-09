using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Common.Logging;
using EzbAdapter.Contracts;
using RestSharp;

namespace EzbAdapter
{
    public class Client : IClient
    {
        private readonly int maxGap;
        private readonly List<Currency> currencies;
        private readonly DateTime start;
        private readonly DateTime end;
        private static ILog log = LogManager.GetLogger<Client>();

        // format of date: YYY-MM-DD
        private static string url = "service/data/EXR/D.{fx}.EUR.SP00.A/ECB?startPeriod={start}&endPeriod={end}&detail=dataonly";

        public Client(int maxGap, List<Currency> currencies, DateTime start, DateTime end)
        {
            this.maxGap = maxGap;
            this.currencies = currencies;
            this.start = start;
            this.end = end;
        }

        public virtual RestResult GetContent()
        {
            var startString = start.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var endString = end.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            var client = new RestClient("https://sdw-wsrest.ecb.europa.eu");

            var request = new RestRequest(url);
            request.AddUrlSegment("start", startString);
            request.AddUrlSegment("end", endString);
            request.AddUrlSegment("fx", currencies.Skip(1).Aggregate(currencies[0].ToString(), (x, y) => $"{x}+{y}"));

            request.Method = Method.GET;

            log.Debug($"url that will be called: {client.BuildUri(request)}");

            try
            {
                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new RestResult { Content = response.Content, State = ConverterState.Success };
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        return new RestResult { State = ConverterState.Rest500 };
                    }
                    else if (response.StatusCode == HttpStatusCode.GatewayTimeout ||
                              response.StatusCode == HttpStatusCode.RequestTimeout)
                    {
                        return new RestResult { State = ConverterState.RestTimeout };
                    }

                    return new RestResult {State = ConverterState.RestOther};
                }
            }
            catch (Exception e)
            {
                log.Error("error during fetching ecb data: " + e);
                return new RestResult {State = ConverterState.RestFatal};
            }
        }

        private ICurrencyConverter BuildFromText(string content, DateTime start, DateTime end, List<Currency> currencies)
        {
            try
            {
                var result = (CurrencyConverterImpl)Build(content);

                if (result.bundles.Count != currencies.Count)
                {
                    return new FailureImpl(ConverterState.EcbWrongCurrencyCount);
                }

                // force at least halt of the rates to be included in the result. Just a simple check
                if (result.bundles.Any(x => x.Rates.Count < ((end - start).Days / 2)))
                {
                    return new FailureImpl(ConverterState.EcbTooFewResults);
                }

                return result;
            }
            catch (Exception e)
            {
                log.Error("error during parsing of ecb result: " + e);
                return new FailureImpl(ConverterState.ParseFailure);
            }
        }

        public ICurrencyConverter BuildFromEzbFile(string ezbFile)
        {
            log.Debug($"Start ecb adapter from file: {ezbFile}");
            string ezbContent = File.ReadAllText(ezbFile);
            return BuildFromText(ezbContent, start, end, currencies);
        }

        public ICurrencyConverter BuildForDate()
        {
            log.Debug($"Start ecb adapter for: start: {start}, end: {end}, currencies: {currencies.Select(x => x.ToString()).Aggregate("", (x,y) => x + "," + y)}");
            var response = GetContent();

            if (response.State != ConverterState.Success)
            {
                return new FailureImpl(response.State);
            }

            return BuildFromText(response.Content, start, end, currencies);
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
                                Rate = float.Parse(x.Element("ObsValue").Attribute("value").Value, CultureInfo.InvariantCulture)
                            };
                        }).ToList();

                    bundle.Rates = rates;

                    return bundle;
                }).ToList();

            return new CurrencyConverterImpl(list, maxGap);
        }

        public class RestResult
        {
            public string Content;
            public ConverterState State;
        }
    }
}
