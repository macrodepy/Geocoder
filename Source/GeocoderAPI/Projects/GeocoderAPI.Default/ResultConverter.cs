using GeocoderAPI.DAL;
using GeocoderAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.Default
{
    public static class ResultConverter
    {
        public static GeocoderAPIResultModel ConvertAddressLevelToGeocoderAPIResultModel(AddressLevel model)
        {
            GeocoderAPIResultModel result = new GeocoderAPIResultModel();

            result.Bina = model.Bina;
            result.Blok = model.Blok;
            result.Bulvar = model.Bulvar;
            result.Bulvar1 = model.Bulvar1;
            result.Bulvar2 = model.Bulvar2;
            result.BulvarS = model.BulvarS;
            result.Cadde = model.Cadde;
            result.Cadde1 = model.Cadde1;
            result.Cadde2 = model.Cadde2;
            result.CaddeS = model.CaddeS;
            result.CoordinateLevel = model.CoordinateLevel;
            result.Daire = model.Daire;
            result.Il = model.Il;
            result.Ilçe = model.Ilçe;
            result.Kapı = model.Kapı;
            result.Kat = model.Kat;
            result.Köy = model.Köy;
            result.Mahalle = model.Mahalle;
            result.OriginalAddress = model.OriginalAddress;
            result.Poi = model.Poi;
            result.Poi1 = model.Poi1;
            result.Poi2 = model.Poi2;
            result.Poi3 = model.Poi3;
            result.PoiS = model.PoiS;
            result.PostaKodu = model.PostaKodu;
            result.Sokak = model.Sokak;
            result.Sokak1 = model.Sokak1;
            result.Sokak2 = model.Sokak2;
            result.SokakS = model.SokakS;
            result.XCoor = model.XCoor;
            result.YCoor = model.YCoor;
            result.Yolu = model.Yolu;

            return result;
        }
    }
}
