using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using OtherGeocoders.Core;
using OtherGeocoders.Model;

namespace OtherGeocoders.Implementation
{
    public class YandexGeocoder : IGeocoder
    {
        public ResponseData Geocode(RequestData requestData)
        {
            var resp = new ResponseData();

            string url = "http://geocode-maps.yandex.ru/1.x/?geocode={0}&lang=tr-TR";

            WebResponse response = null;
            try
            {
                url = string.Format(url, requestData.Address);
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

        private decimal GetYandexFoundedGeocodeCount(XmlDocument document)
        {
            var xmlNodeList = document.GetElementsByTagName("found");
            var node = xmlNodeList[0];

            if (node == null) return 0;

            decimal nodeValue;
            string value = node.InnerText;

            var geocodeCountExist = decimal.TryParse(value, out nodeValue);

            return !geocodeCountExist ? 0 : nodeValue;
        }

        private List<YandexResult> YandexGeocodeResult(XmlDocument document)
        {
            List<YandexResult> results = new List<YandexResult>();
            XmlNodeList xmlNodeList = document.GetElementsByTagName("featureMember");

            if (xmlNodeList.Count == 0) return results;

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                var xml = new XmlDocument();
                xml.LoadXml(xmlNode.InnerXml);

                var result = new YandexResult
                {
                    Address = xml.GetElementsByTagName("text")[0].InnerText,
                    Precision = xml.GetElementsByTagName("precision")[0].InnerText,
                    Position = xml.GetElementsByTagName("pos")[0].InnerText,
                };

                results.Add(result);
            }

            return results;
        }

        public List<YandexResult> GeocodeAndParse(RequestData requestData)
        {
            ResponseData responseData = Geocode(requestData);
            XmlDocument xml = Common.GetXmlDocumentFromXmlString(responseData.Data);
            decimal yandexCount = GetYandexFoundedGeocodeCount(xml);

            if (yandexCount == 0)
            {
                return new List<YandexResult>();
            }

            List<YandexResult> resultList = YandexGeocodeResult(xml);
            return resultList;
        }
    }
}
