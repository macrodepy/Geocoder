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
    public class Geocoder : IParsing
    {
        private readonly System.Globalization.CultureInfo cultureInfo;
        private readonly GeocoderService geocoderService;
        private AddressLevel addressLevel;

        private IList<VUnitSearch> vUnitSearches;
        private IList<KAPI> kapiList;
        int veriTipiId = -1;
        string[,] hierarchy;
        private readonly bool integrationSave;

        public Geocoder()
        {
            cultureInfo = new System.Globalization.CultureInfo("tr-TR");
            addressLevel = new AddressLevel();
            geocoderService = new GeocoderService();
            hierarchy = new string[7, 2];
            integrationSave = false;
        }

        public AddressLevel IntegrationParsing(AddressLevel addressLevel)
        {
            this.addressLevel = addressLevel;

            try
            {
                AddresGeocode();
            }
            catch (Exception ex)
            {
                //throw;
                Log(ex);
            }

            for (var i = 6; i >= 0; i--)
            {
                if (hierarchy[i, 0] != "9999999")
                {
                    addressLevel.XCoor = hierarchy[i, 0];
                    addressLevel.YCoor = hierarchy[i, 1];
                    addressLevel.CoordinateLevel = i;
                    break;
                }
            }

            return addressLevel;
        }

        private void Log(Exception ex)
        {
            //TODO: Loglama yazılacak
        }

        public void AddresGeocode()
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    hierarchy[i, j] = "9999999";
                }
            }

            IList<HINTCITYGEOCITYCR> ilIdDataByHintCityGeoCityCr = geocoderService.GetIL_IDDataByHintCityGeoCityCR(addressLevel.Il.ToUpper(cultureInfo).Trim()).ToList();
            int flag = 0;

            if (ilIdDataByHintCityGeoCityCr.Count > 0)
            {
                var hintCityGeoCity = ilIdDataByHintCityGeoCityCr.FirstOrDefault();
                if (hintCityGeoCity.GEOLOC_IL_ID != null)  //tabloda geoloc_il_id adında kolon var ise
                {
                    var cityId = hintCityGeoCity.GEOLOC_IL_ID.Value;  //İl ID'si atanıyor

                    #region İl Seçilirse İl'in koordinat bilgisi aktarılır
                    var il = geocoderService.GetIlDataByIL_ID(cityId).FirstOrDefault();

                    if (il.XCOOR != null || il.XCOOR != "0")
                    {
                        addressLevel.ILModel = il;

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
                            var townId = hintTownGeoTown.GEOLOC_ILCE_ID.Value;  //İlçe Id'si combodan seçiliyor

                            #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                            var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();

                            if (ilce.XCOOR != "0")
                            {
                                addressLevel.IlceModel = ilce;

                                hierarchy[1, 0] = ilce.XCOOR;
                                hierarchy[1, 1] = ilce.YCOOR;
                            }
                            else
                            {
                                hierarchy[1, 0] = "9999999";
                                hierarchy[1, 1] = "9999999";
                            }
                            #endregion


                            while (true)
                            {
                                string searchText = "999999999";

                                searchText = SearchAddressType(flag);

                                if (flag == 29)
                                {
                                    break;
                                }

                                if (searchText == "999999999")  //Eğer birşey dönmedi ise döngüye devam et
                                {
                                    flag += 1;
                                    continue;
                                }

                                string replaceText = SearchStringFixer(searchText.ToUpper(cultureInfo));

                                vUnitSearches = geocoderService.GetUnitSearchDataByIlAndIlceId(replaceText,
                                    hintTownGeoTown.GEOLOC_ILCE_ID.Value);

                                if (vUnitSearches != null && vUnitSearches.Count > 0)  //Eğer veritabanında adres bulundu ise
                                {
                                    if (vUnitSearches.Count > 0 && vUnitSearches.Count < 2) //Eğer adresin karşılığı tek kayıt ise
                                    {
                                        SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış
                                        flag += 1;
                                    }
                                    else if (vUnitSearches.Count > 1)  //Eğer birden fazla kayıt dönerse tipe göre filtre kullan
                                    {
                                        switch (veriTipiId)
                                        {
                                            case (int)Enums.VeriTipi.Mahalle:  //Parser'dan gelen bilgi mahalle ise mahallelerde ara
                                                vUnitSearches = vUnitSearches.Where(x => x.MahalleId != null && x.YolId == null && x.PoiId == null).ToList();
                                                break;
                                            case (int)Enums.VeriTipi.Cadde:  //Parser'dan gelen bilgi Cadde ise caddelerde ara
                                                vUnitSearches = vUnitSearches.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                                break;
                                            case (int)Enums.VeriTipi.Sokak:  //Parser'dan gelen bilgi Sokak ise sokaklarda ara
                                                vUnitSearches = vUnitSearches.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                                break;
                                            case (int)Enums.VeriTipi.POI: //Parser'dan gelen bilgi POI ise POI'lerde ara
                                                vUnitSearches = vUnitSearches.Where(x => x.YolId == null && x.PoiId != null).ToList();

                                                break;
                                        }

                                        if (vUnitSearches.Count > 0 && vUnitSearches.Count < 2) //Eğer adresin karşılığı tek kayıt ise
                                        {
                                            SearchAddress();  //Gelen adresi ayrıştır ve VM tespiti yapmaya çalış
                                            flag += 1;
                                        }
                                        else if (vUnitSearches.Count > 1)  //Eğer adresin karşılığı birden fazla kaıt ise
                                        {
                                            bool sameAddress = true;
                                            string firstRecord = vUnitSearches.FirstOrDefault().SonucIlceli;
                                            for (int q = 1; q < vUnitSearches.Count; q++)
                                            {
                                                if (vUnitSearches.Any(search => firstRecord != search.SonucIlceli))
                                                {
                                                    sameAddress = false;
                                                }
                                            }

                                            if (sameAddress)  //Eğer tüm kayıtlar birebir aynı ise 
                                            {
                                                SearchAddress();  //Gelen adresi ayrıştır ve kordinat tespiti yapmaya çalış
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
                    else
                    {
                        while (true)
                        {
                            string searchText = "999999999";

                            searchText = SearchAddressType(flag);

                            if (flag == 29)
                                break;

                            if (searchText == "999999999")  //Eğer birşey dönmedi ise döngüye devam et
                            {
                                flag += 1;
                                continue;
                            }

                            string replaceText = SearchStringFixer(searchText.ToUpper(cultureInfo));
                            vUnitSearches = geocoderService.GetUnitSearchDataByIlId(replaceText, hintCityGeoCity.GEOLOC_IL_ID.Value).ToList();

                            if (vUnitSearches != null && vUnitSearches.Count > 0)
                            {
                                if (vUnitSearches.Count > 0 && vUnitSearches.Count < 2)
                                {
                                    #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                    if (!String.IsNullOrEmpty(vUnitSearches.FirstOrDefault().IlceId.ToString()))
                                    {
                                        var townId = vUnitSearches.FirstOrDefault().IlceId;

                                        var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();
                                        if (ilce.XCOOR != "0")
                                        {
                                            addressLevel.IlceModel = ilce;
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
                                    flag += 1;
                                }
                                else if (vUnitSearches.Count > 1)  //Eğer birden fazla kayıt dönerse tipe göre filtre kullan
                                {
                                    switch (veriTipiId)
                                    {
                                        case (int)Enums.VeriTipi.Mahalle:
                                            vUnitSearches = vUnitSearches.Where(x => x.MahalleId != null && x.YolId == null && x.PoiId == null).ToList();
                                            break;
                                        case (int)Enums.VeriTipi.Cadde:
                                            vUnitSearches = vUnitSearches.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                            break;
                                        case (int)Enums.VeriTipi.Sokak:
                                            vUnitSearches = vUnitSearches.Where(x => x.YolId != null && x.PoiId == null).ToList();
                                            break;
                                        case (int)Enums.VeriTipi.POI:
                                            vUnitSearches = vUnitSearches.Where(x => x.YolId == null && x.PoiId != null).ToList();
                                            break;
                                    }

                                    if (vUnitSearches.Count == 1) //Eğer adresin karşılığı tek kayıt ise
                                    {
                                        #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                        if (!String.IsNullOrEmpty(vUnitSearches.FirstOrDefault().IlceId.ToString()))
                                        {
                                            var townId = vUnitSearches.FirstOrDefault().IlceId;
                                            var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();

                                            if (ilce.XCOOR != "0")
                                            {
                                                addressLevel.IlceModel = ilce;
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
                                        flag += 1;
                                    }
                                    else if (vUnitSearches.Count > 1)
                                    {

                                        bool sameAddress = true;
                                        string firstRecord = vUnitSearches.FirstOrDefault().SonucIlceli;  //Sonuç ilçeliden bakılmasının nedeni tüm kayıtarın birebir aynı olmasına bakılmasıdır

                                        if (vUnitSearches.Any(search => firstRecord != search.SonucIlceli))
                                        {
                                            sameAddress = false;
                                        }

                                        if (sameAddress)  //Eğer tüm kayıtlar birebir aynı ise 
                                        {
                                            #region İlçe Seçilirse ilçenin koordinat bilgisi alnır
                                            if (!String.IsNullOrEmpty(vUnitSearches.FirstOrDefault().IlceId.ToString()))
                                            {
                                                var townId = vUnitSearches.FirstOrDefault().IlceId;

                                                //HinterlandPOIData.ILCEDataTable dtIlce = new HinterlandMapProxy().GetIlceDataByILCE_ID(TownId);
                                                var ilce = geocoderService.GetIlceDataByILCE_ID(townId).FirstOrDefault();
                                                if (ilce.XCOOR != "0")
                                                {
                                                    addressLevel.IlceModel = ilce;

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

        protected string SearchAddressType(int flag)
        {
            //Aranacak adres string'i
            string returntext = "";
            veriTipiId = -1;

            //Aramayı kaçıncı kez yapıyorsak ona göre aranacak string değişiyor
            switch (flag)
            {
                case 0:
                    if (addressLevel.Poi.Trim() != "")
                    {
                        returntext = addressLevel.Poi.Trim();
                        veriTipiId = (int)Enums.VeriTipi.POI;
                    }
                    break;
                case 1:
                    if (addressLevel.Poi.Trim() != "" && addressLevel.Blok.Trim() != "")
                    {
                        returntext = addressLevel.Poi.Trim() + addressLevel.Blok.Trim();
                        veriTipiId = (int)Enums.VeriTipi.POI;
                    }
                    break;
                case 2:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Poi.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ" + addressLevel.Poi.Trim();
                        veriTipiId = (int)Enums.VeriTipi.POI;
                    }
                    break;
                case 3:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Poi.Trim() != "" && addressLevel.Blok.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ" + addressLevel.Poi.Trim() + addressLevel.Blok.Trim();
                        veriTipiId = (int)Enums.VeriTipi.POI;
                    }
                    break;

                case 10:
                    if (addressLevel.Yolu.Trim() != "")
                    {
                        returntext = addressLevel.Yolu.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 11:
                    if (addressLevel.Sokak.Trim() != "")
                    {
                        returntext = addressLevel.Sokak.Trim() + "SOKAK";
                        veriTipiId = (int)Enums.VeriTipi.Sokak;
                    }
                    break;
                case 12:
                    if (addressLevel.Sokak1.Trim() != "" && addressLevel.Sokak2.Trim() != "")
                    {
                        returntext = addressLevel.Sokak1.Trim() + " ";
                        returntext += addressLevel.Sokak2.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Sokak;
                    }
                    break;
                case 13:
                    if (addressLevel.Cadde.Trim() != "")
                    {
                        returntext = addressLevel.Cadde.Trim() + "CADDESİ";
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 14:
                    if (addressLevel.Cadde1.Trim() != "" && addressLevel.Cadde2.Trim() != "")
                    {
                        returntext = addressLevel.Cadde1.Trim() + " ";
                        returntext += addressLevel.Cadde2.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 15:
                    if (addressLevel.Bulvar.Trim() != "")
                    {
                        returntext = addressLevel.Bulvar.Trim() + "BULVARI";
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 16:
                    if (addressLevel.Bulvar1.Trim() != "" && addressLevel.Bulvar2.Trim() != "")
                    {
                        returntext = addressLevel.Bulvar1.Trim() + " ";
                        returntext += addressLevel.Bulvar2.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 17:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Yolu.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Yolu.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 18:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Sokak.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Sokak.Trim() + "SOKAK";
                        veriTipiId = (int)Enums.VeriTipi.Sokak;
                    }
                    break;
                case 19:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Sokak.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Sokak.Trim() + "%" + "SOKAK";
                        veriTipiId = (int)Enums.VeriTipi.Sokak;
                    }
                    break;
                case 20:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Sokak1.Trim() != "" && addressLevel.Sokak2.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Sokak1.Trim() + " ";
                        returntext += addressLevel.Sokak2.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Sokak;
                    }
                    break;
                case 21:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Cadde.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Cadde.Trim() + "CADDESİ";
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 22:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Cadde.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Cadde.Trim() + "%" + "CADDESİ";
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;

                case 23:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Cadde1.Trim() != "" && addressLevel.Cadde2.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Cadde1.Trim() + " ";
                        returntext += addressLevel.Cadde2.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 24:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Bulvar.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Bulvar.Trim() + "BULVARI";
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 25:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Bulvar.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Bulvar.Trim() + "%" + "BULVARI";
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 26:
                    if (addressLevel.Mahalle.Trim() != "" && addressLevel.Bulvar1.Trim() != "" && addressLevel.Bulvar2.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        returntext += addressLevel.Bulvar1.Trim() + " ";
                        returntext += addressLevel.Bulvar2.Trim();
                        veriTipiId = (int)Enums.VeriTipi.Cadde;
                    }
                    break;
                case 27:
                    if (addressLevel.Köy.Trim() != "")
                    {
                        returntext = addressLevel.Köy.Trim() + "KÖYÜ";
                        veriTipiId = (int)Enums.VeriTipi.Mahalle;
                    }
                    break;
                case 28:
                    if (addressLevel.Mahalle.Trim() != "")
                    {
                        returntext = addressLevel.Mahalle.Trim() + "MAHALLESİ";
                        veriTipiId = (int)Enums.VeriTipi.Mahalle;
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
            long yolId = 0;
            long mahalleId = 0;
            long poiId = 0;
            var model = vUnitSearches.FirstOrDefault();

            if (!String.IsNullOrEmpty(model.YolId.ToString())) //yol_ID'sini almak için
            {
                yolId = model.YolId.Value;
            }
            if (!String.IsNullOrEmpty(model.MahalleId.ToString())) //mahalle_id'sini almak için
            {
                mahalleId = model.MahalleId.Value;
            }
            if (!String.IsNullOrEmpty(model.PoiId.ToString())) //poi_id'sini almak için
            {
                poiId = model.PoiId.Value;
            }

            var idariYolSınırs = geocoderService.GetIdariYolDataByMahalleAndYolId(yolId, mahalleId).ToList();

            var mahalles = geocoderService.GetMahalleDataByMAHALLE_ID(mahalleId).ToList();

            var pois = geocoderService.GetPOIDataByPOI_ID(poiId).ToList();

            if (mahalles.Count > 0)
            {
                var mahalle = mahalles.FirstOrDefault();

                #region Mahalle Var ise Koordinat Bilgisini almak için
                //mahallenin x - y koordinatını yazdırmak için (hierarchy dizisi burada sıfır gelme durumunda bir önceki hiyerarşiye bakılmasını sağlamak için tutulmaktadır)                    
                if (mahalle.XCOOR != "0")
                {
                    addressLevel.MahalleModel = mahalle;

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
                        #region Sokak var ise koordinat bilgisini almak için

                        if (idariSınırYol.XCoor != "0")
                        {
                            addressLevel.SokakModel = idariSınırYol;

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
                        #region Cadde var ise koordinat bilgisini almak için

                        if (idariSınırYol.XCoor != "0")
                        {
                            addressLevel.CaddeModel = idariSınırYol;

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

                    if (!string.IsNullOrEmpty(addressLevel.Cadde) || !string.IsNullOrEmpty(addressLevel.Sokak)) //eğer cadde veya sokak bilgisi girildi ise
                    {
                        kapiList = geocoderService.GetKapiDataByYOL_IDAndMAHALLE_ID(mahalleId, yolId);

                        if (kapiList.Count > 0)
                        {
                            if (addressLevel.Kapı != string.Empty)
                            {
                                kapiList = kapiList.Where(x => x.KAPI_NO == addressLevel.Kapı).ToList();

                                if (kapiList.Count == 1)
                                {
                                    hierarchy[6, 0] = kapiList.First().XCOOR;
                                    hierarchy[6, 1] = kapiList.First().YCOOR;
                                    addressLevel.KapıModel = kapiList.First();
                                }
                            }
                        }
                    }
                }
            }

            if (pois.Count > 0)  //(birim) POI bilgisi 
            {
                var poi = pois.FirstOrDefault();

                #region POI varsa koordinat bilgisini almak için

                if (poi.XCOOR != "0")
                {
                    addressLevel.PoiModel = poi;

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

            if (addressLevel.Kapı.Trim() == "")  //Eğer kapı numarası parser'dan gelmedi ise
            {
                if (kapiList != null)
                {
                    int KapiCount = kapiList.Count;
                    var kapi = kapiList.FirstOrDefault();

                    if (KapiCount == 1)  //Eğer sistemde girilen yol altında bir adet kapı var ise ve parser'dan kapı bilgisi gelmedi ise
                    {
                        searchDoorNumber();
                    }
                }
            }
        }

        public void searchDoorNumber()
        {
            if (addressLevel.Kapı.Trim() != "")
            {
                int counter = 0;  //el ile girilen kapı numarasının kapı combobox'ının içindeki bir kapı numarasına denk gelip gelmediğini anlamak için

                foreach (var kapi in kapiList)
                {
                    if (kapi.KAPI_NO != "") //eğer kapı numarası boş değil ise
                    {
                        if (addressLevel.Kapı == kapi.KAPI_NO)  //eğer girilen kapı numarası combobox'ın içindeki kapı numaralarından birine denk geliyor ise
                        {
                            SearchDoorNumberSelect((long)kapi.KAPI_ID);
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
                        string[] Ks = Regex.Split(addressLevel.Kapı, patternK);

                        string no = Ks[0];

                        int result = Convert.ToInt32((Regex.Replace(no, "[^0-9]", "") == "") ? "0" : (Regex.Replace(no, "[^0-9]", ""))); //el ile girilen kapı numarası değişkene aktarılıyor (girilebilecek diğer karakterleri ayıklama yaprak)

                        string originalKapı = kapiList[i].KAPI_NO;
                        int digitKapı = Convert.ToInt32((Regex.Replace(originalKapı, "[^0-9]", "") == "") ? "0" : (Regex.Replace(originalKapı, "[^0-9]", "")));


                        if (kapiList[i].KAPI_NO != "") //eğer kapı numarası boş değil ise
                        {
                            if (result.ToString() == originalKapı)  //eğer girilen kapı numarası combobox'ın içindeki kapı numaralarından birine denk geliyor ise
                            {
                                SearchDoorNumberSelect((long)kapiList[i].KAPI_ID);
                                counter = 1;
                                break;
                            }
                            else if (result.ToString() == digitKapı.ToString())
                            {
                                SearchDoorNumberSelect((long)kapiList[i].KAPI_ID);
                                counter = 1;
                                break;
                            }
                        }
                    }
                }

                if (counter == 0)  //eğer el ile girilen kapı numarası kapı combobox'ının içerisindeki(O mahallede) bir kapı numarasına denk gelmedi ise
                {
                    string patternK = @"(-)|(/)";
                    string[] Ks = Regex.Split(addressLevel.Kapı, patternK);

                    string no = Ks[0];

                    int result = Convert.ToInt32((Regex.Replace(no, "[^0-9]", "") == "") ? "0" : Regex.Replace(no, "[^0-9]", ""));
                    int cntr = DoorNumberSelect(result);

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

        private void SearchDoorNumberSelect(long Kapi_ID)
        {
            var kapi = geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(vUnitSearches[0].MahalleId.Value, vUnitSearches[0].YolId.Value, Kapi_ID).FirstOrDefault();

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
                            var kapi = geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(vUnitSearches[0].MahalleId.Value, vUnitSearches[0].YolId.Value, selectedKapiId).FirstOrDefault();

                            #region kapı seçildi ise koordinat bilgisini almak için

                            if (kapi.XCOOR != "0" && kapi.XCOOR != null)
                            {
                                addressLevel.KapıModel = kapi;

                                hierarchy[6, 0] = kapi.XCOOR;
                                hierarchy[6, 1] = kapi.YCOOR;
                            }
                            else
                            {
                                hierarchy[6, 0] = "9999999";
                                hierarchy[6, 1] = "9999999";
                            }
                            #endregion
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
                            var kapi = geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(vUnitSearches[0].MahalleId.Value, vUnitSearches[0].YolId.Value, selectedKapiId).FirstOrDefault();
                            #region kapı seçildi ise koordinat bilgisini almak için
                            if (kapi.XCOOR != "0" && kapi.XCOOR != null)
                            {
                                addressLevel.KapıModel = kapi;
                                hierarchy[6, 0] = kapi.XCOOR;
                                hierarchy[6, 1] = kapi.YCOOR;
                            }
                            else
                            {
                                hierarchy[6, 0] = "9999999";
                                hierarchy[6, 1] = "9999999";
                            }
                            #endregion

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
                var yolId = vUnitSearches[0].YolId;
                var mahalleId = vUnitSearches[0].MahalleId;

                if (yolId != null && mahalleId != null)
                {
                    var kapi =
                        geocoderService.GetKapiDataByMahalleIdandYolIdandKapiId(mahalleId.Value, yolId.Value, endKapiId)
                            .FirstOrDefault();

                    #region kapı seçildi ise koordinat bilgisini almak için

                    if (kapi.XCOOR != "0" && kapi.XCOOR != null)
                    {
                        addressLevel.KapıModel = kapi;

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

                cntr = 1;
            }

            return cntr;  // en yakın kapı seçiminin yapılıp yapılmadığı bilgisi geri döndürülüyor (eğer yapılamamış ise fonksiyon tekrar çağırılacak)
        }

        public static string SearchStringFixer(string searchText)
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
