using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.Model
{
    public class GeocoderAPIResultModel
    {
        public GeocoderAPIResultModel()
        {
            OriginalAddress = string.Empty;
            Il = string.Empty;
            Ilçe = string.Empty;
            Mahalle = string.Empty;
            Köy = string.Empty;
            Cadde = string.Empty;
            Sokak = string.Empty;
            Bulvar = string.Empty;
            Yolu = string.Empty;
            Poi = string.Empty;
            PostaKodu = string.Empty;
            Bina = string.Empty;
            Blok = string.Empty;
            Daire = string.Empty;
            Kapı = string.Empty;
            Kat = string.Empty;
            Poi1 = string.Empty;
            Poi2 = string.Empty;
            Poi3 = string.Empty;
            PoiS = string.Empty;
            Cadde1 = string.Empty;
            Cadde2 = string.Empty;
            CaddeS = string.Empty;
            Sokak1 = string.Empty;
            Sokak2 = string.Empty;
            SokakS = string.Empty;
            Bulvar1 = string.Empty;
            Bulvar2 = string.Empty;
            BulvarS = string.Empty;
        }

        public string XCoor { get; set; }
        public string YCoor { get; set; }
        public int CoordinateLevel { get; set; }

        public string OriginalAddress { get; set; }
        public string Il { get; set; }
        public string Ilçe { get; set; }
        public string Mahalle { get; set; }
        public string Köy { get; set; }
        public string Cadde { get; set; }
        public string Sokak { get; set; }
        public string Bulvar { get; set; }
        public string Yolu { get; set; }
        public string Poi { get; set; }
        public string PostaKodu { get; set; }
        public string Bina { get; set; }
        public string Blok { get; set; }
        public string Daire { get; set; }
        public string Kapı { get; set; }
        public string Kat { get; set; }
        public string Poi1 { get; set; }
        public string Poi2 { get; set; }
        public string Poi3 { get; set; }
        public string PoiS { get; set; }
        public string Cadde1 { get; set; }
        public string Cadde2 { get; set; }
        public string CaddeS { get; set; }
        public string Sokak1 { get; set; }
        public string Sokak2 { get; set; }
        public string SokakS { get; set; }
        public string Bulvar1 { get; set; }
        public string Bulvar2 { get; set; }
        public string BulvarS { get; set; }
    }
}
