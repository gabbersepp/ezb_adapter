using System;
using System.Collections.Generic;
using EzbAdapter.Contracts;

namespace EzbAdapter
{
    public interface IClient
    {
        ICurrencyConverter BuildForDate(DateTime start, DateTime end, List<Currency> currencies);
        ICurrencyConverter BuildFromEzbFile(string ezbFile);
    }
}