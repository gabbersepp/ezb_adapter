using System.Collections.Generic;

namespace EzbAdapter.Contracts
{
    public class ExchangeRateBundle
    {
        public Currency Currency;
        public List<ExchangeRate> Rates;
    }
}