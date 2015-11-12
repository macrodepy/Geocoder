using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using GeocoderAPI.Core;
using GeocoderAPI.Model;
using GeocoderAPI.DAL;

namespace GeocoderAPI.Default
{
    public class Parsing : IParsing
    {
        public enum ParsingAdress
        {
            Il,
            Ilçe,
            Mahalle,
            MahalleId,
            Cadde,
            Sokak,
            POI,
            PostaKodu,
            BinaAdi,
            Blok,
            Daire,
            Kapı,
            Kat,
            XCoor,
            YCoor,
            TespitSeviyesi
        }; //Adres değişkenlerinin listesi

        public string[] AddressParsing = new string[17];
        private IList<VUnitSearch> cross;
        private IList<KAPI> kapiList;

        public enum VeriTipi { Mahalle, Cadde, Sokak, POI };  //Adres değişkenlerinin listesi

        int veriTipiId = -1;

        static string[,] hierarchy;

        long cityId = 0;
        long townId = 0;

        long yolId;
        long mahalleId;
        long poiId;

        bool integrationSave = false;

        private readonly System.Globalization.CultureInfo cultureInfo;
        private readonly GeocoderService geocoderService;
        private AddressLevel addressLevel;
        private GeocoderModel geocoderModel;

        public Parsing()
        {
            cultureInfo = new System.Globalization.CultureInfo("tr-TR");
            addressLevel = new AddressLevel();
            geocoderService = new GeocoderService();
        }

        public string[] IntegrationParsing(AddressLevel addressLevel)
        {
            geocoderModel = new GeocoderModel();

            this.addressLevel = addressLevel;
            try
            {
                AddresGeocode();
            }
            catch (Exception ex)
            {
            }

            for (int i = 6; i >=0; i--)
            {
                if (hierarchy[i, 0] != "9999999")
                {
                    addressLevel.XCoor = hierarchy[i, 0];
                    addressLevel.YCoor = hierarchy[i, 1];
                    break;
                }
            }

            return AddressParsing;
        }

        public void AddresGeocode()
        {
            hierarchy = new string[7, 2];

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    hierarchy[i, j] = "9999999";
                }
            }

            //HinterlandPOIData.HINTCITYGEOCITYCRDataTable hintCityGeoCityCR = new HinterlandMapProxy().GetIL_IDDataByHintCityGeoCityCR(pCityName.ToUpper(cultureInfo).Trim());
            IList<HINTCITYGEOCITYCR> ilIdDataByHintCityGeoCityCr = geocoderService.GetIL_IDDataByHintCityGeoCityCR(addressLevel.Il.ToUpper(cultureInfo).Trim()).ToList();

            int flag = 0;

