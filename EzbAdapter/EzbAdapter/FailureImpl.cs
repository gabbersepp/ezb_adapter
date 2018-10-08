using System;
using EzbAdapter.Contracts;

namespace EzbAdapter
{
    public class FailureImpl : ICurrencyConverter
    {
        private readonly ConverterState state;

        public FailureImpl(ConverterState state)
        {
            this.state = state;
        }

        public double GetEuroFrom(Currency currency, double foreignValue, DateTime day)
        {
            throw new NotImplementedException();
        }

        public double GetEuroFxFrom(Currency currency, DateTime day)
        {
            throw new NotImplementedException();
        }

        public ConverterState State => state;
    }
}