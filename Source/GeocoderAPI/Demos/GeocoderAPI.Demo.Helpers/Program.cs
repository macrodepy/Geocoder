using System.Collections.Generic;
using OtherGeocoders.Implementation;
using OtherGeocoders.Model;

namespace GeocoderAPI.Demo.Helpers
{
    class Program
    {

        static void Main(string[] args)
        {
            GoogleGeocoder googleGeocoder = new GoogleGeocoder();
            YandexGeocoder yandexGeocoder = new YandexGeocoder();

            RequestData request = new RequestData()
            {
                Address = "inönü mahallesi atatürk caddesi birlik apt. no:55 daire 7 istanbul ataşehir türkiye"
            };

            ResponseData googleResult = googleGeocoder.Geocode(request);

            List<GoogleResult> googleGeocodeResultList = googleGeocoder.GeocodeAndParse(request);

            ResponseData yandexResult = yandexGeocoder.Geocode(request);

            List<YandexResult> yandexGeocodeResultList = yandexGeocoder.GeocodeAndParse(request);
        }
    }
}