            if (ilIdDataByHintCityGeoCityCr.Count > 0)
            {
                var hintCityGeoCity = ilIdDataByHintCityGeoCityCr.FirstOrDefault();
                if (hintCityGeoCity.GEOLOC_IL_ID != null)  //tabloda geoloc_il_id adında kolon var ise
                {
                    cityId = hintCityGeoCity.GEOLOC_IL_ID.Value;  //İl ID'si atanıyor

                    AddressParsing[(int)ParsingAdress.Il] = cityId.ToString();

                    #region İl Seçilirse İl'in koordinat bilgisi aktarılır
                    var il = geocoderService.GetIlDataByIL_ID(cityId).FirstOrDefault();

                    if (il.XCOOR != null || il.XCOOR != "0")
                    {
                        hierarchy[0, 0] = il.XCOOR;
                        hierarchy[0, 1] = il.YCOOR;
                    }
                    else
                    {
                        hierarchy[0, 0] = "9999999";
                        hierarchy[0, 1] = "9999999";
                    }
                    #endregion


                    IList<HINTTOWNGEOTOWNCR> hintTownGeoTownCrs = geocoderService.GetGeoloc_Ilce_IDDataByHINTTOWNGEOTOWNCR(addressLevel.Ilçe.ToUpper(cultureInfo).Trim(),
                        hintCityGeoCity.GEOLOC_IL_ID.Value).ToList();


                    if (hintTownGeoTownCrs.Count > 0)  //Eğer İlçe bulunabildi ise
                    {
                        var hintTownGeoTown = hintTownGeoTownCrs.FirstOrDefault();
                        if (hintTownGeoTown.GEOLOC_ILCE_ID.Value != null)  //Tabloda geoloc_ilce_id adında kolon var ise
                        {
                            townId = hintTownGeoTown.GEOLOC_ILCE_ID.Value;  //İlçe Id'si combodan seçiliyor

                            AddressParsing[(int)ParsingAdress.Ilçe] = townId.ToString();

                            #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                            var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();

                            if (ilce.XCOOR != "0")
                            {
                                hierarchy[1, 0] = ilce.XCOOR;
                                hierarchy[1, 1] = ilce.YCOOR;
                            }
                            else
                            {
                                hierarchy[1, 0] = "9999999";
                                hierarchy[1, 1] = "9999999";
                            }
                            #endregion

                            if (!integrationSave)
                            {
                                while (true)
                                {
                                    string searchText = "999999999";

                                    searchText = Mapaddressconcat(flag);

                                    if (flag == 29)
                                    {
                                        break;
                                    }

         

                                    if (searchText == "999999999")  //Eğer birşey dönmedi ise döngüye devam et
                                    {
                                        flag += 1;
                                        continue;
                                    }

                                    //Mahalle/Yol/Birim'den dönen sonuçları dataTable'a aktarma
                                    string replaceText = SearchText(searchText.ToUpper(cultureInfo));

                                    cross = geocoderService.GetUnitSearchDataByIlAndIlceId(replaceText,
                                        hintTownGeoTown.GEOLOC_ILCE_ID.Value);

                                    if (cross != null && cross.Count > 0)  //Eğer veritabanında adres bulundu ise
                                    {
                                        if (cross.Count > 0 && cross.Count < 2) //Eğer adresin karşılığı tek kayıt ise
                                        {
                                            SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                            //if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                            //{
                                            //    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                            //    //integrationSave = true;
                                            //    break;
                                            //}
                                            flag += 1;
                                        }
                                        else if (cross.Count > 1)  //Eğer birden fazla kayıt dönerse tipe göre filtre kullan
                                        {
                                            switch (veriTipiId)
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

                                                //if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                                //{
                                                //    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                                //    //integrationSave = true;
                                                //    break;
                                                //}
                                                flag += 1;
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

                                                    //if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                                    //{
                                                    //    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                                    //    //integrationSave = true;
                                                    //    break;
                                                    //}
                                                    //else
                                                    //{
                                                    //    //address parser'dan gelen bir sonraki datayı işlemesi için
                                                    flag += 1;
                                                    //}
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

                            if (flag == 29)
                                break;

                            if (searchText == "999999999")  //Eğer birşey dönmedi ise döngüye devam et
                            {
                                flag += 1;
                                continue;
                            }

                            //Mahalle/Yol/Birim'den dönen sonuçları dataTable'a aktarma
                            string replaceText = SearchText(searchText.ToUpper(cultureInfo));
                            //dtCross = new HinterlandMapProxy().GetUnitSearchDataByIlId(replaceText, hintCityGeoCityCR[0].GEOLOC_IL_ID);
                            cross = geocoderService.GetUnitSearchDataByIlId(replaceText, hintCityGeoCity.GEOLOC_IL_ID.Value).ToList();

                            if (cross != null && cross.Count > 0)
                            {
                                if (cross.Count > 0 && cross.Count < 2)
                                {
                                    #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                    if (!String.IsNullOrEmpty(cross.FirstOrDefault().IlceId.ToString()))
                                    {
                                        townId = cross.FirstOrDefault().IlceId;

                                        AddressParsing[(int)ParsingAdress.Ilçe] = townId.ToString();

                                        //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                                        var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();

                                        if (ilce.XCOOR != "0")
                                        {
                                            hierarchy[1, 0] = ilce.XCOOR;
                                            hierarchy[1, 1] = ilce.YCOOR;
                                        }
                                        else
                                        {
                                            hierarchy[1, 0] = "9999999";
                                            hierarchy[1, 1] = "9999999";
                                        }
                                    }
                                    #endregion

                                    SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                    //if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                    //{
                                    //    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                    //    //integrationSave = true;

                                    //    break;
                                    //}
                                    //else
                                    //{
                                        flag += 1;
                                    //}
                                }
                                else if (cross.Count > 1)  //Eğer birden fazla kayıt dönerse tipe göre filtre kullan
                                {
                                    switch (veriTipiId)
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

                                    if (cross.Count == 1) //Eğer adresin karşılığı tek kayıt ise
                                    {
                                        #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                        if (!String.IsNullOrEmpty(cross.FirstOrDefault().IlceId.ToString()))
                                        {
                                            townId = cross.FirstOrDefault().IlceId;

                                            AddressParsing[(int)ParsingAdress.Ilçe] = townId.ToString();

                                            //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                                            var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();

                                            if (ilce.XCOOR != "0")
                                            {
                                                hierarchy[1, 0] = ilce.XCOOR;
                                                hierarchy[1, 1] = ilce.YCOOR;
                                            }
                                            else
                                            {
                                                hierarchy[1, 0] = "9999999";
                                                hierarchy[1, 1] = "9999999";
                                            }

                                        }
                                        #endregion

                                        SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                        //if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                        //{
                                        //    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                        //    //integrationSave = true;
                                        //    break;
                                        //}
                                        //else
                                        //{
                                            flag += 1;
                                        //}
                                    }
                                    else if (cross.Count > 1)
                                    {

                                        bool sameAddress = true;
                                        string firstRecord = cross.FirstOrDefault().SonucIlceli;  //Sonuç ilçeliden bakılmasının nedeni tüm kayıtarın birebir aynı olmasına bakılmasıdır
                                  
                                        if (cross.Any(search => firstRecord != search.SonucIlceli))
                                        {
                                            sameAddress = false;
                                        }

                                        if (sameAddress)  //Eğer tüm kayıtlar birebir aynı ise 
                                        {

                                            #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                            if (!String.IsNullOrEmpty(cross.FirstOrDefault().IlceId.ToString()))
                                            {
                                                townId = cross.FirstOrDefault().IlceId;

                                                AddressParsing[(int)ParsingAdress.Ilçe] = townId.ToString();

                                                //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                                                var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();

                                                if (ilce.XCOOR != "0")
                                                {
                                                    hierarchy[1, 0] = ilce.XCOOR;
                                                    hierarchy[1, 1] = ilce.YCOOR;
                                                }
                                                else
                                                {
                                                    hierarchy[1, 0] = "9999999";
                                                    hierarchy[1, 1] = "9999999";
                                                }

                                            }
                                            #endregion

                                            SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış

                                            //if (table.Rows.Count == 1)  //Eğer Varış Merkezi Tespiti yapılabildi ise 
                                            //{
                                            //    //Direk kaydet işlemi yapılıp geçilmesi için eklendi
                                            //    //integrationSave = true;
                                            //    break;
                                            //}
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

        protected string Mapaddressconcat(int flag)
        {
            //Aranacak adres string'i
            string returntext = "";
            veriTipiId = -1;

            //Aramayı kaçıncı kez yapıyorsak ona göre aranacak string değişiyor
            switch (flag)
            {
                //TODO pField nedir??
                case 0:
                    if (addressLevel.Poi.Trim() != "")
                    {
                        returntext = addressLevel.Poi.Trim();
                        veriTipiId = (int)VeriTipi.POI;
                    }
                    break;
                case 1:
                    if (addressLevel.Poi.Trim() != "" && addressLevel.Blok.Trim() != "")
                    {
                        returntext = addressLevel.Poi.Trim() + addressLevel.Blok.Trim();
                        veriTipiId = (int)VeriTipi.POI;
                    }
                    break;
                case 2:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Poi.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ" + addressLevel.Poi.Trim();
                        veriTipiId = (int)VeriTipi.POI;
                    }
                    break;
                case 3:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Poi.Trim() != "" && addressLevel.Blok.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ" + addressLevel.Poi.Trim() + addressLevel.Blok.Trim();
                        veriTipiId = (int)VeriTipi.POI;
                    }
                    break;
             
                case 10:
                    if (addressLevel.Yolu.Trim() != "")
                    {
                        returntext = addressLevel.Yolu.Trim();
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 11:
                    if (addressLevel.Sokak.Trim() != "")
                    {
                        returntext = addressLevel.Sokak.Trim() + "SOKAK";
                        veriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 12:
                    if (addressLevel.Sokak1.Trim() != "" && addressLevel.Sokak2.Trim() != "")
                    {
                        returntext = addressLevel.Sokak1.Trim() + " ";
                        returntext += addressLevel.Sokak2.Trim();
                        veriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 13:
                    if (addressLevel.Cadde.Trim() != "")
                    {
                        returntext = addressLevel.Cadde.Trim() + "CADDESİ";
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 14:
                    if (addressLevel.Cadde1.Trim() != "" && addressLevel.Cadde2.Trim() != "")
                    {
                        returntext = addressLevel.Cadde1.Trim() + " ";
                        returntext += addressLevel.Cadde2.Trim();
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 15:
                    if (addressLevel.Bulvar.Trim() != "")
                    {
                        returntext = addressLevel.Bulvar.Trim() + "BULVARI";
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 16:
                    if (addressLevel.Bulvar1.Trim() != "" && addressLevel.Bulvar2.Trim() != "")
                    {
                        returntext = addressLevel.Bulvar1.Trim() + " ";
                        returntext += addressLevel.Bulvar2.Trim();
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 17:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Yolu.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Yolu.Trim();
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 18:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Sokak.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Sokak.Trim() + "SOKAK";
                        veriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 19:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Sokak.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Sokak.Trim() + "%" + "SOKAK";
                        veriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 20:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Sokak1.Trim() != "" && addressLevel.Sokak2.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Sokak1.Trim() + " ";
                        returntext += addressLevel.Sokak2.Trim();
                        veriTipiId = (int)VeriTipi.Sokak;
                    }
                    break;
                case 21:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Cadde.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Cadde.Trim() + "CADDESİ";
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 22:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Cadde.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Cadde.Trim() + "%" + "CADDESİ";
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;

                case 23:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Cadde1.Trim() != "" && addressLevel.Cadde2.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Cadde1.Trim() + " ";
                        returntext += addressLevel.Cadde2.Trim();
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 24:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Bulvar.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Bulvar.Trim() + "BULVARI";
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 25:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Bulvar.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Bulvar.Trim() + "%" + "BULVARI";
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 26:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Bulvar1.Trim() != "" && addressLevel.Bulvar2.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Bulvar1.Trim() + " ";
                        returntext += addressLevel.Bulvar2.Trim();
                        veriTipiId = (int)VeriTipi.Cadde;
                    }
                    break;
                case 27:
                    if (addressLevel.Köy.Trim() != "")
                    {
                        returntext = addressLevel.Köy.Trim() + "KÖYÜ";
                        veriTipiId = (int)VeriTipi.Mahalle;
                    }
                    break;
                case 28:
                    if (addressLevel.Mahalle.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        veriTipiId = (int)VeriTipi.Mahalle;
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
            yolId = 0;
            mahalleId = 0;
            poiId = 0;
            var model = cross.FirstOrDefault();

            if (!String.IsNullOrEmpty(model.YolId.ToString())) //yol_ID'sini almak için
            {
                yolId = (long)model.YolId.Value;
            }
            if (!String.IsNullOrEmpty(model.MahalleId.ToString())) //mahalle_id'sini almak için
            {
                mahalleId = (long)model.MahalleId.Value;
            }
            if (!String.IsNullOrEmpty(model.PoiId.ToString())) //poi_id'sini almak için
            {
                poiId = (long)model.PoiId.Value;
            }

            var idariYolSınırs = geocoderService.GetIdariYolDataByMahalleAndYolId(yolId, mahalleId).ToList();

            //HinterlandPOIData.MAHALLEDataTable dtMahalle = new HinterlandMapProxy().GetMahalleDataByMAHALLE_ID(mahalle_id); //mahalleid'si mahalle tablosunda aranıyor
            var mahalles = geocoderService.GetMahalleDataByMAHALLE_ID(mahalleId).ToList();

            //HinterlandPOIData.POIDataTable dtField = new HinterlandMapProxy().GetPOIDataByPOI_ID(poi_id); //poiid'si poi tablosunda aranıyor
            var pois = geocoderService.GetPOIDataByPOI_ID(poiId).ToList();


            if (mahalles.Count > 0)
            {
                var mahalle = mahalles.FirstOrDefault();
                if (mahalle.MAHALLE_ADI.IndexOf("MAHALLESİ") != -1)
                {
                    AddressParsing[(int)ParsingAdress.Mahalle] = mahalle.MAHALLE_ADI.Replace(" MAHALLESİ", "");   //mahalle bilgisi
                    AddressParsing[(int)ParsingAdress.MahalleId] = mahalle.MAHALLE_ID.ToString();
                }
                else
                {
                    AddressParsing[(int)ParsingAdress.Mahalle] = mahalle.MAHALLE_ADI;   //mahalle bilgisi
                    AddressParsing[(int)ParsingAdress.MahalleId] = mahalle.MAHALLE_ID.ToString();
                }


                #region Mahalle Var ise Koordinat Bilgisini almak için
                //en küçük hangi hiyerarşi bulundu ise onun ismini ekrana yazdırmak için
                //mahallenin x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    

                if (mahalle.XCOOR != "0")
                {
                    hierarchy[2, 0] = mahalle.XCOOR;
                    hierarchy[2, 1] = mahalle.YCOOR;
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
                            AddressParsing[(int)ParsingAdress.Sokak] = idariSınırYol.YolAdı.Replace(" SOKAK", "");  //Sokak Bilgisi
                        }
                        else
                        {
                            AddressParsing[(int)ParsingAdress.Sokak] = idariSınırYol.YolAdı; //Sokak Bilgisi
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
                            AddressParsing[(int)ParsingAdress.Cadde] = idariSınırYol.YolAdı.Replace(" CADDESİ", ""); //Cadde Bilgisi
                        }
                        else
                        {
                            AddressParsing[(int)ParsingAdress.Cadde] = idariSınırYol.YolAdı; //Cadde Bilgisi
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

                    if (!string.IsNullOrEmpty(AddressParsing[(int)ParsingAdress.Cadde]) || !string.IsNullOrEmpty(AddressParsing[(int)ParsingAdress.Sokak])) //eğer cadde veya sokak bilgisi girildi ise
                    {
                        kapiList = geocoderService.GetKapiDataByYOL_IDAndMAHALLE_ID(mahalleId, yolId);

                        if (kapiList.Count > 0)
                        {
                            kapiList = kapiList.Where(x => x.KAPI_NO == addressLevel.Kapı).ToList();

                            if (kapiList.Count == 1)
                            {
                                hierarchy[6, 0] = kapiList.First().XCOOR;
                                hierarchy[6, 1] = kapiList.First().YCOOR;
                            }
                           
                        }
                    }
                }
            }

            if (pois.Count > 0)  //(birim) POI bilgisi 
            {
                var poi = pois.FirstOrDefault();
                AddressParsing[(int)ParsingAdress.POI] = poi.STANDARD_NAME;

                #region POI varsa koordinat bilgisini almak için
                //en küçük hangi hiyerarşi bulundu ise onun ismini ekrana yazdırmak için
                //POI'nin x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    

                if (poi.XCOOR != "0")
                {
                    hierarchy[5, 0] = poi.XCOOR;
                    hierarchy[5, 1] = poi.YCOOR;
                }
                else
                {
                    hierarchy[5, 0] = "9999999";
                    hierarchy[5, 1] = "9999999";
                }
                #endregion
            }

            //if (pois.Count > 0)
            //{
            //    VMTespit();
            //}
            //else
            //{
            if (addressLevel.Kapı.Trim() != "")  //Eğer parser'dan kapı numarası geldi ise
            {
                AddressParsing[(int)ParsingAdress.Kapı] = addressLevel.Kapı.Trim();
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
                        AddressParsing[(int)ParsingAdress.Kapı] = kapi.KAPI_NO;

                        searchDoorNumber();

                        //VMTespit();
                    }
                    //else if (KapiCount > 1)
                    //{
                    //    decimal kapiUnitid;

                    //    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(dtKapi.Rows[0]["XCOOR"].ToString(), dtKapi.Rows[0]["YCOOR"].ToString(), 3);
                    //    IList<PolygonList> polygonUnitLists = new List<PolygonList>();
                    //    if (kapi.XCOOR != null)
                    //        polygonUnitLists = geocoderService.GetXyCoorAndPolygonTypeId(kapi.XCOOR, kapi.YCOOR, 3);

                    //    if (polygonUnitLists.Count > 0)  //Eğer Caddenin denk geldiği şube polygonu var ise
                    //    {
                    //        var polygonUnit = polygonUnitLists.FirstOrDefault();
                    //        kapiUnitid = Convert.ToDecimal(polygonUnit.Unit.UnitId);
                    //        bool kapiTespit = true;
                    //        for (int i = 1; i < KapiCount; i++)
                    //        {
                    //            //TODO yanlış yazılmış olabilir
                    //            //polygonunit2 = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(dtKapi.Rows[i]["XCOOR"].ToString(), dtKapi.Rows[i]["YCOOR"].ToString(), 3);
                    //             IList<PolygonList> polygonUnit2Lists = new List<PolygonList>();
                    //             if (kapi.XCOOR != null)
                    //               polygonUnit2Lists = geocoderService.GetXyCoorAndPolygonTypeId(kapi.XCOOR, kapi.YCOOR, 3);

                    //            if (polygonUnitLists.Count > 0)  //Eğer Caddenin denk geldiği şube polygonu var ise
                    //            {
                    //                if (kapiUnitid != Convert.ToDecimal(polygonUnit.Unit.UnitId))
                    //                {
                    //                    kapiTespit = false;
                    //                    break;
                    //                }
                    //            }
                    //        }

                    //        if (kapiTespit)  //Eğer parser'dan kapı bilgisi gelmedi ise ve tüm kapı numaraları aynı şubeyi veriyor ise 
                    //        {
                    //            AddressParsing[(int)ParsingAdress.Kapı] = "1";

                    //            searchDoorNumber();

                    //            VMTespit();  //Parser'dan kapı bilgisi gelmedi fakat yol üzerindeki tüm kapılar tek şube veriyor ise yoldan VM Tespiti yapılır
                    //        }
                    //    }
                    //}
                    //}
                    //else
                    //{
                    //    VMTespit();
                    //}
                }
            }
        }

        public void searchDoorNumber()
        {
            if (AddressParsing[(int)ParsingAdress.Kapı].Trim() != "")
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
                    if (kapi.KAPI_NO != "") //eğer kapı numarası boş değil ise
                    {
                        if (AddressParsing[(int)ParsingAdress.Kapı] == kapi.KAPI_NO)  //eğer girilen kapı numarası combobox'ın içindeki kapı numaralarından birine denk geliyor ise
                        {
                            //searchDoorNumber.SelectedIndex = i;  //kapı combobox'ındaki değeri seçtir
                            //searchDoorNumber_SelectedIndexChanged(sender, e);                                                    
                            searchDoorNumberSelect((long)kapi.KAPI_ID);
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
                        string[] Ks = Regex.Split(AddressParsing[(int)ParsingAdress.Kapı], patternK);

                        string no = Ks[0];

                        int result = Convert.ToInt32((Regex.Replace(no, "[^0-9]", "") == "") ? "0" : (Regex.Replace(no, "[^0-9]", ""))); //el ile girilen kapı numarası değişkene aktarılıyor (girilebilecek diğer karakterleri ayıklama yaprak)

                        string originalKapı = kapiList[i].KAPI_NO;
                        int digitKapı = Convert.ToInt32((Regex.Replace(originalKapı, "[^0-9]", "") == "") ? "0" : (Regex.Replace(originalKapı, "[^0-9]", "")));


                        if (kapiList[i].KAPI_NO != "") //eğer kapı numarası boş değil ise
                        {
                            if (result.ToString() == originalKapı)  //eğer girilen kapı numarası combobox'ın içindeki kapı numaralarından birine denk geliyor ise
                            {
                                //searchDoorNumber.SelectedIndex = i;  //kapı combobox'ındaki değeri seçtir
                                //searchDoorNumber_SelectedIndexChanged(sender, e);                                                    
                                searchDoorNumberSelect((long)kapiList[i].KAPI_ID);
                                counter = 1;
                                break;
                            }
                            else if (result.ToString() == digitKapı.ToString())
                            {
                                searchDoorNumberSelect((long)kapiList[i].KAPI_ID);
                                counter = 1;
                                break;
                            }

                        }
                    }
                }


                if (counter == 0)  //eğer el ile girilen kapı numarası kapı combobox'ının içerisindeki(O mahallede) bir kapı numarasına denk gelmedi ise
                {

                    string patternK = @"(-)|(/)";
                    string[] Ks = Regex.Split(AddressParsing[(int)ParsingAdress.Kapı], patternK);

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

            var kapi = geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(cross[0].MahalleId.Value, cross[0].YolId.Value, Kapi_ID).FirstOrDefault();

            #region Kapı seçildi ise koordinat bilgisini almak için
            if (kapi.XCOOR != "0" && kapi.XCOOR != null)
            {
                hierarchy[6, 0] = kapi.XCOOR;
                hierarchy[6, 1] = kapi.YCOOR;
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
                string[] Ks = Regex.Split(kapiList[i].KAPI_NO, patternK);

                string no = Ks[0];

                int selectedKapi = Convert.ToInt32((Regex.Replace(no, "[^0-9]", "") == "") ? "0" : Regex.Replace(no, "[^0-9]", ""));

                long selectedKapiId = Convert.ToInt64(kapiList[i].KAPI_ID);

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
                            var kapi = geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(cross[0].MahalleId.Value, cross[0].YolId.Value, selectedKapiId).FirstOrDefault();

                            #region kapı seçildi ise koordinat bilgisini almak için
                            if (kapi.XCOOR != "0" && kapi.XCOOR != null)
                            {
                                hierarchy[6, 0] = kapi.XCOOR;
                                hierarchy[6, 1] = kapi.YCOOR;
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
                            var kapi = geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(cross[0].MahalleId.Value, cross[0].YolId.Value, selectedKapiId).FirstOrDefault();

                            #region kapı seçildi ise koordinat bilgisini almak için
                            if (kapi.XCOOR != "0" && kapi.XCOOR != null)
                            {
                                hierarchy[6, 0] = kapi.XCOOR;
                                hierarchy[6, 1] = kapi.YCOOR;
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
                        geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(mahalleId.Value, yolId.Value, endKapiId)
                            .FirstOrDefault();

                    #region kapı seçildi ise koordinat bilgisini almak için

                    if (kapi.XCOOR != "0" && kapi.XCOOR != null)
                    {
                        hierarchy[6, 0] = kapi.XCOOR;
                        hierarchy[6, 1] = kapi.YCOOR;
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

        //protected void VMTespit()
        //{
        //    if (cityId != 0)  //Eğer İL combobox'ı boş değil ise
        //    {
        //        string xCoor = "0";
        //        string yCoor = "0";
              
        //        //var cityPolygonHierarchies = geocoderService.GetIncludeSearchPolygonDataByIlId(cityId).ToList();
        //        //IList<TownPolygonHierarchy> townPolygonHierarchies = new List<TownPolygonHierarchy>();

        //        #region İlçeden varış merkezi tespiti için hangi ilçeden tespit yapılacağı kontrol ediliyor
        //        //HinterlandPOIData.TOWNPOLYGONHIERARCHYDataTable dtTownPolygonHierarchy = new HinterlandPOIData.TOWNPOLYGONHIERARCHYDataTable();

        //        //if (townId != 0)
        //        //{
        //        //    //dtTownPolygonHierarchy = new HinterlandMapProxy().GetSearchIlceDataByIlidandIlceid(CityId, TownId);
        //        //    townPolygonHierarchies = geocoderService.GetSearchIlceDataByIlidandIlceid(cityId, townId).ToList();
        //        //}
        //        #endregion


        //        int exist = 0;
        //        int upsearch = 0;

        //        for (int i = 6; i >= 0; i--)
        //        {
        //            if (hierarchy[i, 0].ToString() != "0")
        //            {
        //                if (exist == 1)
        //                {
        //                    break;
        //                }

        //                xCoor = hierarchy[i, 0];
        //                yCoor = hierarchy[i, 1];

        //                switch (i)
        //                {
        //                    case 0:  //İl bazında şube bulma

        //                        //if (upsearch == 0)
        //                        //{
        //                        //    if (cityPolygonHierarchies.Count > 0)  //Eğer seçili ilde arama yapılacaksa
        //                        //    {
        //                        //        if (cityPolygonHierarchies[0].IlId == cityId && cityPolygonHierarchies[0].Il == "1")
        //                        //        {
        //                        //            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(8, CityId.ToString());
        //                        //            polygonUnitType = procService.GetUnitByIdAndPolygonType(8, cityId.ToString());
        //                        //        }
        //                        //        else
        //                        //        {
        //                        //            //DataTable dt = new DataTable();
        //                        //            //polygonunit.Tables.Add(dt);
        //                        //        }
        //                        //        upsearch = 1;
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        //DataTable dt = new DataTable();
        //                        //        //polygonunit.Tables.Add(dt);
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    if (cityPolygonHierarchies.Count > 0)  //Eğer seçili ilde arama yapılacaksa
        //                        //    {
        //                        //        if (cityPolygonHierarchies[0].IlId == cityId && cityPolygonHierarchies[0].Il == "1")
        //                        //        {
        //                        //            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(8, CityId.ToString());
        //                        //            polygonUnitType = procService.GetUnitByIdAndPolygonType(8, cityId.ToString());
        //                        //        }
        //                        //        else
        //                        //        {
        //                        //            //DataTable dt = new DataTable();
        //                        //            //polygonunit.Tables.Add(dt);
        //                        //        }
        //                        //        upsearch = 1;
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        //DataTable dt = new DataTable();
        //                        //        //polygonunit.Tables.Add(dt);
        //                        //    }
        //                        //}


        //                        //if (polygonUnitType.Count > 0)  //seçili ilde arama yapıldıktan sonra içerisine denk gelen Şube polygonu var ise
        //                        //{
        //                        //    tespitSeviyesi = 1;
        //                        //    exist = 1;
        //                        //}
        //                        //else
        //                        //{
        //                        //    tespitSeviyesi = -1;
        //                        //}

        //                        break;
        //                    case 1:  //İlçe bazında şube bulma

        //                        //if (upsearch == 0)
        //                        //{
        //                        //    if (townPolygonHierarchies.Count > 0)  //Eğer seçili ilçede arama yapılacaksa
        //                        //    {
        //                        //        if (townPolygonHierarchies[0].Search == "1")
        //                        //        {
        //                        //            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(9, TownId.ToString());
        //                        //            polygonUnitType = procService.GetUnitByIdAndPolygonType(9, townId.ToString());

        //                        //        }
        //                        //        else
        //                        //        {
        //                        //            //DataTable dt = new DataTable();
        //                        //            //polygonunit.Tables.Add(dt);
        //                        //        }
        //                        //        upsearch = 1;
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        //DataTable dt = new DataTable();
        //                        //        //polygonunit.Tables.Add(dt);
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    if (townPolygonHierarchies.Count > 0)  //Eğer seçili ilçede arama yapılacaksa
        //                        //    {
        //                        //        if (townPolygonHierarchies[0].Search == "1")
        //                        //        {
        //                        //            //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(9, TownId.ToString());
        //                        //            polygonUnitType = procService.GetUnitByIdAndPolygonType(9, townId.ToString());

        //                        //        }
        //                        //        else
        //                        //        {
        //                        //            upsearch = 1;
        //                        //        }
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        upsearch = 1;
        //                        //    }
        //                        //}


        //                        //if (polygonUnitType.Count > 0)  //Seçili ilde arama yapıldıktan sonra içerisine denk gelen şube polygonu var ise
        //                        //{
        //                        //    tespitSeviyesi = 2;
        //                        //    exist = 1;
        //                        //}
        //                        break;

        //                    case 2:  //Mahalle bazında şube bulma
        //                        //if (xCoor != "9999999")
        //                        //{
        //                        //    //HinterlandPOIData.GEOLOC_MAHALLEDataTable dtGeoloc_Mahalle = new HinterlandMapProxy().GetGeolocMahalleDataByMahalleId((decimal)dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"]);

        //                        //    var geolocMahalles = geocoderService.GetGeolocMahalleDataByMahalleId(cross[0].MahalleId.Value).ToList();

        //                        //    if (geolocMahalles.Count > 0)  //Mahallenin polygonu var ise polygona denk gelen şubeleri bul
        //                        //    {
        //                        //        //polygonunit = new HinterlandMapProxy().GetUnitIdByIDAndPolygonType(10, dtCross.DefaultView.ToTable().Rows[0]["MAHALLE_ID"].ToString());
        //                        //        polygonUnitType = procService.GetUnitByIdAndPolygonType(10, cross[0].MahalleId.Value.ToString());
        //                        //    }
        //                        //    else //Mahallenin polygonu yok ise mahalle koordinatının denk geldiği şubeyi bul
        //                        //    {
        //                        //        //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
        //                        //        if (xCoor != null)
        //                        //            polygonLists = geocoderService.GetXyCoorAndPolygonTypeId(xCoor, yCoor, 3).ToList();
        //                        //    }

        //                        //    if (polygonUnitType.Count > 0 || polygonLists.Count > 0) //Eğer mahallenin denk geldiği şube polygonu var ise
        //                        //    {
        //                        //        tespitSeviyesi = 3;
        //                        //        exist = 1;
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    upsearch = 1;
        //                        //}

        //                        break;

        //                    case 3:  //Cadde bazında şube bulma

        //                        //if (xCoor != "9999999")
        //                        //{
        //                        //    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
        //                        //    if (xCoor != null)
        //                        //        polygonLists = geocoderService.GetXyCoorAndPolygonTypeId(xCoor, yCoor, 3).ToList();

        //                        //    if (polygonLists.Count > 0)  //Eğer Caddenin denk geldiği şube polygonu var ise
        //                        //    {
        //                        //        tespitSeviyesi = 4;
        //                        //        exist = 1;
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    upsearch = 1;
        //                        //}
        //                        break;

        //                    case 4:  //Sokak bazında şube bulma

        //                        //if (xCoor != "9999999")
        //                        //{
        //                        //    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
        //                        //    if (xCoor != null)
        //                        //        polygonLists = geocoderService.GetXyCoorAndPolygonTypeId(xCoor, yCoor, 3).ToList();

        //                        //    if (polygonLists.Count > 0)  //Eğer sokağın denk geldiği şube polygonu var ise
        //                        //    {
        //                        //        tespitSeviyesi = 4;
        //                        //        exist = 1;
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    upsearch = 1;
        //                        //}
        //                        break;

        //                    case 5:  //POI bazında şube bulma

        //                        //if (xCoor != "9999999")
        //                        //{
        //                        //    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
        //                        //    if (xCoor != null)
        //                        //        polygonLists = geocoderService.GetXyCoorAndPolygonTypeId(xCoor, yCoor, 3).ToList();

        //                        //    if (polygonLists.Count > 0)  //Eğer POI'nin denk geldiği şube polygonu var ise
        //                        //    {
        //                        //        tespitSeviyesi = 5;
        //                        //        exist = 1;
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    upsearch = 1;
        //                        //}
        //                        break;

        //                    case 6: //Kapı Numarası bazında şube bulma

        //                        //if (xCoor != "9999999")
        //                        //{
        //                        //    //polygonunit = new HinterlandMapProxy().GetUnitIdByCRAndPolygonType(XCoor, YCoor, 3);
        //                        //    if (xCoor != null)
        //                        //        polygonLists = geocoderService.GetXyCoorAndPolygonTypeId(xCoor, yCoor, 3).ToList();

        //                        //    if (polygonLists.Count > 0)  //Eğer Kapı Numarasının denk geldiği şube polygonu var ise
        //                        //    {
        //                        //        tespitSeviyesi = 6;
        //                        //        exist = 1;
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    upsearch = 1;
        //                        //}
        //                        break;
        //                }

        //            }
        //        }

        //        //if (exist != 0) //Eğer koordinat bilgisi içeriyorsa polygon araması yapsın
        //        //{
        //        //    if (polygonLists.Count > 0)
        //        //    //Eğer Mahalle/Yol/Birim(POI) seçildikten sonra Koordinat bilgisi polygonlist tablosunda karşılık gelen bir şubeID'si var ise
        //        //    {
        //                //table = new DataTable();
        //                //table.Columns.Add("UNITID", typeof(Int32));
        //                //table.Columns.Add("UNIT_NAME", typeof(string));

        //                //for (int i = 0; i < polygonLists.Count; i++) //Denk gelen bütün şubelerin ismini yazdırma
        //                //{
        //                //    //Tablodan gelen ŞubeID'sinin ismini almak için UnitNameFromUnitId fonksiyonuna gönderiliyor
        //                //    string unitName = UnitNameFromUnitId(Convert.ToInt32(polygonLists[i].Unit.UnitId));

        //                //    if (unitName != "bos")
        //                //    //Eğer unit tablosunda tablodan gelen ŞubeID'si var ise (bu kontrol yeni eklenen şubeler olabilir ihtimaline karşı koyulmuştur)
        //                //    {
        //                //        table.Rows.Add(Convert.ToInt32(polygonLists[i].Unit.UnitId), unitName);
        //                //    }
        //                //}
        //            //}
        //            //else if (polygonUnitType.Count > 0)
        //            //{
        //            //    table = new DataTable();
        //            //    table.Columns.Add("UNITID", typeof(Int32));
        //            //    table.Columns.Add("UNIT_NAME", typeof(string));

        //            //    for (int i = 0; i < polygonUnitType.Count; i++) //Denk gelen bütün şubelerin ismini yazdırma
        //            //    {
        //            //        //Tablodan gelen ŞubeID'sinin ismini almak için UnitNameFromUnitId fonksiyonuna gönderiliyor
        //            //        string unitName = UnitNameFromUnitId(Convert.ToInt32(polygonUnitType[i].UnitId));

        //            //        if (unitName != "bos")
        //            //        //Eğer unit tablosunda tablodan gelen ŞubeID'si var ise (bu kontrol yeni eklenen şubeler olabilir ihtimaline karşı koyulmuştur)
        //            //        {
        //            //            table.Rows.Add(Convert.ToInt32(polygonUnitType[i].UnitId), unitName);
        //            //        }
        //            //    }
        //            //}
        //            //else //Eğer Mahalle/Yol/Birim(POI) seçildikten sonra Koordinat bilgisi polygonlist tablosunda karşılık gelen bir şubeID'si yok ise
        //            //{
        //            //    table = new DataTable();
        //            //}
        //        //}
        //        //else
        //        //{
        //        //    table = new DataTable();
        //        //}


        //    }
        //    //else  // Eğer Mahalle/Yol/Birim(POI) bilgisi boş ise varış şubesi alanı boşaltılsın
        //    //{
        //    //    table = new DataTable();
        //    //}
        //}
        

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
