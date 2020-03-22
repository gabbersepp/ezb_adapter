Read EUR exchange rates for every date for every available currency from the public ECB API

## Usage
There are two possible use cases where you can use this library. First you can fetch the data directly from ecb. To do this, use following code:

```c#
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client(10, new List<Currency> { Currency.USD, Currency.JPY }, new DateTime(2018, 10, 1), new DateTime(2018, 10, 9));
            var result = client.BuildForDate();

            if (result.State == ConverterState.Success)
            {
                Console.WriteLine($"100$ on 5. of October are {result.GetEuroFrom(Currency.USD, 100, new DateTime(2018, 10, 5))}€ at a exchange rate of {result.GetEuroFxFrom(Currency.USD, new DateTime(2018, 10, 5))}");
            }
            else
            {
                Console.WriteLine("Error while retriving fx data: " + result.State);
            }

            Console.ReadKey();
        }
    }
```

Secondly you can pass a path to a text file that contains the content of the ecb API:

```c#
...
	client.BuildFromEzbFile("....")
...
```

## Expanding the date range
The ECB does not deliver a rate for every day. For example for Sundays no rates are delivered.
If you build the client to fetch rates from e.g. Sunday to next Monday which can be a holiday and you then try to get the rate of the first Sunday, then you will not get any rate. 
To avoid this, the date range is expanded 10 days at both sides.

## Notes
Please check the result state before using it. The ecb server is not always available.

## Links

![Travis CI](https://travis-ci.org/gabbersepp/ezb_adapter.svg?branch=master)
