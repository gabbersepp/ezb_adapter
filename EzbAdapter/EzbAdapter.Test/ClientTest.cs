using System;
using System.Collections.Generic;
using EzbAdapter.Contracts;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace EzbAdapter.Test
{
    public class ClientTest
    {
        /*
         * The maximal gap between two dates must be configured. A day outside of this allowed gap is an error
         */
        [Test]
        public void HandleRequestedDateOutsideRange()
        {
            var bundle = new ExchangeRateBundle { Currency = Currency.USD };
            var rates = new List<ExchangeRate>();
            rates.Add(new ExchangeRate { Date = new DateTime(2018, 01, 01), Rate = 1.5f });
            bundle.Rates = rates;

            var converter = new CurrencyConverterImpl(new List<ExchangeRateBundle> { bundle }, 10);

            // use date outside of range
            Assert.Throws<DateOutsideRangeException>(() => converter.GetEuroFxFrom(Currency.USD, new DateTime(2018, 01, 10)));
            Assert.Throws<DateOutsideRangeException>(() => converter.GetEuroFxFrom(Currency.USD, new DateTime(2017, 12, 10)));
        }

        [Test]
        public void UseNextPossibleRate()
        {
            var bundle = new ExchangeRateBundle { Currency = Currency.USD };
            var rates = new List<ExchangeRate>();
            rates.Add(new ExchangeRate { Date = new DateTime(2018, 01, 01), Rate = 1.5f });
            rates.Add(new ExchangeRate { Date = new DateTime(2018, 01, 03), Rate = 2f });
            rates.Add(new ExchangeRate { Date = new DateTime(2018, 01, 04), Rate = 3f });
            bundle.Rates = rates;

            var converter = new CurrencyConverterImpl(new List<ExchangeRateBundle> { bundle }, 1);
            var fx = converter.GetEuroFxFrom(Currency.USD, new DateTime(2018, 01, 02));
            fx.Should().Be(2f);
        }

        [Test]
        public void TestParsing()
        {
            var partialSubstitute = Substitute.ForPartsOf<Client>(0, new List<Currency> { Currency.GBP, Currency.JPY }, default(DateTime), default(DateTime));
            partialSubstitute.When(x => x.GetContent()).DoNotCallBase();

            var content = @"<?xml version=""1.0"" encoding=""UTF-8""?><message:GenericData xmlns:message=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message"" xmlns:common=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/common"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:generic=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic"" xsi:schemaLocation=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message https://sdw-wsrest.ecb.europa.eu:443/vocabulary/sdmx/2_1/SDMXMessage.xsd http://www.sdmx.org/resources/sdmxml/schemas/v2_1/common https://sdw-wsrest.ecb.europa.eu:443/vocabulary/sdmx/2_1/SDMXCommon.xsd http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic https://sdw-wsrest.ecb.europa.eu:443/vocabulary/sdmx/2_1/SDMXDataGeneric.xsd"">
<message:Header>
<message:ID>615a7764-34d9-4e27-9901-b983fd2da9c0</message:ID>
<message:Test>false</message:Test>
<message:Prepared>2018-08-11T10:55:40.970+02:00</message:Prepared>
<message:Sender id=""ECB""/>
<message:Structure structureID=""ECB_EXR1"" dimensionAtObservation=""TIME_PERIOD"">
<common:Structure>
<URN>urn:sdmx:org.sdmx.infomodel.datastructure.DataStructure=ECB:ECB_EXR1(1.0)</URN>
</common:Structure>
</message:Structure>
</message:Header>
<message:DataSet action=""Replace"" validFromDate=""2018-08-11T10:55:40.970+02:00"" structureRef=""ECB_EXR1"">
<generic:Series>
<generic:SeriesKey>
<generic:Value id=""FREQ"" value=""D""/>
<generic:Value id=""CURRENCY"" value=""JPY""/>
<generic:Value id=""CURRENCY_DENOM"" value=""EUR""/>
<generic:Value id=""EXR_TYPE"" value=""SP00""/>
<generic:Value id=""EXR_SUFFIX"" value=""A""/>
</generic:SeriesKey>
<generic:Obs>
<generic:ObsDimension value=""2017-01-02""/>
<generic:ObsValue value=""122.92""/>
</generic:Obs>
<generic:Obs>
<generic:ObsDimension value=""2017-01-03""/>
<generic:ObsValue value=""122.75""/>
</generic:Obs>
<generic:Obs>
<generic:ObsDimension value=""2017-01-04""/>
<generic:ObsValue value=""122.64""/>
</generic:Obs>
</generic:Series>
<generic:Series>
<generic:SeriesKey>
<generic:Value id=""FREQ"" value=""D""/>
<generic:Value id=""CURRENCY"" value=""USD""/>
<generic:Value id=""CURRENCY_DENOM"" value=""EUR""/>
<generic:Value id=""EXR_TYPE"" value=""SP00""/>
<generic:Value id=""EXR_SUFFIX"" value=""A""/>
</generic:SeriesKey>
<generic:Obs>
<generic:ObsDimension value=""2017-01-02""/>
<generic:ObsValue value=""1.0465""/>
</generic:Obs>
<generic:Obs>
<generic:ObsDimension value=""2017-01-03""/>
<generic:ObsValue value=""1.0385""/>
</generic:Obs>
</generic:Series>
</message:DataSet>
</message:GenericData>
            ";

            partialSubstitute.GetContent().ReturnsForAnyArgs(new Client.RestResult { Content = content, State = ConverterState.Success });

            var result = partialSubstitute.BuildForDate();

            result.GetEuroFrom(Currency.USD, 30, new DateTime(2017, 1, 2)).Should().BeLessThan(28.667f);
            result.GetEuroFrom(Currency.USD, 30, new DateTime(2017, 1, 2)).Should().BeGreaterThan(28.666f);

            result.GetEuroFrom(Currency.USD, 30, new DateTime(2017, 1, 3)).Should().BeLessThan(28.888f);
            result.GetEuroFrom(Currency.USD, 30, new DateTime(2017, 1, 3)).Should().BeGreaterThan(28.887f);

            result.GetEuroFrom(Currency.JPY, 3000, new DateTime(2017, 1, 2)).Should().BeLessThan(24.407f);
            result.GetEuroFrom(Currency.JPY, 3000, new DateTime(2017, 1, 2)).Should().BeGreaterThan(24.406f);

            result.GetEuroFrom(Currency.JPY, 3000, new DateTime(2017, 1, 3)).Should().BeLessThan(24.44f);
            result.GetEuroFrom(Currency.JPY, 3000, new DateTime(2017, 1, 3)).Should().BeGreaterThan(24.4399f);

            result.GetEuroFrom(Currency.JPY, 3000, new DateTime(2017, 1, 4)).Should().BeLessThan(24.462f);
            result.GetEuroFrom(Currency.JPY, 3000, new DateTime(2017, 1, 4)).Should().BeGreaterThan(24.461f);

        }

        [Test]
        public void TestParsing2()
        {
            var partialSubstitute = Substitute.ForPartsOf<Client>(0, new List<Currency> { Currency.GBP, Currency.JPY }, default(DateTime), default(DateTime));
            partialSubstitute.When(x => x.GetContent()).DoNotCallBase();

            var content = @"<?xml version=""1.0"" encoding=""UTF-8""?><message:GenericData xmlns:message=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message"" xmlns:common=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/common"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:generic=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic"" xsi:schemaLocation=""http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message https://sdw-wsrest.ecb.europa.eu:443/vocabulary/sdmx/2_1/SDMXMessage.xsd http://www.sdmx.org/resources/sdmxml/schemas/v2_1/common https://sdw-wsrest.ecb.europa.eu:443/vocabulary/sdmx/2_1/SDMXCommon.xsd http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic https://sdw-wsrest.ecb.europa.eu:443/vocabulary/sdmx/2_1/SDMXDataGeneric.xsd"">
<message:Header>
<message:ID>615a7764-34d9-4e27-9901-b983fd2da9c0</message:ID>
<message:Test>false</message:Test>
<message:Prepared>2018-08-11T10:55:40.970+02:00</message:Prepared>
<message:Sender id=""ECB""/>
<message:Structure structureID=""ECB_EXR1"" dimensionAtObservation=""TIME_PERIOD"">
<common:Structure>
<URN>urn:sdmx:org.sdmx.infomodel.datastructure.DataStructure=ECB:ECB_EXR1(1.0)</URN>
</common:Structure>
</message:Structure>
</message:Header>
<message:DataSet action=""Replace"" validFromDate=""2018-08-11T10:55:40.970+02:00"" structureRef=""ECB_EXR1"">
<generic:Series>
<generic:SeriesKey>
<generic:Value id=""FREQ"" value=""D""/>
<generic:Value id=""CURRENCY"" value=""GBP""/>
<generic:Value id=""CURRENCY_DENOM"" value=""EUR""/>
<generic:Value id=""EXR_TYPE"" value=""SP00""/>
<generic:Value id=""EXR_SUFFIX"" value=""A""/>
</generic:SeriesKey>
<generic:Obs>
<generic:ObsDimension value=""2017-08-07""/>
<generic:ObsValue value=""0.90435""/>
</generic:Obs>
</generic:Series>
<generic:Series>
<generic:SeriesKey>
<generic:Value id=""FREQ"" value=""D""/>
<generic:Value id=""CURRENCY"" value=""USD""/>
<generic:Value id=""CURRENCY_DENOM"" value=""EUR""/>
<generic:Value id=""EXR_TYPE"" value=""SP00""/>
<generic:Value id=""EXR_SUFFIX"" value=""A""/>
</generic:SeriesKey>
<generic:Obs>
<generic:ObsDimension value=""2017-08-07""/>
<generic:ObsValue value=""1.1797""/>
</generic:Obs>
</generic:Series>
<generic:Series>
<generic:SeriesKey>
<generic:Value id=""FREQ"" value=""D""/>
<generic:Value id=""CURRENCY"" value=""GBP""/>
<generic:Value id=""CURRENCY_DENOM"" value=""EUR""/>
<generic:Value id=""EXR_TYPE"" value=""SP00""/>
<generic:Value id=""EXR_SUFFIX"" value=""A""/>
</generic:SeriesKey>
<generic:Obs>
<generic:ObsDimension value=""2017-08-08""/>
<generic:ObsValue value=""0.90678""/>
</generic:Obs>
</generic:Series>
<generic:Series>
<generic:SeriesKey>
<generic:Value id=""FREQ"" value=""D""/>
<generic:Value id=""CURRENCY"" value=""USD""/>
<generic:Value id=""CURRENCY_DENOM"" value=""EUR""/>
<generic:Value id=""EXR_TYPE"" value=""SP00""/>
<generic:Value id=""EXR_SUFFIX"" value=""A""/>
</generic:SeriesKey>
<generic:Obs>
<generic:ObsDimension value=""2017-08-08""/>
<generic:ObsValue value=""1.1814""/>
</generic:Obs>
</generic:Series>
</message:DataSet>
</message:GenericData>
            ";

            partialSubstitute.GetContent().ReturnsForAnyArgs(new Client.RestResult { Content = content, State = ConverterState.Success });

            var result = partialSubstitute.BuildForDate();
            result.State.Should().Be(ConverterState.Success);
        }
    }
}
