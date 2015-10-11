using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.Model
{
    public class AddressLevel
    {
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

        public decimal IlId { get; set; }
        public decimal IlceId { get; set; }
    }
}
