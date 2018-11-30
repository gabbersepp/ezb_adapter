namespace EzbAdapter.Contracts.Xml
{
    /* 
     Licensed under the Apache License, Version 2.0

     http://www.apache.org/licenses/LICENSE-2.0
     */
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    namespace Xml2CSharp
    {
        [XmlRoot(ElementName = "DataSet", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
        public class XmlDataSet
        {
            [XmlAttribute(AttributeName = "action")]
            public string Action { get; set; }
            [XmlElement(ElementName = "Series", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
            public List<XmlSeries> Series { get; set; }
            [XmlAttribute(AttributeName = "structureRef")]
            public string StructureRef { get; set; }
            [XmlAttribute(AttributeName = "validFromDate")]
            public string ValidFromDate { get; set; }
        }

        [XmlRoot(ElementName = "GenericData", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
        public class XmlGenericData
        {
            [XmlAttribute(AttributeName = "common", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Common { get; set; }
            [XmlElement(ElementName = "DataSet", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
            public XmlDataSet DataSet { get; set; }
            [XmlAttribute(AttributeName = "generic", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Generic { get; set; }
            [XmlElement(ElementName = "Header", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
            public XmlHeader Header { get; set; }
            [XmlAttribute(AttributeName = "message", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Message { get; set; }
            [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
            public string SchemaLocation { get; set; }
            [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Xsi { get; set; }
        }

        [XmlRoot(ElementName = "Header", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
        public class XmlHeader
        {
            [XmlElement(ElementName = "ID", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
            public string ID { get; set; }
            [XmlElement(ElementName = "Prepared", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
            public string Prepared { get; set; }
            [XmlElement(ElementName = "Sender", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
            public XmlSender Sender { get; set; }
            [XmlElement(ElementName = "Structure", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
            public XmlStructure2 Structure2 { get; set; }
            [XmlElement(ElementName = "Test", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
            public string Test { get; set; }
        }

        [XmlRoot(ElementName = "Obs", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
        public class XmlObs
        {
            [XmlElement(ElementName = "ObsDimension", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
            public XmlObsDimension ObsDimension { get; set; }
            [XmlElement(ElementName = "ObsValue", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
            public XmlObsValue ObsValue { get; set; }
        }

        [XmlRoot(ElementName = "ObsDimension", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
        public class XmlObsDimension
        {
            [XmlAttribute(AttributeName = "value")]
            public string Value { get; set; }
        }

        [XmlRoot(ElementName = "ObsValue", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
        public class XmlObsValue
        {
            [XmlAttribute(AttributeName = "value")]
            public double Value { get; set; }
        }

        [XmlRoot(ElementName = "Sender", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
        public class XmlSender
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }
        }

        [XmlRoot(ElementName = "Series", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
        public class XmlSeries
        {
            [XmlElement(ElementName = "Obs", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
            public List<XmlObs> Obs { get; set; }
            [XmlElement(ElementName = "SeriesKey", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
            public XmlSeriesKey SeriesKey { get; set; }
        }

        [XmlRoot(ElementName = "SeriesKey", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
        public class XmlSeriesKey
        {
            [XmlElement(ElementName = "Value", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
            public List<XmlValue> Value { get; set; }
        }

        [XmlRoot(ElementName = "Structure", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/common")]
        public class XmlStructure
        {
            [XmlElement(ElementName = "URN")]
            public string URN { get; set; }
        }

        [XmlRoot(ElementName = "Structure", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message")]
        public class XmlStructure2
        {
            [XmlAttribute(AttributeName = "dimensionAtObservation")]
            public string DimensionAtObservation { get; set; }
            [XmlElement(ElementName = "Structure", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/common")]
            public XmlStructure Structure { get; set; }
            [XmlAttribute(AttributeName = "structureID")]
            public string StructureID { get; set; }
        }

        [XmlRoot(ElementName = "Value", Namespace = "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic")]
        public class XmlValue
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }
            [XmlAttribute(AttributeName = "value")]
            public string Value { get; set; }
        }

    }

}