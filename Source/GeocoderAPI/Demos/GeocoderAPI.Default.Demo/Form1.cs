using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GeocoderAPI.Core;
using GeocoderAPI.DAL;
using GeocoderAPI.Model;

namespace GeocoderAPI.Default.Demo
{
    public partial class Form1 : Form
    {
        private readonly GeocoderService geocoderService;
       // private readonly Geocoder geocoder;
        private readonly Parse parse;
        private readonly Parsing parsing;

        public Form1()
        {
            InitializeComponent();
            geocoderService = new GeocoderService();
    //        geocoder = new Geocoder();
            parse = new Parse();
            parsing = new Parsing();
            //ProcTest();
            //button1_Click(null, null);
        }

        private void ProcTest()
        {
            geocoderService.GetUnitSearchDataByIlAndIlceId("DUZLERKOYU", 57000014000);
        }

        private List<string> CheckForTown(IEnumerable<string> notParsedList, ref AddressLevel addressLevel)
        {
            List<string> result = new List<string>();
            decimal ilId = addressLevel.IlId;

            foreach (var item in notParsedList)
            {
                var town = geocoderService.GetTownByNameAndCityId(item.Trim(), addressLevel.IlId);

                if (town != null)
                    addressLevel.Ilçe = item.Trim();
                else
                    result.Add(item);
            }

            return result;
        }

        private List<string> CheckForCity(IEnumerable<string> notParsedList, ref AddressLevel addressLevel)
        {
            List<string> result = new List<string>();

            foreach (var item in notParsedList)
            {
                var city = geocoderService.GetCityByName(item.Trim());

                if (city != null)
                {
                    addressLevel.Il = item.Trim();
                    addressLevel.IlId = city.IL_ID;
                }
                else
                    result.Add(item);
            }
            
            return result;
        }

        private void GeocoderTest(AddressLevel addressLevel)
        {
      //      geocoder.Geocode(addressLevel);
        }

        private string FixerTest(string address)
        {
            string preparedAddress = Fixer.Prepare(address);
            return preparedAddress;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (textBox1.Text.Trim().Equals(string.Empty))
            //{
            //    MessageBox.Show("Lütfen bir adres giriniz!");
            //    return;
            //}

            string address = textBox1.Text.Trim();

            address = "inönü mahallesi ilköğretmen caddesi birlik apt. no:55 daire 7 istanbul ataşehir";
            string fixerTest = FixerTest(address);

            AddressLevel addressLevel = parse.ParseAddress(fixerTest);
            List<string> list = parse.NotParsedList;

            list = CheckForCity(list, ref addressLevel);

            if (!addressLevel.Il.Equals(string.Empty))
            {
                list = CheckForTown(list, ref addressLevel);
            }

            addressLevel = parsing.IntegrationParsing(addressLevel);

            GeocoderTest(addressLevel);
        }
    }
}
