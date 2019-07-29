using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;
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
        private static string url = "service/data/EXR/D.{fx}.EUR.SP00.A/ECB?detail=dataonly";

        public Client(int maxGap, List<Currency> currencies, DateTime start, DateTime end)
        {
            this.maxGap = maxGap;
            this.currencies = currencies;
            this.start = start;
            this.end = end;
        }

        public virtual RestResult GetContent()
        {
            var startString = start.AddDays(-10).ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var endString = end.AddDays(10).ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            var client = new RestClient("https://sdw-wsrest.ecb.europa.eu");

            var request = new RestRequest(url);
            request.AddQueryParameter("startPeriod", startString);
            request.AddQueryParameter("endPeriod", endString);
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
            var obj = Deserializer.Deserialize(GenerateStreamFromString(content), out var errors);

            var list = obj.DataSet.Series.Select(series =>
                {
                    var currency = series.SeriesKey.Value.First(x => x.Id == "CURRENCY").Value;
                    var curParsed = (Currency) Enum.Parse(typeof(Currency), currency);

                    return series.Obs.Select(obs => new
                        {
                            Currency = curParsed,
                            Date = DateTime.Parse(obs.ObsDimension.Value),
                            Rate = obs.ObsValue.Value
                        }
                    );
                })
                .SelectMany(x => x)
                .GroupBy(x => x.Currency)
                .Select(x => new ExchangeRateBundle
                {
                    Currency = x.Key,
                    Rates = x.Select(rate => new ExchangeRate {Date = rate.Date, Rate = rate.Rate}).ToList()
                }).ToList();

            return new CurrencyConverterImpl(list, maxGap);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public class RestResult
        {
            public string Content;
            public ConverterState State;
        }
    }
}
