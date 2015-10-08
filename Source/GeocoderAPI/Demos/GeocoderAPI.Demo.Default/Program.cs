using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeocoderAPI.DAL;
using GeocoderAPI.Default;
using GeocoderAPI.Model;

namespace GeocoderAPI.Demo.Default
{
    public class Program
    {


        static void Main(string[] args)
        {
            GeocoderEntities entities = new GeocoderEntities();

            //GEOLOC_IL il = entities.GEOLOC_IL.FirstOrDefault(x => x.IL_ADI == "İSTANBUL");
            //string address = "inönü mahallesi atatürk caddesi birlik apt. no:55 daire:7";

            string address = "inönü mahallesi atatürk caddesi birlik apt. no:55 daire 7 istanbul ataşehir";
            string fixerTest = FixerTest(address);

            Parse parse = new Parse();
            AddressLevel addressLevel = parse.ParseAddress(fixerTest);
            List<string> list = parse.notParsedList;

            CheckForCity(ref list, addressLevel);

            GeocoderTest(addressLevel);

        }

        static AddressLevel CheckForCity(ref List<string> notParsedList, AddressLevel addressLevel)
        {
            GeocoderEntities entities = new GeocoderEntities();

            foreach (var item in notParsedList)
            {
                var city = entities.HINTCITYGEOCITYCR.FirstOrDefault(x => x.IL_ADI == item.Trim());

                if (city != null)
                {
                    addressLevel.Il = item;
                    notParsedList.Remove(item);
                }
            }

            return addressLevel;
        }

        static void GeocoderTest(AddressLevel addressLevel)
        {
            Geocoder geocoder = new Geocoder();
            geocoder.Geocode(addressLevel);
        }

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
