using System;
using System.Collections.Generic;
using System.Linq;
using EzbAdapter.Contracts;

namespace EzbAdapter
{
    public sealed class CurrencyConverterImpl : ICurrencyConverter
    {
        private readonly int maxGap;

        public CurrencyConverterImpl(List<ExchangeRateBundle> bundles, int maxGap)
        {
            this.maxGap = maxGap;
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
                    return new { Rate = x, Gap = 0.0 };
                }

                if (x.Date > day)
                {
                    // first rate after wanted date
                    return new { Rate = x, Gap = Math.Abs((x.Date-day).TotalDays) };
                }

                return new { Rate = (ExchangeRate)null, Gap = 0.0 };
            }).Where(x => x.Gap <= maxGap).OrderBy(x => x.Gap).FirstOrDefault(x => x.Rate != null);

            if (firstPossibleDate == null)
            {
                throw new DateOutsideRangeException(day);
            }

            return firstPossibleDate.Rate.Rate;
        }

        public ConverterState State => ConverterState.Success;
    }
}
