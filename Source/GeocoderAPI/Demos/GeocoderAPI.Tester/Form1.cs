using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GeocoderAPI.DAL;
using GeocoderAPI.Default;
using OtherGeocoders.Implementation;
using OtherGeocoders.Model;

namespace GeocoderAPI.Tester
{
    public partial class Form1 : Form
    {
        private readonly GoogleGeocoder googleGeocoder;
        private readonly YandexGeocoder yandexGeocoder;
        private readonly GeocoderService geocoderService;
        private readonly Tokenizer tokenizer;
        private readonly Geocoder geocoder;

        public Form1()
        {
            InitializeComponent();

            googleGeocoder = new GoogleGeocoder();
            yandexGeocoder = new YandexGeocoder();
            geocoder = new Geocoder();
            tokenizer = new Tokenizer();
            geocoderService = new GeocoderService();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<SAMPLEADDRESS> samples = geocoderService.GetSampleaddresss();

            Stopwatch timer = new Stopwatch();

            foreach (var item in samples)
            {
                bool checkIfExist = geocoderService.CheckIfExist(item);

                if (checkIfExist)
                {
                    continue;
                }


                SAMPLEADDRESSRESULT result = new SAMPLEADDRESSRESULT();

                RequestData request = new RequestData();
                request.Address = item.ADDRESS + " " + item.CITY + " " + item.TOWN;
                timer.Reset();
                timer.Start();
                List<YandexResult> yandexResults = yandexGeocoder.GeocodeAndParse(request);
                timer.Stop();

                if (yandexResults.Count == 1)
                {
                    string[] coordinates = yandexResults[0].Position.Split(' ');
                    string xCoor = coordinates[0];
                    string yCoor = coordinates[1];

                    result.XCOORYANDEX = xCoor;
                    result.YCOORYANDEX = yCoor;
                    result.YANDEXTIME = timer.Elapsed.ToString();
                }
                timer.Reset();

                timer.Start();
                List<GoogleResult> googleResults = googleGeocoder.GeocodeAndParse(request);
                timer.Stop();

                if (googleResults.Count == 1)
                {
                    string xCoor = googleResults[0].Longtitute;
                    string yCoor = googleResults[0].Latitute;

                    result.XCOORGOOGLE = xCoor;
                    result.YCOORGOOGLE = yCoor;
                    result.GOOGLETIME = timer.Elapsed.ToString();
                }

                timer.Reset();

                timer.Start();
                AddressLevel addressLevel = Geocode(request.Address);
                timer.Stop();

                if (addressLevel.XCoor != string.Empty || addressLevel.XCoor != "0")
                {
                    string xCoor = addressLevel.XCoor;
                    string yCoor = addressLevel.YCoor;

                    result.XCOORMY = xCoor;
                    result.YCOORMY = yCoor;
                    result.MYTIME = timer.Elapsed.ToString();
                }

                result.ID = item.ID;
                try
                {
                    geocoderService.InsertSampleAddressResult(result);
                }
                catch
                {
                    continue;
                }
            }
        }

        public AddressLevel Geocode(string address)
        {
            string fixedAddress = Fixer.Prepare(address);

            AddressLevel addressLevel = tokenizer.ParseAddress(fixedAddress);
            List<string> list = tokenizer.NotParsedList;

            list = geocoder.CheckForCity(list, ref addressLevel);

            if (!addressLevel.Il.Equals(string.Empty))
            {
                list = geocoder.CheckForTown(list, ref addressLevel);
            }

            addressLevel = geocoder.Geocode(addressLevel);

            return addressLevel;
        }
    }
}
