using OtherGeocoders.Model;

namespace OtherGeocoders.Core
{
    interface IGeocoder
    {
        ResponseData Geocode(RequestData requestData);
    }
}
