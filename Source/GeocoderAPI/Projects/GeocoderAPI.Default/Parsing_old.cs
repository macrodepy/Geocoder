using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Aras.Module.Address.LegacyParser.Default
{
    public class Parsing : IParsing
    {
        public enum ParsingAdress { Il, Ilçe, Mahalle, MahalleId, Cadde, Sokak, POI, PostaKodu, BinaAdi, Blok, Daire, Kapı, Kat, Unitid, xcoor, ycoor, TespitSeviyesi };  //Adres değişkenlerinin listesi

        public string[] addressParsing = new string[17];
        private IList<V_Unit_Search> cross;
        private IList<Kapi> kapiList; 
        public enum VeriTipi { Mahalle, Cadde, Sokak, POI };  //Adres değişkenlerinin listesi

        int TespitSeviyesi;
        int VeriTipiId = -1;

        string orginalAddress = string.Empty;
        string pCustomerName = string.Empty;
        string pCityCode = string.Empty;
        string pCityName = string.Empty;
        string pTownName = string.Empty;
        string pDistrict = string.Empty;
        string pField = string.Empty;
        string pQuarter = string.Empty;
        string pAvenue = string.Empty;
        string pAvenue_1 = string.Empty;
        string pAvenue_2 = string.Empty;
        string pAvenue_S = string.Empty;
        string pStreet = string.Empty;
        string pStreet_1 = string.Empty;
        string pStreet_2 = string.Empty;
        string pStreet_S = string.Empty;
        string pPostCode = string.Empty;
        string pBuildNo = string.Empty;
        string pBuildName = string.Empty;
        string pBlock = string.Empty;
        string pDoorNumber = string.Empty;
        string pTaxOffice = string.Empty;
        string pTaxNumber = string.Empty;
        string pPhone1 = string.Empty;
        string pCustomerCode = string.Empty;
        string pField_1 = string.Empty;
        string pField_2 = string.Empty;
        string pField_3 = string.Empty;
        string pField_S = string.Empty;
        string pBulvar = string.Empty;
        string pBulvar_1 = string.Empty;
        string pBulvar_2 = string.Empty;
        string pBulvar_S = string.Empty;
        string pKöy = string.Empty;
        string pYolu = string.Empty;
        string pKat = string.Empty;
        string pBlok = string.Empty;
        string textBoxMobilePhone = string.Empty;
        string textBoxPhone1 = string.Empty;

        static string[,] hierarchy;

        long CityId = 0;
        long TownId = 0;

        string xcoor = "0";
        string ycoor = "0";

        long yol_id;
        long mahalle_id;
        long poi_id;

        DataTable table = new DataTable();

        bool IntegrationSave = false;


        private readonly IAddressService parsingService;
        private readonly IDao<Unit> unitService;
        private readonly IVUnitSearchService vUnitSearchService;
        private readonly IProcService procService;


        public Parsing(IAddressService parsingService, IDao<Unit> unitService,IVUnitSearchService vUnitSearchService, IProcService procService)
        {
            this.parsingService = parsingService;
            this.unitService = unitService;
            this.vUnitSearchService = vUnitSearchService;
            this.procService = procService;
        }

        public string[] IntegrationParsing(ref string[] enableControl, String cityName, String townName, String addressString)
        {
            Cleanup();

            orginalAddress = addressString;

            Destination.Default.LegacyParser.MapAddressParser adp = new Destination.Default.LegacyParser.MapAddressParser();
            adp.mAddress = addressString;

            System.Globalization.CultureInfo dil = new System.Globalization.CultureInfo("tr-TR");

            cityName = cityName.ToUpper(dil);
            cityName = cityName.Replace("AFYON", "AFYONKARAHİSAR");
            townName = townName.ToUpper(dil);
            townName = townName.Replace("SAMANDAĞI", "SAMANDAĞ");

            pCityName = cityName;
            pTownName = townName;

            //Parse edilen Mahalle Bilgisi Alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Mahalle] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Mahalle].ToString() != "")
                    pQuarter = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Mahalle].ToString();

            //Parse edilen Cadde bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde].ToString() != "")
                    pAvenue = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde].ToString();

            //Parse edilen Cadde bilgisi alınıyor (Parça 1)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_1] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_1].ToString() != "")
                    pAvenue_1 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_1].ToString();

            //Parse edilen Cadde bilgisi alınıyor (Parça 2)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_2] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_2].ToString() != "")
                    pAvenue_2 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_2].ToString();

            //Parse edilen Cadde bilgisi alınıyor (Parçanın geri kalanı)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_S] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_S].ToString() != "")
                    pAvenue_S = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Cadde_S].ToString();

            //Parse edilen Bulvar bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar].ToString() != "")
                    pBulvar = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar].ToString();

            //Parse edilen Bulvar bilgisi alınıyor (Parça 1)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_1] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_1].ToString() != "")
                    pBulvar_1 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_1].ToString();

            //Parse edilen Bulvar bilgisi alınıyor (Parça 2)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_2] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_2].ToString() != "")
                    pBulvar_2 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_2].ToString();

            //Parse edilen Bulvar bilgisi alınıyor (Parçanın geri kalanı)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_S] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_S].ToString() != "")
                    pBulvar_S = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bulvar_S].ToString();

            //Parse edilen Köy bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Köy] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Köy].ToString() != "")
                    pKöy = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Köy].ToString();

            //Parse edilen Sokak bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak].ToString() != "")
                    pStreet = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak].ToString();

            //Parse edilen Sokak bilgisi alınıyor (Parça 1)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_1] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_1].ToString() != "")
                    pStreet_1 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_1].ToString();

            //Parse edilen Sokak bilgisi alınıyor (Parça 2)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_2] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_2].ToString() != "")
                    pStreet_2 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_2].ToString();

            //Parse edilen Sokak bilgisi alınıyor (Parçanın geri kalanı)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_S] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_S].ToString() != "")
                    pStreet_S = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Sokak_S].ToString();

            //Parse edilen POI bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI].ToString() != "")
                    pField = adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI].ToString();

            //Parse edilen POI bilgisi alınıyor (Parça 1)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_1] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_1].ToString() != "")
                    pField_1 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_1].ToString();

            //Parse edilen POI bilgisi alınıyor (Parça 2)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_2] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_2].ToString() != "")
                    pField_2 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_2].ToString();

            //Parse edilen POI bilgisi alınıyor (Parça 3)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_3] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_3].ToString() != "")
                    pField_3 = adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_3].ToString();

            //Parse edilen POI bilgisi alınıyor (Parçanın geri kalanı)
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_S] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_S].ToString() != "")
                    pField_S = adp.addressParsing[(int)MapAddressParser.ParsingAdress.POI_S].ToString();

            //Parse edilen KapıNumarası bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Kapı] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Kapı].ToString() != "")
                    pBuildNo = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Kapı].ToString();

            //Parse edilen Binaİsmi bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bina] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bina].ToString() != "")
                    pBuildName = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bina].ToString();

            //Parse edilen Yolu bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Yolu] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Yolu].ToString() != "")
                    pYolu = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Yolu].ToString();

            //Parse edilen Kat bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Kat] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Kat].ToString() != "")
                    pKat = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Kat].ToString();

            //Parse edilen Kat bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Blok] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Blok].ToString() != "")
                    pBlok = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Blok].ToString();

            //Parse edilen Kat bilgisi alınıyor
            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Daire] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Daire].ToString() != "")
                    pDoorNumber = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Daire].ToString();
            try
            {
                AddresGeocode();
            }
            catch(Exception ex)
            {
                //throw ex;
            }

            #region Adres tamamlama
            //(varış merkezi tespit edildikten sonra adres parserden gelen diğer veriler var ise doldurulur)

            if (string.IsNullOrEmpty(addressParsing[(int)ParsingAdress.Mahalle]))  //Eğer mahalle bilgisi seçilmedi ise
            {
                if (pQuarter != "")  // ve mahalle bilgisi parser'dan geldi ise
                {
                    addressParsing[(int)ParsingAdress.Mahalle] = pQuarter.ToUpper(dil);  //Mahalle bilgisini Büyük Harfe çevir ve mahalle textbox'ına ekle
                    enableControl[(int)VeriTipi.Mahalle] = "false";  //textbox'ın enable'ını kontrol için
                }
            }
            else
            {
                enableControl[(int)VeriTipi.Mahalle] = "true";  //textbox'ın enable'ını kontrol için
            }

            if (string.IsNullOrEmpty(addressParsing[(int)ParsingAdress.Cadde])) //Eğer Cadde bilgisi seçilmedi ise
            {
                if (pAvenue != "") // ve cadde bilgisi parser'dan geldi ise
                {
                    addressParsing[(int)ParsingAdress.Cadde] = pAvenue.ToUpper(dil); //cadde bilgisini Büyük Harfe çevir ve mahalle textbox'ına ekle                        
                }
                else if (pYolu != "")
                {
                    addressParsing[(int)ParsingAdress.Cadde] = pYolu.ToUpper(dil); //cadde bilgisini Büyük Harfe çevir ve mahalle textbox'ına ekle    
                    enableControl[(int)VeriTipi.Cadde] = "false";  //textbox'ın enable'ını kontrol için
                }
            }
            else
            {
                enableControl[(int)VeriTipi.Cadde] = "true";  //textbox'ın enable'ını kontrol için
            }

            if (string.IsNullOrEmpty(addressParsing[(int)ParsingAdress.Sokak])) //Eğer Sokak bilgisi seçilmedi ise
            {
                if (pStreet != "") // ve sokak bilgisi parser'dan geldi ise
                {
                    addressParsing[(int)ParsingAdress.Sokak] = pStreet.ToUpper(dil); //sokak bilgisini Büyük Harfe çevir ve mahalle textbox'ına ekle                                            
                    enableControl[(int)VeriTipi.Sokak] = "false";  //textbox'ın enable'ını kontrol için
                }
            }
            else
            {
                enableControl[(int)VeriTipi.Sokak] = "true";  //textbox'ın enable'ını kontrol için
            }

            if (string.IsNullOrEmpty(addressParsing[(int)ParsingAdress.POI])) //Eğer POI bilgisi seçilmedi ise
            {
                if (pField != "") // ve sokak bilgisi parser'dan geldi ise
                {
                    addressParsing[(int)ParsingAdress.POI] = pField.ToUpper(dil); //sokak bilgisini Büyük Harfe çevir ve mahalle textbox'ına ekle                                            
                    enableControl[(int)VeriTipi.POI] = "false";  //textbox'ın enable'ını kontrol için
                }
            }
            else
            {
                enableControl[(int)VeriTipi.POI] = "true";  //textbox'ın enable'ını kontrol için
            }

            if (string.IsNullOrEmpty(addressParsing[(int)ParsingAdress.Kapı]))
            {
                if (pBuildNo.ToString() != "")
                {
                    addressParsing[(int)ParsingAdress.Kapı] = pBuildNo.ToString().Trim();
                }
            }

            if (pBlok.ToString().ToUpper(dil).Length > 8)
            {
                addressParsing[(int)ParsingAdress.Blok] = pBlok.ToString().ToUpper(dil).Substring(0, 8);
            }
            else
            {
                addressParsing[(int)ParsingAdress.Blok] = pBlok.ToString().ToUpper(dil);
            }

            try
            {
                addressParsing[(int)ParsingAdress.Daire] = pDoorNumber;
            }
            catch (Exception)
            {
                //
            }

            addressParsing[(int)ParsingAdress.BinaAdi] += (pBuildName.ToString() != "") ? pBuildName.ToString().ToUpper(dil) + " " : ""; //parserdan gelen Bina ismini büyük harfe çevir ve bina ismi textbox'ına yaz
            addressParsing[(int)ParsingAdress.BinaAdi] += (pKat.ToString() != "") ? "K:" + pKat.ToString() : "";  //Kat bilgisi var ise bina ismi ile birleştir

            #endregion

            if (table != null)
            {
                if (table.Rows.Count == 1)
                {
                    addressParsing[(int) ParsingAdress.Unitid] = table.Rows.Count > 0
                        ? table.Rows[0]["UNITID"].ToString()
                        : "";
                    addressParsing[(int) ParsingAdress.xcoor] = xcoor != null ? xcoor : "";
                    addressParsing[(int) ParsingAdress.ycoor] = ycoor != null ? ycoor : "";
                    addressParsing[(int) ParsingAdress.TespitSeviyesi] = TespitSeviyesi != null
                        ? TespitSeviyesi.ToString()
                        : "";
                }
            }

            return addressParsing;
        }

        private void Cleanup()
        {
            addressParsing = new string[17];
            cross = null;
            kapiList = null; 
            TespitSeviyesi = 0;
            VeriTipiId = -1;

            orginalAddress = string.Empty;
            pCustomerName = string.Empty;
            pCityCode = string.Empty;
            pCityName = string.Empty;
            pTownName = string.Empty;
            pDistrict = string.Empty;
            pField = string.Empty;
            pQuarter = string.Empty;
            pAvenue = string.Empty;
            pAvenue_1 = string.Empty;
            pAvenue_2 = string.Empty;
            pAvenue_S = string.Empty;
            pStreet = string.Empty;
            pStreet_1 = string.Empty;
            pStreet_2 = string.Empty;
            pStreet_S = string.Empty;
            pPostCode = string.Empty;
            pBuildNo = string.Empty;
            pBuildName = string.Empty;
            pBlock = string.Empty;
            pDoorNumber = string.Empty;
            pTaxOffice = string.Empty;
            pTaxNumber = string.Empty;
            pPhone1 = string.Empty;
            pCustomerCode = string.Empty;
            pField_1 = string.Empty;
            pField_2 = string.Empty;
            pField_3 = string.Empty;
            pField_S = string.Empty;
            pBulvar = string.Empty;
            pBulvar_1 = string.Empty;
            pBulvar_2 = string.Empty;
            pBulvar_S = string.Empty;
            pKöy = string.Empty;
            pYolu = string.Empty;
            pKat = string.Empty;
            pBlok = string.Empty;
            textBoxMobilePhone = string.Empty;
            textBoxPhone1 = string.Empty;

            hierarchy = new string[,] {};

            CityId = 0;
            TownId = 0;

            xcoor = "0";
            ycoor = "0";

            yol_id = 0;
            mahalle_id = 0;
            poi_id = 0;

            table = new DataTable();

            IntegrationSave = false;        
        }

        public void AddresGeocode()
        {
            //HinterlandMapProxy mappingProxy = new HinterlandMapProxy();
            System.Globalization.CultureInfo dil = new System.Globalization.CultureInfo("tr-TR");

            hierarchy = new string[7, 2];

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    hierarchy[i, j] = "9999999";
                }
            }

            //VeriTabanından (AccountAddress) il ismi ile bilgi alma
            //HinterlandPOIData.HINTCITYGEOCITYCRDataTable hintCityGeoCityCR = new HinterlandMapProxy().GetIL_IDDataByHintCityGeoCityCR(pCityName.ToUpper(dil).Trim());
            IList<HintCityGeoCityCr> ilIdDataByHintCityGeoCityCr = parsingService.GetIL_IDDataByHintCityGeoCityCR(pCityName.ToUpper(dil).Trim()).ToList();

            int flag = 0;

            if (ilIdDataByHintCityGeoCityCr.Count > 0)
            {
                var hintCityGeoCity = ilIdDataByHintCityGeoCityCr.FirstOrDefault();
                if (hintCityGeoCity.GeolocIlId != null)  //tabloda geoloc_il_id adında kolon var ise
                {
                    CityId = hintCityGeoCity.GeolocIlId.Value;  //İl ID'si atanıyor
                    //addressInfoControlMap1.comboBoxCity_Validating(addressInfoControlMap1.comboBoxCity, null); //bulunan il_id'sini seçtirdikten sonra validating event'ını tetikliyoruz 

                    addressParsing[(int)ParsingAdress.Il] = CityId.ToString();

                    #region İl Seçilirse İl'in koordinat bilgisi aktarılır
                    //HinterlandPOIData.ILDataTable dtIl = new HinterlandMapProxy().GetIlDataByIL_ID(CityId);
                    var il = parsingService.GetIlDataByIL_ID(CityId).FirstOrDefault();

                    //HinterlandPOIData.GEOLOC_IL_TMPDataTable dtIl = new MappingProxy().GetDataByIlId((decimal)comboBoxCity.SelectedValue, Esas.Shared.Proxy.WebServiceCallType.LOCAL);
                    if (il.XCoor != null || il.XCoor != "0")
                    {
                        hierarchy[0, 0] = il.XCoor;
                        hierarchy[0, 1] = il.YCoor;
                    }
                    else
                    {
                        hierarchy[0, 0] = "9999999";
                        hierarchy[0, 1] = "9999999";
                    }
                    #endregion

                    //VeriTabanından (AccountAddress) ilce ismi ile bilgi alma
                    //HinterlandPOIData.HINTTOWNGEOTOWNCRDataTable hintTownGeoTownCR = new HinterlandMapProxy().GetGeoloc_Ilce_IDDataByHINTTOWNGEOTOWNCR(pTownName.ToUpper(dil).Trim(), hintCityGeoCityCR[0].GEOLOC_IL_ID);

                    var hintTownGeoTownCrs = parsingService.GetGeoloc_Ilce_IDDataByHINTTOWNGEOTOWNCR(pTownName.ToUpper(dil).Trim(),
                        hintCityGeoCity.GeolocIlId.Value).ToList();


                    if (hintTownGeoTownCrs.Count > 0)  //Eğer İlçe bulunabildi ise
                    {
                        var hintTownGeoTown = hintTownGeoTownCrs.FirstOrDefault();
                        if (hintTownGeoTown.GeolocIlceId.Value != null)  //Tabloda geoloc_ilce_id adında kolon var ise
                        {
                            TownId = hintTownGeoTown.GeolocIlceId.Value;  //İlçe Id'si combodan seçiliyor
                            //addressInfoControlMap1.comboBoxTown_Validating(addressInfoControlMap1.comboBoxTown, null); //bulunan ilce_id'sini seçtirdikten sonra validating event'ını tetikliyoruz  

                            addressParsing[(int)ParsingAdress.Ilçe] = TownId.ToString();

                            #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                            //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                            var ilce = parsingService.GetIlceDataByILCE_ID(TownId).FirstOrDefault();

                            if (ilce.XCoor != "0")
                            {
                                hierarchy[1, 0] = ilce.XCoor;
                                hierarchy[1, 1] = ilce.YCoor;
                            }
                            else
                            {
                                hierarchy[1, 0] = "9999999";
                                hierarchy[1, 1] = "9999999";
                            }
                            #endregion

                            //VMTespit();  //İlçeden tespit var mı?

                            //if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                            //{
                            //    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                            //    IntegrationSave = true;
                            //}

                            if (!IntegrationSave)
                            {
                                while (true)
                                {
                                    string searchText = "999999999";

                                    searchText = Mapaddressconcat(flag);

                                    if (searchText == "888888888")
                                    {
                                        //adres parse'tan gelen veri ile tespit yapılamadıysa işlemi bitir.
                                        if (TownId != 0)
                                        {
                                            VMTespit();  //İlçeden tespit var mı?

                                            if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                            {
                                                //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                                IntegrationSave = true;
                                            }
                                        }

                                        break;
                                    }

                                    if (searchText == "999999999")  //Eğer birşey dönmedi ise döngüye devam et
                                    {
                                        flag += 1;
                                        continue;
                                    }

                                    //Mahalle/Yol/Birim'den dönen sonuçları dataTable'a aktarma
                                    string replaceText = SearchText(searchText.ToUpper(dil));
                                    //dtCross = new HinterlandMapProxy().GetUnitSearchDataByIlAndIlceId(replaceText, hintTownGeoTownCR[0].GEOLOC_ILCE_ID); //, Esas.Shared.Proxy.WebServiceCallType.LOCAL

                                    cross = vUnitSearchService.GetUnitSearchDataByIlAndIlceId(replaceText,
                                        hintTownGeoTown.GeolocIlceId.Value).ToList();

                                    if (cross != null && cross.Count > 0)  //Eğer veritabanında adres bulundu ise
                                    {
                                        if (cross.Count > 0 && cross.Count < 2) //Eğer adresin karşılığı tek kayıt ise
                                        {
                                            SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                            if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                            {
                                                //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                                IntegrationSave = true;
                                                break;
                                            }
                                            else
                                            {
                                                flag += 1;
                                            }
                                        }
                                        else if (cross.Count > 1)  //Eğer birden fazla kayıt dönerse tipe göre filtre kullan
                                        {
                                            switch (VeriTipiId)
                                            {
                                                case (int)VeriTipi.Mahalle:  //Parser'dan gelen bilgi mahalle ise mahallelerde ara
                                                    //dtCross.DefaultView.RowFilter = "MAHALLE_ID IS NOT NULL AND YOL_ID IS NULL AND POI_ID IS NULL";
                                                    cross = cross.Where(x => x.MahalleId != null && x.YolId == null && x.PoiId == null).ToList();
                                                    break;
                                                case (int)VeriTipi.Cadde:  //Parser'dan gelen bilgi Cadde ise caddelerde ara
                                                    //dtCross.DefaultView.RowFilter = "YOL_ID IS NOT NULL AND POI_ID IS NULL";
                                                    cross = cross.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                                    break;
                                                case (int)VeriTipi.Sokak:  //Parser'dan gelen bilgi Sokak ise sokaklarda ara
                                                    //dtCross.DefaultView.RowFilter = "YOL_ID IS NOT NULL AND POI_ID IS NULL";
                                                    cross = cross.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                                    break;
                                                case (int)VeriTipi.POI: //Parser'dan gelen bilgi POI ise POI'lerde ara
                                                    //dtCross.DefaultView.RowFilter = "YOL_ID IS NULL AND POI_ID IS NOT NULL";
                                                    cross = cross.Where(x => x.YolId == null && x.PoiId != null).ToList();

                                                    break;
                                            }

                                            if (cross.Count > 0 && cross.Count < 2) //Eğer adresin karşılığı tek kayıt ise
                                            {
                                                SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                                if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                                {
                                                    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                                    IntegrationSave = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    //address parser'dan gelen bir sonraki datayı işlemesi için
                                                    flag += 1;
                                                }
                                            }
                                            else if (cross.Count > 1)  //Eğer adresin karşılığı birden fazla kaıt ise
                                            {
                                                bool sameAddress = true;
                                               // string firstRecord = dtCross.DefaultView.ToTable().Rows[0]["SONUC_ILCELI"].ToString();  //Sonuç ilçeliden bakılmasının nedeni tüm kayıtarın birebir aynı olmasına bakılmasıdır
                                                string firstRecord = cross.FirstOrDefault().SonucIlceli;  //Sonuç ilçeliden bakılmasının nedeni tüm kayıtarın
                                                //Eğer tüm kayıtlar birbiri ile aynı ise ilk kaydı seç ve VM Tespiti yap
                                                for (int q = 1; q < cross.Count; q++)
                                                {
                                                    //if (firstRecord != dtCross.DefaultView.ToTable().Rows[q]["SONUC_ILCELI"].ToString())
                                                    //{
                                                    //    sameAddress = false;
                                                    //    break;
                                                    //}
                                                    if (cross.Any(search => firstRecord != search.SonucIlceli))
                                                    {
                                                        sameAddress = false;
                                                    }
                                                    
                                                }

                                                if (sameAddress)  //Eğer tüm kayıtlar birebir aynı ise 
                                                {
                                                    SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                                    if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                                    {
                                                        //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                                        IntegrationSave = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        //address parser'dan gelen bir sonraki datayı işlemesi için
                                                        flag += 1;
                                                    }
                                                }
                                                else
                                                {

                                                    flag += 1;
                                                }
                                            }
                                            else
                                            {
                                                //address parser'dan gelen bir sonraki datayı işlemesi için
                                                flag += 1;
                                            }

                                        }
                                        else
                                        {
                                            //address parser'dan gelen bir sonraki datayı işlemesi için
                                            flag += 1;
                                        }
                                    }
                                    else
                                    {
                                        //address parser'dan gelen bir sonraki datayı işlemesi için
                                        flag += 1;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            string searchText = "999999999";

                            searchText = Mapaddressconcat(flag);

                            if (searchText == "888888888")
                            {
                                //adres parse'tan gelen veri ile tespit yapılamadıysa işlemi bitir.
                                if (TownId != 0)
                                {
                                    VMTespit();  //İlçeden tespit var mı?

                                    if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                    {
                                        //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                        IntegrationSave = true;
                                    }
                                }
                                break;
                            }

                            if (searchText == "999999999")  //Eğer birşey dönmedi ise döngüye devam et
                            {
                                flag += 1;
                                continue;
                            }

                            //Mahalle/Yol/Birim'den dönen sonuçları dataTable'a aktarma
                            string replaceText = SearchText(searchText.ToUpper(dil));
                            //dtCross = new HinterlandMapProxy().GetUnitSearchDataByIlId(replaceText, hintCityGeoCityCR[0].GEOLOC_IL_ID);
                            cross = vUnitSearchService.GetUnitSearchDataByIlId(replaceText, hintCityGeoCity.GeolocIlId.Value).ToList();

                            if (cross != null && cross.Count > 0)
                            {
                                if (cross.Count > 0 && cross.Count < 2)
                                {
                                    #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                    if (!String.IsNullOrEmpty(cross.FirstOrDefault().IlceId.ToString()))
                                    {
                                        TownId = cross.FirstOrDefault().IlceId;

                                        addressParsing[(int)ParsingAdress.Ilçe] = TownId.ToString();

                                        //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                                        var ilce = parsingService.GetIlceDataByILCE_ID(TownId).FirstOrDefault();

                                        if (ilce.XCoor != "0")
                                        {
                                            hierarchy[1, 0] = ilce.XCoor;
                                            hierarchy[1, 1] = ilce.YCoor;
                                        }
                                        else
                                        {
                                            hierarchy[1, 0] = "9999999";
                                            hierarchy[1, 1] = "9999999";
                                        }
                                    }
                                    #endregion

                                    SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                    if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                    {
                                        //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                        IntegrationSave = true;

                                        break;
                                    }
                                    else
                                    {
                                        flag += 1;
                                    }
                                }
                                else if (cross.Count > 1)  //Eğer birden fazla kayıt dönerse tipe göre filtre kullan
                                {
                                    switch (VeriTipiId)
                                    {
                                        case (int)VeriTipi.Mahalle:
                                            //dtCross.DefaultView.RowFilter = "MAHALLE_ID IS NOT NULL AND YOL_ID IS NULL AND POI_ID IS NULL";
                                            cross = cross.Where(x => x.MahalleId != null && x.YolId == null && x.PoiId == null).ToList();
                                            break;
                                        case (int)VeriTipi.Cadde:
                                            //dtCross.DefaultView.RowFilter = "YOL_ID IS NOT NULL AND POI_ID IS NULL";
                                            cross = cross.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                            break;
                                        case (int)VeriTipi.Sokak:
                                            //dtCross.DefaultView.RowFilter = "YOL_ID IS NOT NULL AND POI_ID IS NULL";
                                            cross = cross.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                            break;
                                        case (int)VeriTipi.POI:
                                           // dtCross.DefaultView.RowFilter = "YOL_ID IS NULL AND POI_ID IS NOT NULL";
                                            cross = cross.Where(x => x.YolId == null && x.PoiId != null).ToList();
                                            break;
                                    }

                                    if (cross.Count > 0 && cross.Count < 2) //Eğer adresin karşılığı tek kayıt ise
                                    {
                                        #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                        if (!String.IsNullOrEmpty(cross.FirstOrDefault().IlceId.ToString()))
                                        {
                                            TownId = cross.FirstOrDefault().IlceId;

                                            addressParsing[(int)ParsingAdress.Ilçe] = TownId.ToString();

                                            //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                                            var ilce = parsingService.GetIlceDataByILCE_ID(TownId).FirstOrDefault();

                                            if (ilce.XCoor != "0")
                                            {
                                                hierarchy[1, 0] = ilce.XCoor;
                                                hierarchy[1, 1] = ilce.YCoor;
                                            }
                                            else
                                            {
                                                hierarchy[1, 0] = "9999999";
                                                hierarchy[1, 1] = "9999999";
                                            }

                                        }
                                        #endregion

                                        SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                        if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                        {
                                            //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                            IntegrationSave = true;
                                            break;
                                        }
                                        else
                                        {
                                            flag += 1;
                                        }
                                    }
                                    else if (cross.Count > 1)
                                    {

                                        bool sameAddress = true;
                                        string firstRecord = cross.FirstOrDefault().SonucIlceli;  //Sonuç ilçeliden bakılmasının nedeni tüm kayıtarın birebir aynı olmasına bakılmasıdır
                                        //Eğer tüm kayıtlar birbiri ile aynı ise ilk kaydı seç ve VM Tespiti yap
                                        //for (int q = 1; q < cross.Count; q++)
                                        //{
                                        //    if (firstRecord != dtCross.DefaultView.ToTable().Rows[q]["SONUC_ILCELI"].ToString())
                                        //    {
                                        //        sameAddress = false;
                                        //        break;
                                        //    }
                                        //}

                                        if (cross.Any(search => firstRecord != search.SonucIlceli))
                                        {
                                            sameAddress = false;
                                        }

                                        if (sameAddress)  //Eğer tüm kayıtlar birebir aynı ise 
                                        {

                                            #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                            if (!String.IsNullOrEmpty(cross.FirstOrDefault().IlceId.ToString()))
                                            {
                                                TownId = cross.FirstOrDefault().IlceId;

                                                addressParsing[(int)ParsingAdress.Ilçe] = TownId.ToString();

                                                //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                                                var ilce = parsingService.GetIlceDataByILCE_ID(TownId).FirstOrDefault();

                                                if (ilce.XCoor != "0")
                                                {
                                                    hierarchy[1, 0] = ilce.XCoor;
                                                    hierarchy[1, 1] = ilce.YCoor;
                                                }
                                                else
                                                {
                                                    hierarchy[1, 0] = "9999999";
                                                    hierarchy[1, 1] = "9999999";
                                                }

                                            }
                                            #endregion

                                            SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                            if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                            {
                                                //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                                IntegrationSave = true;
                                                break;
                                            }
                                            flag += 1;
                                        }
                                        else
                                        {

                                            flag += 1;
                                        }
                                    }
                                    else
                                    {
                                        flag += 1;
                                    }

                                }
                                else
                                {
                                    //address parser'dan gelen bir sonraki datayı işlemesi için
                                    flag += 1;
                                }
                            }
                            else
                            {
                                flag += 1;
                            }
                        }
                    }
                }
            }
        }

        //Balkar ATEŞ - 24.04.2013 - (Yeni MapParse ile) Müşteri Entegrasyonları için adres stringi birleştirme methodu
        protected string Mapaddressconcat(int flag)
        {
            //Aranacak adres string'i
            string returntext = "";
            VeriTipiId = -1;

            //Aramayı kaçıncı kez yapıyorsak ona göre aranacak string değişiyor
            switch (flag)
            {
                case 0:
                    if (pField.Trim() != "")
                    {
                        returntext = pField.Trim();
                        VeriTipiId = (int)VeriTipi.POI;
                    }
                    break;
                case 1:
                    if (pField.Trim() != "" && pBlok.Trim() != "")
                    {
                        returntext = pField.Trim();
                        returntext += pBlok.Trim();
                        VeriTipiId = (int)VeriTipi.POI;
                    }
                    break;
                case 2:
                    if (pQuarter.Trim() != "" && pField.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pField.Trim();
                        VeriTipiId = (int)VeriTipi.POI;
                    }
                    break;
                case 3:
                    if (pQuarter.Trim() != "" && pField.Trim() != "" && pBlok.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pField.Trim();
                        returntext += pBlok.Trim();
                        VeriTipiId = (int)VeriTipi.POI;
                    }
                    break;
                case 4:
                    //if (pField_1.Trim() != "" && pField_2.Trim() != "" && pField_3.Trim() != "")
                    //{
                    //    returntext = pField_1.Trim() + " ";
                    //    returntext += pField_2.Trim();
                    //    returntext += pField_3.Trim();
                    //    VeriTipiId = (int)VeriTipi.POI;
                    //}
                    break;
                case 5:
                    //if (pQuarter.Trim() != "" && pField_1.Trim() != "" && pField_2.Trim() != "" && pField_3.Trim() != "")
                    //{
                    //    returntext = pQuarter.Trim() + "MAHALLESİ";
                    //    returntext += pField_1.Trim() + " ";
                    //    returntext += pField_2.Trim();
                    //    returntext += pField_3.Trim();
                    //    VeriTipiId = (int)VeriTipi.POI;
                    //}
                    break;
                case 6:
                    //if (pField_1.Trim() != "" && pField_2.Trim() != "")
                    //{
                    //    returntext = pField_1.Trim() + " ";
                    //    returntext += pField_2.Trim();
                    //    VeriTipiId = (int)VeriTipi.POI;
                    //}
                    break;
                case 7:
                    //if (pQuarter.Trim() != "" && pField_1.Trim() != "" && pField_2.Trim() != "")
                    //{
                    //    returntext = pQuarter.Trim() + "MAHALLESİ";
                    //    returntext += pField_1.Trim() + " ";
                    //    returntext += pField_2.Trim();
                    //    VeriTipiId = (int)VeriTipi.POI;
                    //}
                    break;
                case 8:
                    //if (pField_1.Trim() != "")
                    //{
                    //    returntext = pField_1.Trim();
                    //    VeriTipiId = (int)VeriTipi.POI;
                    //}
                    break;
                case 9:
                    //if (pQuarter.Trim() != "" && pField_1.Trim() != "")
                    //{
                    //    returntext = pQuarter.Trim() + "MAHALLESİ";
                    //    returntext += pField_1.Trim();
                    //    VeriTipiId = (int)VeriTipi.POI;
                    //}
                    break;
                case 10:
                    if (pYolu.Trim() != "")
                    {
                        returntext = pYolu.Trim();
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 11:
                    if (pStreet.Trim() != "")
                    {
                        returntext = pStreet.Trim() + "SOKAK";
                        VeriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 12:
                    if (pStreet_1.Trim() != "" && pStreet_2.Trim() != "")
                    {
                        returntext = pStreet_1.Trim() + " ";
                        returntext += pStreet_2.Trim();
                        VeriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 13:
                    if (pAvenue.Trim() != "")
                    {
                        returntext = pAvenue.Trim() + "CADDESİ";
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 14:
                    if (pAvenue_1.Trim() != "" && pAvenue_2.Trim() != "")
                    {
                        returntext = pAvenue_1.Trim() + " ";
                        returntext += pAvenue_2.Trim();
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 15:
                    if (pBulvar.Trim() != "")
                    {
                        returntext = pBulvar.Trim() + "BULVARI";
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 16:
                    if (pBulvar_1.Trim() != "" && pBulvar_2.Trim() != "")
                    {
                        returntext = pBulvar_1.Trim() + " ";
                        returntext += pBulvar_2.Trim();
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 17:
                    if (pQuarter.Trim() != "" && pYolu.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pYolu.Trim();
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 18:
                    if (pQuarter.Trim() != "" && pStreet.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pStreet.Trim() + "SOKAK";
                        VeriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 19:
                    if (pQuarter.Trim() != "" && pStreet.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pStreet.Trim() + "%" + "SOKAK";
                        VeriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 20:
                    if (pQuarter.Trim() != "" && pStreet_1.Trim() != "" && pStreet_2.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pStreet_1.Trim() + " ";
                        returntext += pStreet_2.Trim();
                        VeriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 21:
                    if (pQuarter.Trim() != "" && pAvenue.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pAvenue.Trim() + "CADDESİ";
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 22:
                    if (pQuarter.Trim() != "" && pAvenue.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pAvenue.Trim() + "%" + "CADDESİ";
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;

                case 23:
                    if (pQuarter.Trim() != "" && pAvenue_1.Trim() != "" && pAvenue_2.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pAvenue_1.Trim() + " ";
                        returntext += pAvenue_2.Trim();
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 24:
                    if (pQuarter.Trim() != "" && pBulvar.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pBulvar.Trim() + "BULVARI";
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 25:
                    if (pQuarter.Trim() != "" && pBulvar.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pBulvar.Trim() + "%" + "BULVARI";
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 26:
                    if (pQuarter.Trim() != "" && pBulvar_1.Trim() != "" && pBulvar_2.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        returntext += pBulvar_1.Trim() + " ";
                        returntext += pBulvar_2.Trim();
                        VeriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 27:
                    if (pKöy.Trim() != "")
                    {
                        returntext = pKöy.Trim() + "KÖYÜ";
                        VeriTipiId = (int)VeriTipi.Mahalle;
                    }
                    break;
                case 28:
                    if (pQuarter.Trim() != "")
                    {
                        returntext = pQuarter.Trim() + "MAHALLESİ";
                        VeriTipiId = (int)VeriTipi.Mahalle;
                    }
                    break;
                default:
                    returntext = "888888888";
                    break;
            }


            if (returntext.Trim() == "")
            {
                returntext = "999999999";
            }

            return returntext;
        }

        private void SearchAddress()
        {
            yol_id = 0;
            mahalle_id = 0;
            poi_id = 0;
            var model = cross.FirstOrDefault();

            if (!String.IsNullOrEmpty(model.YolId.ToString())) //yol_ID'sini almak için
            {
                yol_id = (long) model.YolId.Value;
            }
            if (!String.IsNullOrEmpty(model.MahalleId.ToString())) //mahalle_id'sini almak için
            {
                mahalle_id = (long) model.MahalleId.Value;
            }
            if (!String.IsNullOrEmpty(model.PoiId.ToString())) //poi_id'sini almak için
            {
                poi_id = (long) model.PoiId.Value;
            }

            //Yol tablosuna göre kontrol mahalleleri kesen yollar için yanlış olduğu için bu kontrol boşa çıktı 
            //HinterlandPOIData.YOLDataTable dtYol = new HinterlandMapProxy().GetYolDataByMAHALLE_ID(yol_id);  //yolid'si yol tablosunda aranıyor

            #region Balkar ATEŞ - 04.01.2013 - İdari Sınır yol tablosuna göre yol kontrolü eklendi
            //HinterlandPOIData.YOL_IDARIDataTable dtIYol = new HinterlandMapProxy().GetIdariYolDataByMahalleAndYolID(yol_id, mahalle_id);  //yolid'si idari_sinir_yol tablosunda aranıyor 
            var idariYolSınırs = parsingService.GetIdariYolDataByMahalleAndYolId(yol_id, mahalle_id).ToList();
            #endregion

            //HinterlandPOIData.MAHALLEDataTable dtMahalle = new HinterlandMapProxy().GetMahalleDataByMAHALLE_ID(mahalle_id); //mahalleid'si mahalle tablosunda aranıyor
            var mahalles = parsingService.GetMahalleDataByMAHALLE_ID(mahalle_id).ToList();

            //HinterlandPOIData.POIDataTable dtField = new HinterlandMapProxy().GetPOIDataByPOI_ID(poi_id); //poiid'si poi tablosunda aranıyor
            var pois = parsingService.GetPOIDataByPOI_ID(poi_id).ToList();


            if (mahalles.Count > 0)
            {
                var mahalle = mahalles.FirstOrDefault();
                if (mahalle.MahalleAdi.IndexOf("MAHALLESİ") != -1)
                {
                    addressParsing[(int)ParsingAdress.Mahalle] = mahalle.MahalleAdi.Replace(" MAHALLESİ", "");   //mahalle bilgisi
                    addressParsing[(int)ParsingAdress.MahalleId] = mahalle.MahalleId.ToString();
                }
                else
                {
                    addressParsing[(int)ParsingAdress.Mahalle] = mahalle.MahalleAdi;   //mahalle bilgisi
                    addressParsing[(int)ParsingAdress.MahalleId] = mahalle.MahalleId.ToString();
                }


                #region Mahalle Var ise Koordinat Bilgisini almak için
                //en küçük hangi hiyerarşi bulundu ise onun ismini ekrana yazdırmak için
                //mahallenin x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    

                if (mahalle.XCoor != "0")
                {
                    hierarchy[2, 0] = mahalle.XCoor;
                    hierarchy[2, 1] = mahalle.YCoor;
                }
                else
                {
                    hierarchy[2, 0] = "9999999";
                    hierarchy[2, 1] = "9999999";
                }
                #endregion


                if (idariYolSınırs.Count > 0)
                {
                    var idariSınırYol = idariYolSınırs.FirstOrDefault();
                    if (idariSınırYol.YolSınıfı.ToString() == "2")
                    {
                        if (idariSınırYol.YolAdı.IndexOf("SOKAK") != -1)
                        {
                            addressParsing[(int)ParsingAdress.Sokak] = idariSınırYol.YolAdı.Replace(" SOKAK", "");  //Sokak Bilgisi
                        }
                        else
                        {
                            addressParsing[(int) ParsingAdress.Sokak] = idariSınırYol.YolAdı; //Sokak Bilgisi
                        }

                        #region Sokak var ise koordinat bilgisini almak için
                        //en küçük hangi hiyerarşi bulundu ise onun ismini ekrana yazdırmak için
                        //sokağın x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    

                        if (idariSınırYol.XCoor != "0")
                        {
                            hierarchy[4, 0] = idariSınırYol.XCoor;
                            hierarchy[4, 1] = idariSınırYol.YCoor;
                        }
                        else
                        {
                            hierarchy[4, 0] = "9999999";
                            hierarchy[4, 1] = "9999999";
                        }
                        #endregion
                    }
                    else
                    {
                        if (idariSınırYol.YolAdı.IndexOf("CADDESİ") != -1)
                        {
                            addressParsing[(int)ParsingAdress.Cadde] = idariSınırYol.YolAdı.Replace(" CADDESİ", ""); //Cadde Bilgisi
                        }
                        else
                        {
                            addressParsing[(int)ParsingAdress.Cadde] = idariSınırYol.YolAdı; //Cadde Bilgisi
                        }

                        #region Cadde var ise koordinat bilgisini almak için
                        //en küçük hangi hiyerarşi bulundu ise onun ismini ekrana yazdırmak için
                        //caddenin x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    

                        if (idariSınırYol.XCoor != "0")
                        {
                            hierarchy[3, 0] = idariSınırYol.XCoor;
                            hierarchy[3, 1] = idariSınırYol.YCoor;
                        }
                        else
                        {
                            hierarchy[3, 0] = "9999999";
                            hierarchy[3, 1] = "9999999";
                        }
                        #endregion
                    }

                    if (!string.IsNullOrEmpty(addressParsing[(int)ParsingAdress.Cadde]) || !string.IsNullOrEmpty(addressParsing[(int)ParsingAdress.Sokak])) //eğer cadde veya sokak bilgisi girildi ise
                    {
                        //cadde veya sokağın yolid'si kapı tablosunda aranıyor
                        //HinterlandPOIData.KAPIDataTable dtDoor = new HinterlandMapProxy().GetKapiDataByYOL_IDAndMAHALLE_ID(mahalle_id, yol_id);
                        kapiList = parsingService.GetKapiDataByYOL_IDAndMAHALLE_ID(mahalle_id, yol_id);

                        //cadde, sokağın yol_id'si kapı tablosunda var ise    
                        if (kapiList.Count > 0)
                        {
                            //mutlaka kapı no seçilmelidir combobox olsun
                            //(mah. yol_id kullanarak aranacak)  
                            //dtDoor.DefaultView.RowFilter = "KAPI_NO IS NOT NULL";  //kapı numraları girilmemiş alanların kapı numaralarını getirmemek için filtreleme yapılmaktadır
                            kapiList = kapiList.Where(x => x.KapiNo != null).ToList();
                            //dtKapi = dtDoor.DefaultView.ToTable();  //filtrelenmiş ve sıralı hali combonun içerisine yüklenmektedir - 05.11.2012

                            //en küçük hangi hiyerarşi bulundu ise onun ismini ekrana yazdırmak için
                            //kapının x - y koordinatları burada boş geçilmektedir. (amaç: kapı seçildikten sonra seçilen kapının koordinat bilgisini almak için) (kapı combobox'ının selectedtextchange olayında görülebilir)
                        }
                    }
                }
            }

            if (pois.Count > 0)  //(birim) POI bilgisi 
            {
                var poi = pois.FirstOrDefault();
                addressParsing[(int) ParsingAdress.POI] = poi.StandartName;

                #region POI varsa koordinat bilgisini almak için
                //en küçük hangi hiyerarşi bulundu ise onun ismini ekrana yazdırmak için
                //POI'nin x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    

                if (poi.XCoor != "0")
                {
                    hierarchy[5, 0] = poi.XCoor;
                    hierarchy[5, 1] = poi.YCoor;
                }
                else
                {
                    hierarchy[5, 0] = "9999999";
                    hierarchy[5, 1] = "9999999";
                }
                #endregion
            }

            if (pois.Count > 0)
            {
                VMTespit();
            }
            else
            {
                if (pBuildNo.Trim() != "")  //Eğer parser'dan kapı numarası geldi ise
                {
                    addressParsing[(int)ParsingAdress.Kapı] = pBuildNo.Trim();

                    if (kapiList != null)
                    {
                        searchDoorNumber();

                        VMTespit();
                    }
                    else
                    {
                        VMTespit();
                    }
                }
                else //Eğer kapı numarası parser'dan gelmedi ise
                {
                    //Eğer kapı numarası gelmedi ise ve tüm kapılar aynı şubeyi veriyor ise o şubeyi seç
                    if (kapiList != null)
                    {
                        int KapiCount = kapiList.Count;
                        var kapi = kapiList.FirstOrDefault();

                        if (KapiCount == 1)  //Eğer sistemde girilen yol altında bir adet kapı var ise ve parser'dan kapı bilgisi gelmedi ise
                        {
                            //Tek Kapı numarası olduğu için 1 yaz tespit yaptır
                            addressParsing[(int)ParsingAdress.Kapı] = kapi.KapiNo;

                            searchDoorNumber();

                            VMTespit();
                        }
                        else if (KapiCount > 1)
                        {
                            decimal kapiUnitid;
                            
                            //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(dtKapi.Rows[0]["XCOOR"].ToString(), dtKapi.Rows[0]["YCOOR"].ToString(), 3);
                            IList<PolygonList> polygonUnitLists = new List<PolygonList>();
                            if (kapi.XCoor != null)
                                polygonUnitLists = parsingService.GetXyCoorAndPolygonTypeId(kapi.XCoor, kapi.YCoor, 3);

                            if (polygonUnitLists.Count > 0)  //Eğer Caddenin denk geldiği şube polygonu var ise
                            {
                                var polygonUnit = polygonUnitLists.FirstOrDefault();
                                kapiUnitid = Convert.ToDecimal(polygonUnit.Unit.UnitId);
                                bool kapiTespit = true;
                                for (int i = 1; i < KapiCount; i++)
                                {
                                    //TODO yanlış yazılmış olabilir
                                    //polygonunit2 = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(dtKapi.Rows[i]["XCOOR"].ToString(), dtKapi.Rows[i]["YCOOR"].ToString(), 3);
                                     IList<PolygonList> polygonUnit2Lists = new List<PolygonList>();
                                     if (kapi.XCoor != null)
                                       polygonUnit2Lists = parsingService.GetXyCoorAndPolygonTypeId(kapi.XCoor, kapi.YCoor, 3);

                                    if (polygonUnitLists.Count > 0)  //Eğer Caddenin denk geldiği şube polygonu var ise
                                    {
                                        if (kapiUnitid != Convert.ToDecimal(polygonUnit.Unit.UnitId))
                                        {
                                            kapiTespit = false;
                                            break;
                                        }
                                    }
                                }

                                if (kapiTespit)  //Eğer parser'dan kapı bilgisi gelmedi ise ve tüm kapı numaraları aynı şubeyi veriyor ise 
                                {
                                    addressParsing[(int)ParsingAdress.Kapı] = "1";

                                    searchDoorNumber();

                                    VMTespit();  //Parser'dan kapı bilgisi gelmedi fakat yol üzerindeki tüm kapılar tek şube veriyor ise yoldan VM Tespiti yapılır
                                }
                            }
                        }
                    }
                    else
                    {
                        VMTespit();
                    }
                }
            }
        }

        public void searchDoorNumber()
        {
            if (addressParsing[(int)ParsingAdress.Kapı].Trim() != "")
            {
                int counter = 0;  //el ile girilen kapı numarasının kapı combobox'ının içindeki bir kapı numarasına denk gelip gelmediğini anlamak için

                //for (int i = 0; i < kapiList.Count; i++)  //kapı combosunun içerisindeki kapı numaralarında geziniyoruz
                //{
                //    if (dtKapi.Rows[i]["KAPI_NO"].ToString() != "") //eğer kapı numarası boş değil ise
                //    {
                //        if (addressParsing[(int)ParsingAdress.Kapı] == dtKapi.Rows[i]["KAPI_NO"].ToString())  //eğer girilen kapı numarası combobox'ın içindeki kapı numaralarından birine denk geliyor ise
                //        {
                //            //searchDoorNumber.SelectedIndex = i;  //kapı combobox'ındaki değeri seçtir
                //            //searchDoorNumber_SelectedIndexChanged(sender, e);                                                    
                //            searchDoorNumberSelect((decimal)dtKapi.Rows[i]["KAPI_ID"]);
                //            counter = 1;
                //            break;
                //        }
                //    }
                //}

                foreach (var kapi in kapiList)
                {
                    if (kapi.KapiNo != "") //eğer kapı numarası boş değil ise
                    {
                        if (addressParsing[(int)ParsingAdress.Kapı] == kapi.KapiNo)  //eğer girilen kapı numarası combobox'ın içindeki kapı numaralarından birine denk geliyor ise
                        {
                            //searchDoorNumber.SelectedIndex = i;  //kapı combobox'ındaki değeri seçtir
                            //searchDoorNumber_SelectedIndexChanged(sender, e);                                                    
                            searchDoorNumberSelect(kapi.KapiId.Value);
                            counter = 1;
                            break;
                        }
                    }
                }


                if (counter == 0)
                {
                    for (int i = 0; i < kapiList.Count; i++)  //kapı combosunun içerisindeki kapı numaralarında geziniyoruz
                    {
                        string patternK = @"(-)|(/)";
                        string[] Ks = Regex.Split(addressParsing[(int)ParsingAdress.Kapı], patternK);

                        string no = Ks[0];

                        int result = Convert.ToInt32((Regex.Replace(no, "[^0-9]", "") == "") ? "0" : (Regex.Replace(no, "[^0-9]", ""))); //el ile girilen kapı numarası değişkene aktarılıyor (girilebilecek diğer karakterleri ayıklama yaprak)

                        string originalKapı = kapiList[i].KapiNo;
                        int digitKapı = Convert.ToInt32((Regex.Replace(originalKapı, "[^0-9]", "") == "") ? "0" : (Regex.Replace(originalKapı, "[^0-9]", "")));


                        if (kapiList[i].KapiNo != "") //eğer kapı numarası boş değil ise
                        {
                            if (result.ToString() == originalKapı)  //eğer girilen kapı numarası combobox'ın içindeki kapı numaralarından birine denk geliyor ise
                            {
                                //searchDoorNumber.SelectedIndex = i;  //kapı combobox'ındaki değeri seçtir
                                //searchDoorNumber_SelectedIndexChanged(sender, e);                                                    
                                searchDoorNumberSelect(kapiList[i].KapiId.Value);
                                counter = 1;
                                break;
                            }
                            else if (result.ToString() == digitKapı.ToString())
                            {
                                searchDoorNumberSelect(kapiList[i].KapiId.Value);
                                counter = 1;
                                break;
                            }

                        }
                    }
                }


                if (counter == 0)  //eğer el ile girilen kapı numarası kapı combobox'ının içerisindeki(O mahallede) bir kapı numarasına denk gelmedi ise
                {

                    string patternK = @"(-)|(/)";
                    string[] Ks = Regex.Split(addressParsing[(int)ParsingAdress.Kapı], patternK);

                    string no = Ks[0];

                    int result = Convert.ToInt32((Regex.Replace(no, "[^0-9]", "") == "") ? "0" : Regex.Replace(no, "[^0-9]", "")); //el ile girilen kapı numarası değişkene aktarılıyor (girilebilecek diğer karakterleri ayıklama yaparak) (Eğer ayıklama yapıldığında sayı kalmıyor ise 0 alınıyor)
                    int cntr = DoorNumberSelect(result);  //el ile girilen sayıya en yakın tek/çift kapı numarasını seçmeye yarayan fonksiyon çağrılıyor (burada fonksiyon kapı combobox'ında girilen tek veya çift kapı numarasını karşılaştıracak tek veya çift hiçbir kapı numarası bulamazsa bir değer döndürüyor)

                    //eğer girilen tek kapı numarası ise ve kapı combobox'ında tek kapı yoksa veya
                    //eğer girilen çift kapı numarası ise ve kapı combobox'ında çift kapı yoksa
                    if (cntr == 0)
                    {
                        result += 1;   //sayı tek ise bir üstündeki çift sayıya - çift ise bir üstündeki tek sayıya yuvarlanıyor
                        DoorNumberSelect(result);  //tekrar en yakın tek/çift kapı seçimi için fonksiyon çağrılıyor
                    }
                }
            }
        }

        private void searchDoorNumberSelect(long Kapi_ID)
        {
            //seçilen kapının x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    
            //HinterlandPOIData.KAPIDataTable dtDoor2 = new HinterlandMapProxy().GetKapiDataByMahalleIdandYolIdandKapiId((decimal)dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"], (decimal)dtCross.DefaultView.ToTable().Rows[0]["YOL_ID"], Kapi_ID);

            var kapi = parsingService.GetKapiDataByMahalleIdandYolIdandKapiId(cross[0].MahalleId.Value, cross[0].YolId.Value, Kapi_ID).FirstOrDefault();

            #region Kapı seçildi ise koordinat bilgisini almak için
            if (kapi.XCoor != "0" && kapi.XCoor != null)
            {
                hierarchy[6, 0] = kapi.XCoor;
                hierarchy[6, 1] = kapi.YCoor;
            }
            else
            {
                hierarchy[6, 0] = "9999999";
                hierarchy[6, 1] = "9999999";
            }
            #endregion
        }

        private int DoorNumberSelect(int result)
        {
            //eğer girilen tek kapı numarası ise ve kapı combobox'ında tek kapı yoksa veya
            //eğer girilen çift kapı numarası ise ve kapı combobox'ında çift kapı yoksa kontrolünü yapmak için
            int cntr = 0;

            int r = 1;

            #region comment
            //while (true)
            //{
            //    if (Regex.Replace(dtKapi.Rows[dtKapi.Rows.Count - r]["KAPI_NO"].ToString(), "[^0-9]", "") != "")
            //    {
            //        kapiEnd = Convert.ToInt32(Regex.Replace(dtKapi.Rows[dtKapi.Rows.Count - r]["KAPI_NO"].ToString(), "[^0-9]", ""));
            //        kapiEndId = Convert.ToDecimal(dtKapi.Rows[dtKapi.Rows.Count - r]["KAPI_ID"].ToString());
            //        break;
            //    }

            //    r += 1;

            //    if (r > dtKapi.Rows.Count - r)
            //    {
            //        kapiEnd = 0;
            //        kapiEndId = Convert.ToDecimal(dtKapi.Rows[0]["KAPI_ID"].ToString());
            //        break;
            //    }
            //} 
            #endregion

            int endKapi = 0;
            long endKapiId = 0;

            string[,] doorDistance = new string[kapiList.Count, 2];

            for (int i = 0; i < kapiList.Count; i++)  //kapı combobox'ında bulunan kapı numaralarında geziyoruz
            {
                string patternK = @"(-)|(/)";
                string[] Ks = Regex.Split(kapiList[i].KapiNo, patternK);

                string no = Ks[0];

                int selectedKapi = Convert.ToInt32((Regex.Replace(no, "[^0-9]", "") == "") ? "0" : Regex.Replace(no, "[^0-9]", ""));

                long selectedKapiId = Convert.ToInt64(kapiList[i].KapiId);

                if (result % 2 == 0)  //girdiğimiz kapı numarası çift ise
                {
                    //kapı combobox'ındaki kapı numarası çift ise
                    if (selectedKapi % 2 == 0)
                    {
                        endKapi = selectedKapi;
                        endKapiId = selectedKapiId;

                        if ((result == selectedKapi) || ((result + 2) == selectedKapi) || ((result - 2) == selectedKapi))
                        {
                            //seçilen kapının x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    
                            //HinterlandPOIData.KAPIDataTable dtDoor2 = new HinterlandMapProxy().GetKapiDataByMahalleIdandYolIdandKapiId((decimal)dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"], (decimal)dtCross.DefaultView.ToTable().Rows[0]["YOL_ID"], selectedKapiId);
                            var kapi = parsingService.GetKapiDataByMahalleIdandYolIdandKapiId(cross[0].MahalleId.Value, cross[0].YolId.Value, selectedKapiId).FirstOrDefault();

                            #region kapı seçildi ise koordinat bilgisini almak için
                            if (kapi.XCoor != "0" && kapi.XCoor != null)
                            {
                                hierarchy[6, 0] = kapi.XCoor;
                                hierarchy[6, 1] = kapi.YCoor;
                            }
                            else
                            {
                                hierarchy[6, 0] = "9999999";
                                hierarchy[6, 1] = "9999999";
                            }
                            #endregion
                            //MessageBox.Show(((DataTable)searchDoorNumber.DataSource).Rows[searchDoorNumber.Items.Count - 1]["KAPI_NO"].ToString(), "UYARI", MessageBoxButtons.OK);
                            cntr = 1;
                            break;
                        }


                        #region uzaklık alma
                        if (selectedKapi > result)
                        {
                            doorDistance[i, 0] = (selectedKapi - result).ToString();
                            doorDistance[i, 1] = selectedKapiId.ToString();
                        }
                        else if (selectedKapi < result)
                        {
                            doorDistance[i, 0] = (result - selectedKapi).ToString();
                            doorDistance[i, 1] = selectedKapiId.ToString();
                        }
                        else if (selectedKapi == result)
                        {
                            doorDistance[i, 0] = "0";
                            doorDistance[i, 1] = selectedKapiId.ToString();
                        }
                        #endregion
                    }
                }
                else
                {
                    if (selectedKapi % 2 == 1)
                    {
                        endKapi = selectedKapi;
                        endKapiId = selectedKapiId;

                        if ((result == selectedKapi) || ((result + 2) == selectedKapi) || ((result - 2) == selectedKapi))
                        {
                            //seçilen kapının x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    
                            //HinterlandPOIData.KAPIDataTable dtDoor2 = new HinterlandMapProxy().GetKapiDataByMahalleIdandYolIdandKapiId((decimal)dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"], (decimal)dtCross.DefaultView.ToTable().Rows[0]["YOL_ID"], selectedKapiId);
                            var kapi = parsingService.GetKapiDataByMahalleIdandYolIdandKapiId(cross[0].MahalleId.Value, cross[0].YolId.Value, selectedKapiId).FirstOrDefault();

                            #region kapı seçildi ise koordinat bilgisini almak için
                            if (kapi.XCoor != "0" && kapi.XCoor != null)
                            {
                                hierarchy[6, 0] = kapi.XCoor;
                                hierarchy[6, 1] = kapi.YCoor;
                            }
                            else
                            {
                                hierarchy[6, 0] = "9999999";
                                hierarchy[6, 1] = "9999999";
                            }
                            #endregion
                            //MessageBox.Show(((DataTable)searchDoorNumber.DataSource).Rows[searchDoorNumber.Items.Count - 1]["KAPI_NO"].ToString(), "UYARI", MessageBoxButtons.OK);
                            cntr = 1;
                            break;
                        }


                        #region uzaklık alma
                        if (selectedKapi > result)
                        {
                            doorDistance[i, 0] = (selectedKapi - result).ToString();
                            doorDistance[i, 1] = selectedKapiId.ToString();
                        }
                        else if (selectedKapi < result)
                        {
                            doorDistance[i, 0] = (result - selectedKapi).ToString();
                            doorDistance[i, 1] = selectedKapiId.ToString();
                        }
                        else if (selectedKapi == result)
                        {
                            doorDistance[i, 0] = "0";
                            doorDistance[i, 1] = selectedKapiId.ToString();
                        }
                        #endregion
                        
                    }
                }
            }

            int enKucuk = 9999999;

            if (cntr != 1)
            {
                for (int i = 0; i < kapiList.Count; i++)
                {
                    if (doorDistance[i, 0] != null)
                    {
                        if (Convert.ToInt32(doorDistance[i, 0]) < enKucuk)
                        {
                            endKapiId = Convert.ToInt64(doorDistance[i, 1]);
                            enKucuk = Convert.ToInt32(doorDistance[i, 0]);
                        }
                    }
                }
            }

            if (enKucuk != 9999999)
            {
                //seçilen kapının x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    
                //HinterlandPOIData.KAPIDataTable dtDoor2 = new HinterlandMapProxy().GetKapiDataByMahalleIdandYolIdandKapiId((decimal)dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"], (decimal)dtCross.DefaultView.ToTable().Rows[0]["YOL_ID"], endKapiId);

                var yolId = cross[0].YolId;
                var mahalleId = cross[0].MahalleId;

                if (yolId != null && mahalleId != null)
                {
                    var kapi =
                        parsingService.GetKapiDataByMahalleIdandYolIdandKapiId(mahalleId.Value, yolId.Value, endKapiId)
                            .FirstOrDefault();

                    #region kapı seçildi ise koordinat bilgisini almak için

                    if (kapi.XCoor != "0" && kapi.XCoor != null)
                    {
                        hierarchy[6, 0] = kapi.XCoor;
                        hierarchy[6, 1] = kapi.YCoor;
                    }
                    else
                    {
                        hierarchy[6, 0] = "9999999";
                        hierarchy[6, 1] = "9999999";
                    }
                }

                #endregion
                //MessageBox.Show(((DataTable)searchDoorNumber.DataSource).Rows[searchDoorNumber.Items.Count - 1]["KAPI_NO"].ToString(), "UYARI", MessageBoxButtons.OK);
                cntr = 1;
            }

            return cntr;  // en yakın kapı seçiminin yapılıp yapılmadığı bilgisi geri döndürülüyor (eğer yapılamamış ise fonksiyon tekrar çağırılacak)
        }

        protected void VMTespit()
        {

            if (CityId != 0)  //Eğer İL combobox'ı boş değil ise
            {
                xcoor = "0";
                ycoor = "0";

                //DataSet polygonunit = new DataSet();
                //DataTable dt2 = new DataTable();
                //polygonunit.Tables.Add(dt2);
                IList<Unit> polygonUnitType = new List<Unit>();
                IList<PolygonList> polygonLists = new List<PolygonList>();
                //HinterlandPOIData.CITYPOLYGONHIERARCHYDataTable dtCityPolygonHierarchy = new HinterlandPOIData.CITYPOLYGONHIERARCHYDataTable();
                //dtCityPolygonHierarchy = new HinterlandMapProxy().GetIncludeSearchPolygonDataByIlId(CityId);

                var cityPolygonHierarchies = parsingService.GetIncludeSearchPolygonDataByIlId(CityId).ToList();
                IList<TownPolygonHierarchy> townPolygonHierarchies = new List<TownPolygonHierarchy>();

                #region Balkar ATEŞ - 20.03.2013 - İlçeden varış merkezi tespiti için hangi ilçeden tespit yapılacağı kontrol ediliyor
                //HinterlandPOIData.TOWNPOLYGONHIERARCHYDataTable dtTownPolygonHierarchy = new HinterlandPOIData.TOWNPOLYGONHIERARCHYDataTable();

                if (TownId != 0)
                {
                    //dtTownPolygonHierarchy = new HinterlandMapProxy().GetSearchIlceDataByIlidandIlceid(CityId, TownId);
                    townPolygonHierarchies = parsingService.GetSearchIlceDataByIlidandIlceid(CityId, TownId).ToList();
                }
                #endregion


                int exist = 0;
                int upsearch = 0;

                for (int i = 6; i >= 0; i--)
                {
                    if (hierarchy[i, 0].ToString() != "0")
                    {
                        if (exist == 1)
                        {
                            break;
                        }

                        xcoor = hierarchy[i, 0].ToString();
                        ycoor = hierarchy[i, 1].ToString();

                        switch (i)
                        {
                            case 0:  //İl bazında şube bulma

                                if (upsearch == 0)
                                {
                                    if (cityPolygonHierarchies.Count > 0)  //Eğer seçili ilde arama yapılacaksa
                                    {
                                        if (cityPolygonHierarchies[0].IlId == CityId && cityPolygonHierarchies[0].Il == "1")
                                        {
                                            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(8, CityId.ToString());
                                            polygonUnitType = procService.GetUnitByIdAndPolygonType(8,CityId.ToString());
                                        }
                                        else
                                        {
                                            //DataTable dt = new DataTable();
                                            //polygonunit.Tables.Add(dt);
                                        }
                                        upsearch = 1;
                                    }
                                    else
                                    {
                                        //DataTable dt = new DataTable();
                                        //polygonunit.Tables.Add(dt);
                                    }
                                }
                                else
                                {
                                    if (cityPolygonHierarchies.Count > 0)  //Eğer seçili ilde arama yapılacaksa
                                    {
                                        if (cityPolygonHierarchies[0].IlId == CityId && cityPolygonHierarchies[0].Il == "1")
                                        {
                                            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(8, CityId.ToString());
                                            polygonUnitType = procService.GetUnitByIdAndPolygonType( 8 , CityId.ToString());
                                        }
                                        else
                                        {
                                            //DataTable dt = new DataTable();
                                            //polygonunit.Tables.Add(dt);
                                        }
                                        upsearch = 1;
                                    }
                                    else
                                    {
                                        //DataTable dt = new DataTable();
                                        //polygonunit.Tables.Add(dt);
                                    }
                                }


                                if (polygonUnitType.Count > 0)  //seçili ilde arama yapıldıktan sonra içerisine denk gelen Şube polygonu var ise
                                {
                                    TespitSeviyesi = 1;
                                    exist = 1;
                                }
                                else
                                {
                                    TespitSeviyesi = -1;
                                }

                                break;
                            case 1:  //İlçe bazında şube bulma

                                if (upsearch == 0)
                                {
                                    if (townPolygonHierarchies.Count > 0)  //Eğer seçili ilçede arama yapılacaksa
                                    {
                                        if (townPolygonHierarchies[0].Search == "1")
                                        {
                                            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(9, TownId.ToString());
                                            polygonUnitType = procService.GetUnitByIdAndPolygonType( 9,TownId.ToString());

                                        }
                                        else
                                        {
                                            //DataTable dt = new DataTable();
                                            //polygonunit.Tables.Add(dt);
                                        }
                                        upsearch = 1;
                                    }
                                    else
                                    {
                                        //DataTable dt = new DataTable();
                                        //polygonunit.Tables.Add(dt);
                                    }
                                }
                                else
                                {
                                    if (townPolygonHierarchies.Count > 0)  //Eğer seçili ilçede arama yapılacaksa
                                    {
                                        if (townPolygonHierarchies[0].Search == "1")
                                        {
                                            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(9, TownId.ToString());
                                            polygonUnitType = procService.GetUnitByIdAndPolygonType(9,TownId.ToString());

                                        }
                                        else
                                        {
                                            upsearch = 1;
                                        }
                                    }
                                    else
                                    {
                                        upsearch = 1;
                                    }
                                }


                                if (polygonUnitType.Count > 0)  //Seçili ilde arama yapıldıktan sonra içerisine denk gelen şube polygonu var ise
                                {
                                    TespitSeviyesi = 2;
                                    exist = 1;
                                }
                                break;

                            case 2:  //Mahalle bazında şube bulma
                                if (xcoor != "9999999")
                                {
                                    //HinterlandPOIData.GEOLOC_MAHALLEDataTable dtGeoloc_Mahalle = new HinterlandMapProxy().GetGeolocMahalleDataByMahalleId((decimal)dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"]);

                                    var geolocMahalles = parsingService.GetGeolocMahalleDataByMahalleId(cross[0].MahalleId.Value).ToList();

                                    if (geolocMahalles.Count > 0)  //Mahallenin polygonu var ise polygona denk gelen şubeleri bul
                                    {
                                        //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(10, dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"].ToString());
                                        polygonUnitType = procService.GetUnitByIdAndPolygonType(10,cross[0].MahalleId.Value.ToString());
                                    }
                                    else //Mahallenin polygonu yok ise mahalle koordinatının denk geldiği şubeyi bul
                                    {
                                        //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
                                        if (xcoor != null)
                                            polygonLists = parsingService.GetXyCoorAndPolygonTypeId(xcoor, ycoor, 3).ToList();
                                    }

                                    if (polygonUnitType.Count > 0 || polygonLists.Count > 0) //Eğer mahallenin denk geldiği şube polygonu var ise
                                    {
                                        TespitSeviyesi = 3;
                                        exist = 1;
                                    }
                                }
                                else
                                {
                                    upsearch = 1;
                                }

                                break;

                            case 3:  //Cadde bazında şube bulma

                                if (xcoor != "9999999")
                                {
                                    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
                                    if (xcoor != null)
                                        polygonLists = parsingService.GetXyCoorAndPolygonTypeId(xcoor, ycoor, 3).ToList();

                                    if (polygonLists.Count > 0)  //Eğer Caddenin denk geldiği şube polygonu var ise
                                    {
                                        TespitSeviyesi = 4;
                                        exist = 1;
                                    }
                                }
                                else
                                {
                                    upsearch = 1;
                                }
                                break;

                            case 4:  //Sokak bazında şube bulma

                                if (xcoor != "9999999")
                                {
                                    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
                                    if (xcoor != null)
                                        polygonLists = parsingService.GetXyCoorAndPolygonTypeId(xcoor, ycoor, 3).ToList();

                                    if (polygonLists.Count > 0)  //Eğer sokağın denk geldiği şube polygonu var ise
                                    {
                                        TespitSeviyesi = 4;
                                        exist = 1;
                                    }
                                }
                                else
                                {
                                    upsearch = 1;
                                }
                                break;

                            case 5:  //POI bazında şube bulma

                                if (xcoor != "9999999")
                                {
                                    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
                                    if (xcoor != null)
                                        polygonLists = parsingService.GetXyCoorAndPolygonTypeId(xcoor, ycoor, 3).ToList();

                                    if (polygonLists.Count > 0)  //Eğer POI'nin denk geldiği şube polygonu var ise
                                    {
                                        TespitSeviyesi = 5;
                                        exist = 1;
                                    }
                                }
                                else
                                {
                                    upsearch = 1;
                                }
                                break;

                            case 6: //Kapı Numarası bazında şube bulma

                                if (xcoor != "9999999")
                                {
                                    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
                                    if (xcoor != null)
                                        polygonLists = parsingService.GetXyCoorAndPolygonTypeId(xcoor, ycoor, 3).ToList();

                                    if (polygonLists.Count > 0)  //Eğer Kapı Numarasının denk geldiği şube polygonu var ise
                                    {
                                        TespitSeviyesi = 6;
                                        exist = 1;
                                    }
                                }
                                else
                                {
                                    upsearch = 1;
                                }
                                break;
                        }

                    }
                }

                if (exist != 0) //Eğer koordinat bilgisi içeriyorsa polygon araması yapsın
                {
                    if (polygonLists.Count > 0)
                        //Eğer Mahalle/Yol/Birim(POI) seçildikten sonra Koordinat bilgisi polygonlist tablosunda karşılık gelen bir şubeID'si var ise
                    {
                        table = new DataTable();
                        table.Columns.Add("UNITID", typeof (Int32));
                        table.Columns.Add("UNIT_NAME", typeof (string));

                        for (int i = 0; i < polygonLists.Count; i++) //Denk gelen bütün şubelerin ismini yazdırma
                        {
                            //Tablodan gelen ŞubeID'sinin ismini almak için UnitNameFromUnitId fonksiyonuna gönderiliyor
                            string unitName = UnitNameFromUnitId(Convert.ToInt32(polygonLists[i].Unit.UnitId));

                            if (unitName != "bos")
                                //Eğer unit tablosunda tablodan gelen ŞubeID'si var ise (bu kontrol yeni eklenen şubeler olabilir ihtimaline karşı koyulmuştur)
                            {
                                table.Rows.Add(Convert.ToInt32(polygonLists[i].Unit.UnitId), unitName);
                            }
                        }
                    }
                    else if (polygonUnitType.Count > 0)
                    {
                        table = new DataTable();
                        table.Columns.Add("UNITID", typeof(Int32));
                        table.Columns.Add("UNIT_NAME", typeof(string));

                        for (int i = 0; i < polygonUnitType.Count; i++) //Denk gelen bütün şubelerin ismini yazdırma
                        {
                            //Tablodan gelen ŞubeID'sinin ismini almak için UnitNameFromUnitId fonksiyonuna gönderiliyor
                            string unitName = UnitNameFromUnitId(Convert.ToInt32(polygonUnitType[i].UnitId));

                            if (unitName != "bos")
                            //Eğer unit tablosunda tablodan gelen ŞubeID'si var ise (bu kontrol yeni eklenen şubeler olabilir ihtimaline karşı koyulmuştur)
                            {
                                table.Rows.Add(Convert.ToInt32(polygonUnitType[i].UnitId), unitName);
                            }
                        }
                    }
                    else //Eğer Mahalle/Yol/Birim(POI) seçildikten sonra Koordinat bilgisi polygonlist tablosunda karşılık gelen bir şubeID'si yok ise
                    {
                        table = new DataTable();
                    }
                }
                else
                {
                    table = new DataTable();
                }


            }
            else  // Eğer Mahalle/Yol/Birim(POI) bilgisi boş ise varış şubesi alanı boşaltılsın
            {
                table = new DataTable();
            }
        }

        private string UnitNameFromUnitId(int UnitId)  //ŞubeID'si bilinen bir şubenin ismini bulmak için
        {
            //AccountContractData.UNITDataTable unitdt = new AccountContractData.UNITDataTable();
            //unitdt = accountcontractProxy.GetDataUnitNameFromUnitId(UnitId);

            var unitList = unitService.GetById(UnitId).ToList();

            if (unitList.Count > 0 && unitList != null)  //eğer Unit tablosunda şubeID'sine karşılık gelen bir şube var ise
            {
                return unitList[0].Name;
            }
            else  //eğer Unit tablosunda şubeID'sine karşılık gelen bir şube yok ise
            {
                return "bos";
            }

        }

        private void save()
        {

        }

        public string[] AddressParsingControl(String _cityName, String _townName, String _addressString)
        {
            string[] addressParse = new string[3];

            MapAddressParser adp = new MapAddressParser();
            adp.mAddress = _addressString;

            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bina] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bina].ToString() != "")
                    addressParse[0] = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Bina].ToString();

            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Blok] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Blok].ToString() != "")
                    addressParse[1] = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Blok].ToString();

            if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Daire] != null)
                if (adp.addressParsing[(int)MapAddressParser.ParsingAdress.Daire].ToString() != "")
                    addressParse[2] = adp.addressParsing[(int)MapAddressParser.ParsingAdress.Daire].ToString();

            return addressParse;
        }

        public static string SearchText(string searchText)
        {
            string replaceText = searchText;
            replaceText = replaceText.Replace(" ", "");
            replaceText = replaceText.Replace(".", "");
            replaceText = replaceText.Replace(",", "");
            replaceText = replaceText.Replace("Ü", "U");
            replaceText = replaceText.Replace("İ", "I");
            replaceText = replaceText.Replace("Ö", "O");
            replaceText = replaceText.Replace("Ş", "S");
            replaceText = replaceText.Replace("Ğ", "G");
            replaceText = replaceText.Replace("Ç", "C");

            return replaceText;
        }
        
    }
}
