using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using OtherGeocoders.Core;
using OtherGeocoders.Model;

namespace OtherGeocoders.Implementation
{
    public class GoogleGeocoder : IGeocoder
    {
        private const string GoogleGeocodeApiKey = "AIzaSyC0VGKDBEjOn1gI2v5LHRKB3kBL4dZ9vLM";

        public ResponseData Geocode(RequestData requestData)
        {
            var resp = new ResponseData();

            string url = "https://maps.googleapis.com/maps/api/geocode/xml?address={0}&key={1}";

            WebResponse response = null;

            try
            {
                url = string.Format(url, requestData.Address, GoogleGeocodeApiKey);
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    resp.Data = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return resp;
        }

        private decimal GetGoogleFoundedGeocodeCount(XmlDocument document)
        {
            var xmlNodeList = document.GetElementsByTagName("status");
            var node = xmlNodeList[0];

            if (!node.InnerText.Equals("OK")) return 0;

            xmlNodeList = document.GetElementsByTagName("result");
            return xmlNodeList.Count;
        }

        private List<GoogleResult> GoogleGeocodeResult(XmlDocument document)
        {
            List<GoogleResult> results = new List<GoogleResult>();
            XmlNodeList xmlNodeList = document.GetElementsByTagName("result");

            if (xmlNodeList.Count == 0) return results;

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                var xml = new XmlDocument();
                xml.LoadXml(("<root>" + xmlNode.InnerXml + "</root>"));

                var result = new GoogleResult()
                {
                    Address = xml.GetElementsByTagName("formatted_address")[0].InnerText,
                    FormattedAddress = xml.GetElementsByTagName("formatted_address")[0].InnerText,
                    Latitute = xml.GetElementsByTagName("lat")[0].InnerText,
                    Longtitute = xml.GetElementsByTagName("lng")[0].InnerText,
                    LocationType = xml.GetElementsByTagName("location_type")[0].InnerText,
                    Type = xml.GetElementsByTagName("type")[0].InnerText,
                };

                results.Add(result);

            }

            return results;
        }

        public List<GoogleResult> GeocodeAndParse(RequestData requestData)
        {
            ResponseData responseData = Geocode(requestData);
            XmlDocument xml = Common.GetXmlDocumentFromXmlString(responseData.Data);
            decimal geocodeCount = GetGoogleFoundedGeocodeCount(xml);
           
            if (geocodeCount == 0)
            {
                return new List<GoogleResult>();
            }

            List<GoogleResult> resultList = GoogleGeocodeResult(xml);
            return resultList;
        }
    }
}
