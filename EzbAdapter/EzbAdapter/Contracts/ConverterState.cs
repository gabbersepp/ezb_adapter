namespace EzbAdapter.Contracts
{
    public enum ConverterState
    {
        RestTimeout, Rest500, RestFatal, RestOther, Success,
        ParseFailure, EcbWrongCurrencyCount,
        EcbTooFewResults
    }
}