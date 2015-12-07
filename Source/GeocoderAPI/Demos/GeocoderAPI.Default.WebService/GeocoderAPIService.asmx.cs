using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using GeocoderAPI.DAL;

namespace GeocoderAPI.Default.WebService
{
    /// <summary>
    /// Summary description for GeocoderAPIService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class GeocoderAPIService : System.Web.Services.WebService
    {
        private readonly Tokenizer tokenizer = new Tokenizer();
        private readonly Geocoder geocoder = new Geocoder();
        private readonly GeocoderService geocoderService = new GeocoderService();

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string Fixer(string address)
        {
            return Default.Fixer.Prepare(address);
        }

        [WebMethod]
        public AddressLevel Parser(string address)
        {
            return tokenizer.ParseAddress(address);
        }

        [WebMethod]
        public AddressLevel Geocoder(AddressLevel addressLevel)
        {
            return geocoder.Geocode(addressLevel);
        }

        [WebMethod]
        public AddressLevel Geocode(string address)
        {
            string fixedAddress = Default.Fixer.Prepare(address);

            AddressLevel addressLevel = tokenizer.ParseAddress(fixedAddress);
            List<string> list = tokenizer.NotParsedList;

            list = CheckForCity(list, ref addressLevel);

            if (!addressLevel.Il.Equals(string.Empty))
            {
                list = CheckForTown(list, ref addressLevel);
            }

            addressLevel = geocoder.Geocode(addressLevel);

            return addressLevel;
        }


        private List<string> CheckForTown(IEnumerable<string> notParsedList, ref AddressLevel addressLevel)
        {
            List<string> result = new List<string>();
            decimal ilId = addressLevel.IlId;

            foreach (var item in notParsedList)
            {
                var town = geocoderService.GetTownByNameAndCityId(item.Trim(), addressLevel.IlId);

                if (town != null)
                    addressLevel.Ilçe = item.Trim();
                else
                    result.Add(item);
            }

            return result;
        }

        private List<string> CheckForCity(IEnumerable<string> notParsedList, ref AddressLevel addressLevel)
        {
            List<string> result = new List<string>();

            foreach (var item in notParsedList)
            {
                var city = geocoderService.GetCityByName(item.Trim());

                if (city != null)
                {
                    addressLevel.Il = item.Trim();
                    addressLevel.IlId = city.IL_ID;
                }
                else
                    result.Add(item);
            }

            return result;
        }
    }
}
