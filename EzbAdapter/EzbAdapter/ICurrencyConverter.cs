using EzbAdapter.Contracts;
using System;

namespace EzbAdapter
{
    public interface ICurrencyConverter
    {
        double GetEuroFrom(Currency currency, double foreignValue, DateTime day);
        double GetEuroFxFrom(Currency currency, DateTime day);
        ConverterState State { get; }
    }
}