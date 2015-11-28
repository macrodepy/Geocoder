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
        private readonly Parser parser;
        private readonly Geocoder geocoder;

        public Form1()
        {
            InitializeComponent();
            geocoderService = new GeocoderService();
            parser = new Parser();
            geocoder = new Geocoder();
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

        private string FixerTest(string address)
        {
            string preparedAddress = Fixer.Prepare(address);
            return preparedAddress;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Lütfen bir adres giriniz!");
                return;
            }

            string address = textBox1.Text.Trim();
            string fixedAddress = FixerTest(address);

            AddressLevel addressLevel = parser.ParseAddress(fixedAddress);
            List<string> list = parser.NotParsedList;

            list = CheckForCity(list, ref addressLevel);

            if (!addressLevel.Il.Equals(string.Empty))
            {
                list = CheckForTown(list, ref addressLevel);
            }

            addressLevel = geocoder.IntegrationParsing(addressLevel);

            FillScreen(addressLevel);
        }

        private void FillScreen(AddressLevel addressLevel)
        {
            if (addressLevel.XCoor == string.Empty || addressLevel.XCoor == "0")
            {
                MessageBox.Show("Adres bulunamadı");
                return;
            }

            lblIl.Text = addressLevel.Il;
            lblIlce.Text = addressLevel.Ilçe;
            lblMahalle.Text = addressLevel.Mahalle;
            lblCadde.Text = addressLevel.Cadde;
            lblSokak.Text = addressLevel.Sokak;
            lblBulvar.Text = addressLevel.Bulvar;
            lblPoi.Text = addressLevel.Poi;
            lblBina.Text = addressLevel.Bina;
            lblBlok.Text = addressLevel.Blok;
            lblDaire.Text = addressLevel.Daire;
            lblKapı.Text = addressLevel.Kapı;
            lblKat.Text = addressLevel.Kat;
            lblKöy.Text = addressLevel.Köy;
            lblXcoor.Text = addressLevel.XCoor;
            lblYcoor.Text = addressLevel.YCoor;
            lblLevel.Text = GetLevel(addressLevel);

            LoadMap(addressLevel);

        }

        private string GetLevel(AddressLevel addressLevel)
        {
            switch (addressLevel.CoordinateLevel)
            {
                case 0:
                    return "İL";
                case 1:
                    return "İLÇE";
                case 2:
                    return "MAHALLE";
                case 3:
                    return "CADDE";
                case 4:
                    return "SOKAK";
                case 5:
                    return "POI";
                case 6:
                    return "KAPI";
                default:
                    return "";
            }
        }

        private void LoadMap(AddressLevel addressLevel)
        {
            string geocoderSampleName = "Sample";

            //string url = string.Format("https://www.google.com/maps/place//@{0},{1},15z", addressLevel.YCoor, addressLevel.XCoor);
            string url = string.Format("http://www.openstreetmap.org/?mlat={0}&mlon={1}#map=19/{0}/{1}", addressLevel.YCoor, addressLevel.XCoor);
            //string url = string.Format("http://www.bing.com/maps/default.aspx?sp=point.{0}_{1}_{2}_{3}_{4}_{5}",
            //    addressLevel.YCoor,
            //    addressLevel.XCoor,
            //    "MUSTAFA", //title, 
            //    "NOT", //note,
            //    "www.google.com", //linkUrl,
            //    "");  //photoUrl);

            webBrowser1.Url = new Uri(url);
            webBrowser1.Show();
        }
    }
}
