namespace GeocoderAPI.Core
{
    public interface IGeocoder
    {
        void Geocode();
        void Prepare();
        void Parse();
    }
}
