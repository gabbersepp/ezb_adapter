namespace EzbAdapter
{
    public interface IClient
    {
        ICurrencyConverter BuildForDate();
        ICurrencyConverter BuildFromEzbFile(string ezbFile);
    }
}