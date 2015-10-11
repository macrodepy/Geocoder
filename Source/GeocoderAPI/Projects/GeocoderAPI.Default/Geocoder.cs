using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeocoderAPI.DAL;
using GeocoderAPI.Model;

namespace GeocoderAPI.Default
{
    public class Geocoder
    {
        private readonly CultureInfo cultureInfo;
        private readonly GeocoderEntities entities;
        public Geocoder()
        {
            cultureInfo = new CultureInfo("tr-TR");
            entities = new GeocoderEntities();
        }


        public void Geocode(AddressLevel addressLevel)
        {
            addressLevel.Il = addressLevel.Il.ToUpper(cultureInfo);
            addressLevel.Ilçe = addressLevel.Ilçe.ToUpper(cultureInfo);

            if (addressLevel.Il.Equals(string.Empty))
            {
                return;
            }
            
        }

        private List<HINTTOWNGEOTOWNCR> GetTown(AddressLevel addressLevel, HINTCITYGEOCITYCR city)
        {
            var townList =
                 entities.HINTTOWNGEOTOWNCR.Where(
                     x => x.ILCE_ADI == addressLevel.Ilçe && x.IL_ID == city.IL_ID).ToList();

            return townList;
        }

        private HINTCITYGEOCITYCR GetCity(AddressLevel addressLevel)
        {
            var cityList = entities.HINTCITYGEOCITYCR.Where(x => x.IL_ADI == addressLevel.Il).ToList();
            var city = cityList.FirstOrDefault();

            return city;
        }

    }
}
