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
                            Math.Round(Convert.ToDecimal(x.MyXCoor.Replace('.', ',')), 2) ==
                            Math.Round(Convert.ToDecimal(x.ActualXCoor.Replace('.', ',')), 2) &&
                            Math.Round(Convert.ToDecimal(x.MyYCoor.Replace('.', ',')), 2) ==
                            Math.Round(Convert.ToDecimal(x.ActualYCoor.Replace('.', ',')), 2)).ToList();

            List<TestResultModel> yandexFoundTrue = sampleAddressResult
                .Where(x => x.YandexXCoor != null &&
                            Math.Round(Convert.ToDecimal(x.YandexXCoor.Replace('.', ',')), 2) ==
                            Math.Round(Convert.ToDecimal(x.ActualXCoor.Replace('.', ',')), 2) &&
                            Math.Round(Convert.ToDecimal(x.YandexYCoor.Replace('.', ',')), 2) ==
                            Math.Round(Convert.ToDecimal(x.ActualYCoor.Replace('.', ',')), 2)).ToList();

            List<TestResultModel> geoogleFoundTrue = sampleAddressResult
                .Where(x => x.GoogleXCoor != null &&
                            Math.Round(Convert.ToDecimal(x.GoogleXCoor.Replace('.', ',')), 2) ==
                            Math.Round(Convert.ToDecimal(x.ActualXCoor.Replace('.', ',')), 2) &&
                            Math.Round(Convert.ToDecimal(x.GoogleYCoor.Replace('.', ',')), 2) ==
                            Math.Round(Convert.ToDecimal(x.ActualYCoor.Replace('.', ',')), 2)).ToList();

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
            TimeSpan myTime = new TimeSpan();
            foreach (var item in myGeocoderFoundTrue)
            {
                TimeSpan date = TimeSpan.Parse(item.MyTime);
                myTime = myTime.Add(date);
            }

            long myTicks = myTime.Ticks / myGeocoderFoundTrue.Count;
            TimeSpan myTimeSpan = new TimeSpan(myTicks);  // MyGeocoder

            TimeSpan yandexTime = new TimeSpan();
            foreach (var item in yandexFoundTrue)
            {
                TimeSpan date = TimeSpan.Parse(item.YandexTime);
                yandexTime = yandexTime.Add(date);
            }

            long yandexTicks = yandexTime.Ticks / myGeocoderFoundTrue.Count;
            TimeSpan yandexTimeSpan = new TimeSpan(yandexTicks);  //YandexGeocoder

            TimeSpan googleTime = new TimeSpan();
            foreach (var item in geoogleFoundTrue)
            {
                TimeSpan date = TimeSpan.Parse(item.GoogleTime);
                googleTime = googleTime.Add(date);
            }

            long googleTicks = googleTime.Ticks / myGeocoderFoundTrue.Count;
            TimeSpan googleTimeSpan = new TimeSpan(googleTicks);  // GoogleGeocoder


        }
    }
}
