using System.Collections.Generic;
using System.Linq;
using GeocoderAPI.DAL;
using GeocoderAPI.Default;
using GeocoderAPI.Model;

namespace GeocoderAPI.Demo.Default
{
    public class Program
    {
        private static GeocoderEntities entities;
        private static GeocoderService geocoderService;
        public Program()
        {
        }

        static void Main(string[] args)
        {
            //entities = new GeocoderEntities();
            //geocoderService = new GeocoderService();
         
            //string address = "inönü mahallesi atatürk caddesi birlik apt. no:55 daire 7 istanbul ataşehir";
            //string fixerTest = FixerTest(address);

            //Parse parse = new Parse();
            //AddressLevel addressLevel = parse.ParseAddress(fixerTest);
            //List<string> list = parse.NotParsedList;

            //list = CheckForCity(list, ref addressLevel);

            //if (!addressLevel.Il.Equals(string.Empty))
            //{
            //    list = CheckForTown(list, ref addressLevel);
            //}

            //GeocoderTest(addressLevel);
        }

        private static List<string> CheckForTown(IEnumerable<string> notParsedList, ref AddressLevel addressLevel)
        {
            List<string> result = new List<string>();
            decimal ilId = addressLevel.IlId;

            foreach (var item in notParsedList)
            {
                var town = entities.HINTTOWNGEOTOWNCR.FirstOrDefault(x => x.ILCE_ADI == item.Trim() && x.IL_ID == ilId);

                if (town != null)
                    addressLevel.Ilçe = item.Trim();
                else
                    result.Add(item);
            }

            return result;
        }

        static List<string> CheckForCity(List<string> notParsedList, ref AddressLevel addressLevel)
        {
            List<string> result = new List<string>();

            foreach (var item in notParsedList)
            {
                //var city = GetCityByName(item.Trim());
                var city = entities.HINTCITYGEOCITYCR.FirstOrDefault(x => x.IL_ADI == item.Trim());

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

        //static void GeocoderTest(AddressLevel addressLevel)
        //{
        //    Geocoder geocoder = new Geocoder();
        //    geocoder.Geocode(addressLevel);
        //}

        static string FixerTest(string address)
        {
            string preparedAddress = Fixer.Prepare(address);
            return preparedAddress;
        }

        static AddressLevel ParseTest(string address)
        {
            Parse parse = new Parse();
            AddressLevel addressLevel = parse.ParseAddress(address);
            return addressLevel;
        }
    }
}
