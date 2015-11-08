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
        private readonly ProcedureService procedureService;

        public GeocoderService()
        {
            geocoderEntities = new GeocoderEntities();
            procedureService = new ProcedureService();
        }

        public List<HINTCITYGEOCITYCR> GetIL_IDDataByHintCityGeoCityCR(string ilAdı)
        {
            var result = geocoderEntities.HINTCITYGEOCITYCR.Where(x => x.IL_ADI == ilAdı && x.ACTIVE == "1" && x.AUDIT_DELETED == "0").ToList();
            return result;
        }

        public List<IL> GetIlDataByIL_ID(long ilId)
        {
            var result = geocoderEntities.IL.Where(x => x.IL_ID == ilId).ToList();
            return result;
        }

        public List<ILCE> GetIlceDataByILCE_ID(long ilceId)
        {
            var result = geocoderEntities.ILCE.Where(x => x.ILCE_ID == ilceId).ToList();
            return result;
        }

        public List<HINTTOWNGEOTOWNCR> GetGeoloc_Ilce_IDDataByHINTTOWNGEOTOWNCR(string ilçeAdı, long ilId)
        {
            var result =
                 geocoderEntities.HINTTOWNGEOTOWNCR.Where(x => x.ILCE_ADI == ilçeAdı && x.GEOLOC_IL_ID == ilId && x.ACTIVE == "1" && x.AUDIT_DELETED == "0").ToList();
            return result;
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

        public List<VUnitSearch> GetUnitSearchDataByIlAndIlceId(string search, long ilceId)
        {
            var dataSetResult = procedureService.GetUnitSearchDataByIlAndIlceId(search, ilceId);
            var result = ModelConverter.DataSetToVUnitSearch(dataSetResult);

            return result;
        }

        public List<VUnitSearch> GetUnitSearchDataByIlId(string search, long ilId)
        {
            var dataSetResult = procedureService.GetUnitSearchDataByIlAndIlceId(search, ilId);
            var result = ModelConverter.DataSetToVUnitSearch(dataSetResult);

            return result;
        }

        public IList<KAPI> GetKapiDataByYOL_IDAndMAHALLE_ID(long mahalleId, long yolId)
        {
            IList<KAPI> result = geocoderEntities.KAPI.Where(x => x.YOL_ID == yolId && x.MAHALLE_ID == mahalleId).ToList();
            return result;
        }

        public List<YolIdariDtoModel> GetIdariYolDataByMahalleAndYolId(long yolId, long mahalleId)
        {
            var result = from yol in geocoderEntities.YOL
                         from isy in geocoderEntities.IDARI_SINIR_YOL
                         where yol.YOL_ID == isy.YOL_ID
                         where isy.YOL_ID == yolId
                         where isy.MAHALLE_ID == mahalleId
                         select new YolIdariDtoModel()
                         {
                             IlceId = (long)isy.ILCE_ID.Value,
                             MahalleId = (long)isy.MAHALLE_ID,
                             YolId = (long)isy.YOL_ID,
                             IlAdı = isy.IL_ADI,
                             IlceAdı = isy.ILCE_ADI,
                             MahalleAdı = isy.MAHALLE_ADI,
                             IlId = (long)isy.IL_ID,
                             XCoor = isy.XCOOR,
                             YCoor = isy.YCOOR,
                             YolAdı = yol.YOL_ADI,
                             YolSınıfı = (int)yol.YOL_SINIFI.Value
                         };

            return result.ToList();
        }

        public List<MAHALLE> GetMahalleDataByMAHALLE_ID(long mahalleId)
        {
            var result = geocoderEntities.MAHALLE.Where(x => x.MAHALLE_ID == mahalleId).ToList();
            return result;
        }

        //TODO: POI TABLOSU DB YE EKLENECEK
        public List<POI_ARAS> GetPOIDataByPOI_ID(long poiId)
        {
            var result = geocoderEntities.POI_ARAS.Where(x => x.ID == poiId).ToList();
            return result;
        }

        
    }
}
