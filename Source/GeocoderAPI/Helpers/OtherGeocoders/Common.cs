using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OtherGeocoders
{
    public static class Common
    {
        public static XmlDocument GetXmlDocumentFromXmlString(string xml)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }
    }
}
