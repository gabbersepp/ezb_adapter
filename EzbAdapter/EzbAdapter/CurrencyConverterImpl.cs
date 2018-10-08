using System;
using System.Collections.Generic;
using System.Linq;
using EzbAdapter.Contracts;

namespace EzbAdapter
{
    public sealed class CurrencyConverterImpl : ICurrencyConverter
    {
        public CurrencyConverterImpl(List<ExchangeRateBundle> bundles)
        {
            this.bundles = bundles;
        }

        internal List<ExchangeRateBundle> bundles { get; }

        public double GetEuroFrom(Currency currency, double foreignValue, DateTime day)
        {
            var rate = GetEuroFxFrom(currency, day);

            return foreignValue / rate;
        }

        public double GetEuroFxFrom(Currency currency, DateTime day)
        {
            var rates = bundles.First(x => x.Currency == currency).Rates;

            var firstPossibleDate = rates.Select(x =>
            {
                if (x.Date.Year == day.Year && x.Date.Month == day.Month && x.Date.Day == day.Day)
                {
                    return new { T = true, Rate = x };
                }

                if (x.Date > day)
                {
                    // first rate after wanted date
                    return new { T = true, Rate = x };
                }

                return new { T = false, Rate = (ExchangeRate)null };
            }).FirstOrDefault(x => x.T);

            return firstPossibleDate.Rate.Rate;
        }

        public ConverterState State => ConverterState.Success;
    }
}
