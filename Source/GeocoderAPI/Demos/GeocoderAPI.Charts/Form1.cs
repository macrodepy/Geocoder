using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GeocoderAPI.DAL;
using GeocoderAPI.Model;

namespace GeocoderAPI.Charts
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            GeocoderService service = new GeocoderService();
            List<TestResultModel> sampleAddressResult = service.GetSampleAddressResult();

            int totalSampleAddress = sampleAddressResult.Count;

            //Tespit Sayısı
            int myGeocoderFoundCount = sampleAddressResult.Count(x => x.MyXCoor != null);
            int yandexFoundCount = sampleAddressResult.Count(x => x.YandexXCoor != null);
            int geoogleFoundCount = sampleAddressResult.Count(x => x.GoogleXCoor != null);

            //Doğru Tespit Sayısı
            List<TestResultModel> myGeocoderFoundTrue = sampleAddressResult
                .Where(x => x.MyXCoor != null &&
                            Math.Round(Convert.ToDecimal(x.MyXCoor.Replace('.', ',')), 3) ==
                            Math.Round(Convert.ToDecimal(x.ActualXCoor.Replace('.', ',')), 3) &&
                            Math.Round(Convert.ToDecimal(x.MyYCoor.Replace('.', ',')), 3) ==
                            Math.Round(Convert.ToDecimal(x.ActualYCoor.Replace('.', ',')), 3)).ToList();

            List<TestResultModel> yandexFoundTrue = sampleAddressResult
                .Where(x => x.YandexXCoor != null &&
                            Math.Round(Convert.ToDecimal(x.YandexXCoor.Replace('.', ',')), 3) ==
                            Math.Round(Convert.ToDecimal(x.ActualXCoor.Replace('.', ',')), 3) &&
                            Math.Round(Convert.ToDecimal(x.YandexYCoor.Replace('.', ',')), 3) ==
                            Math.Round(Convert.ToDecimal(x.ActualYCoor.Replace('.', ',')), 3)).ToList();

            List<TestResultModel> geoogleFoundTrue = sampleAddressResult
                .Where(x => x.GoogleXCoor != null &&
                            Math.Round(Convert.ToDecimal(x.GoogleXCoor.Replace('.', ',')), 3) ==
                            Math.Round(Convert.ToDecimal(x.ActualXCoor.Replace('.', ',')), 3) &&
                            Math.Round(Convert.ToDecimal(x.GoogleYCoor.Replace('.', ',')), 3) ==
                            Math.Round(Convert.ToDecimal(x.ActualYCoor.Replace('.', ',')), 3)).ToList();

            //Yanlış tespit sayısı
            int myFoundWrong = myGeocoderFoundCount - myGeocoderFoundTrue.Count();
            int yandexFoundWrong = yandexFoundCount - yandexFoundTrue.Count();
            int googleFoundWrong = geoogleFoundCount - geoogleFoundTrue.Count();

            //Tespit Oranı
            double myRatio = (myGeocoderFoundCount*100)/totalSampleAddress;
            double yandexRatio = (yandexFoundCount*100)/totalSampleAddress;
            double googleRatio = (geoogleFoundCount*100)/totalSampleAddress;

            //Doğru tespit oranı
            double myRatioTrue = (myGeocoderFoundTrue.Count() * 100) / totalSampleAddress;
            double yandexRatioTrue = (yandexFoundTrue.Count() * 100) / totalSampleAddress;
            double googleRatioTrue = (geoogleFoundTrue.Count() * 100) / totalSampleAddress;

            //Bulma Zamanı

            var myGeocoderFound = sampleAddressResult.Where(x => x.MyXCoor != null).ToList();
            TimeSpan myTime = new TimeSpan();
            IList<long> tickLong = new List<long>();
            foreach (var item in myGeocoderFound)
            {
                TimeSpan date = TimeSpan.Parse(item.MyTime);
                tickLong.Add(date.Ticks);
                myTime = myTime.Add(date);
            }

            long myTicks = myTime.Ticks / myGeocoderFound.Count;
            //TimeSpan myTimeSpan = new TimeSpan(myTicks);  // MyGeocoder
            TimeSpan myTimeSpan = new TimeSpan((long)CalculateStandardDeviation(tickLong));  // MyGeocoder

            var yandexGeocoderFound = sampleAddressResult.Where(x => x.YandexXCoor != null).ToList();

            TimeSpan yandexTime = new TimeSpan();
            foreach (var item in yandexGeocoderFound)
            {
                TimeSpan date = TimeSpan.Parse(item.YandexTime);
                yandexTime = yandexTime.Add(date);
            }

            long yandexTicks = yandexTime.Ticks / yandexGeocoderFound.Count;
            TimeSpan yandexTimeSpan = new TimeSpan(yandexTicks);  //YandexGeocoder


            var googleGeocoderFound = sampleAddressResult.Where(x => x.GoogleXCoor != null).ToList();

            TimeSpan googleTime = new TimeSpan();
            foreach (var item in googleGeocoderFound)
            {
                TimeSpan date = TimeSpan.Parse(item.GoogleTime);
                googleTime = googleTime.Add(date);
            }

            long googleTicks = googleTime.Ticks / googleGeocoderFound.Count;
            TimeSpan googleTimeSpan = new TimeSpan(googleTicks);  // GoogleGeocoder

        }


        private double CalculateStandardDeviation(IEnumerable<long> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }
    }
}
