using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeocoderAPI.Model;

namespace GeocoderAPI.DAL
{
    public class GeocoderService
    {
        private readonly GeocoderEntities geocoderEntities;

        public GeocoderService()
        {
            geocoderEntities = new GeocoderEntities();
        }

        public HINTCITYGEOCITYCR GetCityByName(string name)
        {
            var city = geocoderEntities.HINTCITYGEOCITYCR.FirstOrDefault(x => x.IL_ADI == name.Trim());
            return city;
        }

        public HINTTOWNGEOTOWNCR GetTownByNameAndCityId(string name, decimal cityId)
        {
            var town = geocoderEntities.HINTTOWNGEOTOWNCR.FirstOrDefault(x => x.ILCE_ADI == name.Trim() && x.IL_ID == cityId);
            return town;
        }

        private List<HINTTOWNGEOTOWNCR> GetTown(AddressLevel addressLevel, HINTCITYGEOCITYCR city)
        {
            var townList =
                 geocoderEntities.HINTTOWNGEOTOWNCR.Where(
                     x => x.ILCE_ADI == addressLevel.Ilçe && x.IL_ID == city.IL_ID).ToList();

            return townList;
        }

        private HINTCITYGEOCITYCR GetCity(AddressLevel addressLevel)
        {
            var city = geocoderEntities.HINTCITYGEOCITYCR.FirstOrDefault(x => x.IL_ADI == addressLevel.Il);
            return city;
        }
    }
}
