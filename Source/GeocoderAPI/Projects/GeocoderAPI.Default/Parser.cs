using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GeocoderAPI.DAL;
using GeocoderAPI.Model;

namespace GeocoderAPI.Default
{
    public class Parser
    {
        public List<String> NotParsedList { get; set; }

        private void ClearNotParsedList(string token)
        {
            if (NotParsedList.Any(x => x.Equals(token)))
                NotParsedList.Remove(token);
        }

        public AddressLevel ParseAddress(string address)
        {
             int counter = 0;
             int index = 0;

            NotParsedList = new List<string>();

            string[] tokens = Regex.Split(address, " ");

            AddressLevel addressLevel = new AddressLevel();
            addressLevel.OriginalAddress = address;

            while (counter < tokens.Length)
            {
                string tempAddress = " " + tokens[counter] + " ";

                int type = CheckType(tempAddress, counter, tokens);
                string token = string.Empty;

                switch (type)
                {
                    case (int)Enums.ParsingAdress.Mahalle:
                        for (int i = index; i < counter; i++)
                        {
                            token += " " + tokens[i] + " ";
                        }
                        addressLevel.Mahalle = token.Replace("  ", " ").TrimStart().TrimEnd();
                        index = counter + 1;
                        ClearNotParsedList(token);
                        break;
                    case (int)Enums.ParsingAdress.Cadde:
                        if (addressLevel.Cadde == "")
                        {
                            for (int i = index; i < counter; i++)
                            {
                                token += " " + tokens[i] + " ";
                            }
                            addressLevel.Cadde = token.Replace("  ", " ").TrimStart().TrimEnd();
                            ClearNotParsedList(token);

                            string[] caddeLength = Regex.Split(token.Replace("  ", " ").TrimStart().TrimEnd(), " ");

                            if (caddeLength.Length > 1)
                            {
                                for (int i = 0; i < caddeLength.Length; i++)
                                {
                                    ClearNotParsedList(caddeLength[i]);

                                    if (i == 0)
                                    {
                                        addressLevel.Cadde1 = caddeLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i == 1)
                                    {
                                        addressLevel.Cadde2 = caddeLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i > 1)
                                    {
                                        addressLevel.CaddeS += " " + caddeLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                }
                            }
                        }
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Sokak:
                        if (addressLevel.Sokak == "")
                        {
                            for (int i = index; i < counter; i++)
                            {
                                token += " " + tokens[i] + " ";
                            }
                            addressLevel.Sokak = token.Replace("  ", " ").TrimStart().TrimEnd();
                            ClearNotParsedList(token);

                            string[] sokakLength = Regex.Split(token.Replace("  ", " ").TrimStart().TrimEnd(), " ");

                            if (sokakLength.Length > 1)
                            {
                                for (int i = 0; i < sokakLength.Length; i++)
                                {
                                    ClearNotParsedList(sokakLength[i]);

                                    if (i == 0)
                                    {
                                        addressLevel.Sokak1 = sokakLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i == 1)
                                    {
                                        addressLevel.Sokak2 = sokakLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i > 1)
                                    {
                                        addressLevel.SokakS += " " + sokakLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                }
                            }
                        }
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.POI:
                        if (addressLevel.Poi == "")
                        {
                            for (int i = index; i <= counter; i++)
                            {
                                token += " " + tokens[i] + " ";
                            }

                            token = token.Replace(" İŞ.MRK. ", " İŞ MERKEZİ ");
                            token = token.Replace(" TİC.MRK. ", " TİCARET MERKEZİ ");
                            token = token.Replace(" TİC.AŞ. ", " TİCARET ");
                            token = token.Replace(" ORG.SAN. ", " ORGANİZE SANAYİ ");
                            token = token.Replace(" RENT.A.CAR ", " RENT A CAR ");
                            token = token.Replace(" TATİL.KÖYÜ ", " TATİL KÖYÜ ");
                            token = token.Replace(" VİLLALARİ ", " VİLLALARI ");

                            addressLevel.Poi = token.Replace("  ", " ").TrimStart().TrimEnd();
                            ClearNotParsedList(token);

                            string[] poiLength = Regex.Split(token.Replace("  ", " ").TrimStart().TrimEnd(), " ");

                            if (poiLength.Length > 1)
                            {
                                for (int i = poiLength.Length - 1; i >= 0; i--) //Sondan kırpma işlemi
                                {
                                    ClearNotParsedList(poiLength[i]);

                                    if (i == 0)
                                    {
                                        addressLevel.Poi1 = poiLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i == 1)
                                    {
                                        addressLevel.Poi2 = poiLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i == 2)
                                    {
                                        addressLevel.Poi3 = poiLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i > 2)
                                    {
                                        addressLevel.PoiS += " " + poiLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                }
                            }
                        }
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Yolu:
                        for (int i = index; i <= counter; i++)
                        {
                            token += " " + tokens[i] + " ";
                        }
                        addressLevel.Yolu = token.Replace(".GİRİŞİ", "").Replace(".KAVŞAĞI", " KAVŞAĞI").Replace("MEVKİİ", "MEVKİ").Replace(".ÜZERİ", "").Replace("  ", " ").TrimStart().TrimEnd();
                        ClearNotParsedList(token);
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Köy:
                        for (int i = index; i < counter; i++)
                        {
                            token += " " + tokens[i] + " ";
                        }
                        addressLevel.Köy = token.Replace("  ", " ").TrimStart().TrimEnd();
                        ClearNotParsedList(token);
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Bulvar:
                        if (addressLevel.Bulvar == "")
                        {
                            for (int i = index; i < counter; i++)
                            {
                                token += " " + tokens[i] + " ";
                            }

                            addressLevel.Bulvar = token.Replace("  ", " ").TrimStart().TrimEnd();
                            ClearNotParsedList(token);

                            string[] bulvarLength = Regex.Split(token.Replace("  ", " ").TrimStart().TrimEnd(), " ");

                            if (bulvarLength.Length > 1)
                            {
                                for (int i = 0; i < bulvarLength.Length; i++)
                                {
                                    ClearNotParsedList(bulvarLength[i]);

                                    if (i == 0)
                                    {
                                        addressLevel.Bulvar1 = bulvarLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i == 1)
                                    {
                                        addressLevel.Bulvar2 = bulvarLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                    else if (i > 1)
                                    {
                                        addressLevel.BulvarS += " " + bulvarLength[i].Replace("  ", " ").TrimStart().TrimEnd();
                                    }
                                }
                            }
                        }
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Kapı:
                        addressLevel.Kapı = tokens[counter].Replace("NO:", "").Replace("  ", " ").TrimStart().TrimEnd();
                        ClearNotParsedList(token);
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Kat:

                        tokens[counter] = tokens[counter].Replace("K:", "").Replace("  ", " ").TrimStart().TrimEnd();

                        string patternK = @"(-)|(/)";
                        string[] Ks = Regex.Split(tokens[counter], patternK);

                        addressLevel.Kat = Ks[0];
                        ClearNotParsedList(Ks[0]);
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Daire:

                        tokens[counter] = tokens[counter].Replace("D:", "").Replace("  ", " ").TrimStart().TrimEnd();

                        string patternD = @"(-)|(/)";
                        string[] Ds = Regex.Split(tokens[counter], patternD);

                        addressLevel.Daire = Ds[0];
                        ClearNotParsedList(Ds[0]);
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Blok:
                        for (int i = index; i < counter; i++)
                        {
                            token += " " + tokens[i] + " ";
                        }
                        addressLevel.Blok = token.Replace("  ", " ").TrimStart().TrimEnd();
                        ClearNotParsedList(token);
                        index = counter + 1;
                        break;
                    case (int)Enums.ParsingAdress.Bina:
                        for (int i = index; i <= counter; i++)
                        {
                            token += " " + tokens[i] + " ";
                        }
                        addressLevel.Bina = token.Replace("  ", " ").TrimStart().TrimEnd();
                        ClearNotParsedList(token.Replace(" APT. ", ""));
                        index = counter + 1;
                        break;
                    default:
                        if (!tempAddress.Equals(string.Empty))
                        {
                            NotParsedList.Add(tempAddress);
                        }
                        break;
                }

                counter ++;
            }

            return addressLevel;
        }

        private int CheckType(string mTempAddress, int counter,  string[] tokens)
        {
            int CheckType = 0;
            if (" MAHALLESİ ".IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Mahalle;
            }
            else if (" CADDESİ ".IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Cadde;
            }
            else if (" SOKAK ".IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Sokak;
            }
            else if (
                " KISIM ÇARŞISI ÇARŞI GARAJI İLKOKULU ILKOKULU LİSESİ LİSE MÜD MUD MD MÜDÜRLÜĞÜ MUDURLUGU ALTI KARAKOLU KOMUTANLIĞI ORG.SAN. İÇİ ÖNÜ ARKASI KARŞISI YANI DÜK. ALAYI TUGAYI BANKASI BANK ŞB ÜSTÜ DERSANESİ CENTER SANAYİİ  İŞ.MRK. PLAZA TİC.MRK. İŞHANI ISHANI HANI HN PASAJI PSJ OTEL HOTEL OTELİ OTELI  SİTESİ SİT BLOKLARI BLOKL ECZANE ECZANESİ DEPOSU TİC.AŞ. MERKEZİ SANTRALİ KULÜBESİ CAMİSİ HAVRASI KİLİSESİ MANASTIRI MESCİDİ PATRİKHANESİ SİNAGOGU MEZARLIĞI ŞEHİTLİĞİ TÜRBESİ DERHANESİ KURSU İLKOKULU LİSESİ ANAOKULU KREŞİ ÜNİVERSİTESİ YURDU ARSASI KONUTU KOOPERATİFİ ANITI RESİDANCE RESIDANCE REZİDANSI RESİDANSI RESIDANSI KÜTÜPHANESİ GALERİSİ SALONU SİNEMASI TİYATROSU HAVUZU MEYDANI BELDESİ ADLİYESİ BAROSU MAHKEMESİ BİNASI KAYMAKAMLIĞI MUHTARLIĞI POSTANESİ VALİLİĞİ CEZAEVİ AMİRLİĞİ KARAKOLU TESİSİ KONSOLOSLUĞU PARTİSİ KURUMU BORSASI ODASI KIŞLASI KOMUTANLIĞI TESİSİ ŞUBESİ BAŞKANLIĞI ZABITASI HASTANESİ HASTAHANESİ KILİNİĞİ KLİNİĞİ LABORATUVARI LABARATUVARI LABORATUARI LABARATUARI POLİKLİNİĞİ DİSPANSERİ OCAĞI SANATORYUMU VETERİNER TERSANESİ ŞANTİYESİ FABRİKASI KESİMHANESİ RAFİNERİSİ LOJMANI LOKALİ VAKFI EVİ YETİMHANESİ HİPERMARKET MAĞAZASI SÜPERMARKET MARKET PLAZASI PLAZA ŞUBESİ BÜROSU ŞİRKETİ ACENTESİ ACENTA ACENTE BÜRO MÜŞAVİRLİĞİ MÜŞAVİRLİK MÜHENDİSLİK NOTERLİĞİ NOTERLİK AJANS AJANSI GAZİNOSU BAR KAFE CAFE KAFESİ BAHÇESİ KULÜBÜ KULÜP KAHVEHANESİ KAHVESİ KIRAATHANESİ SALONU BAYİİ BAYİSİ DÜKKANI ÇİLİNGİR AYDINLATMA MANİFATURACI MANİFATURA MOBİLYA TESİSAT KUYUMCUSU KUYUMCULUK SAAT OPTİK ANTREPO ANTREPOSU BASIMEVİ MATBAASI BERBERİ BUJİTERİ KUAFOR KUAFÖR PERFÜMERİ STÜDYOSU BAKIMEVİ RESTORANT RESTAURANT RESTORAN BÜFE BÜFESİ SALONU MODAEVİ TEMİZLEME KONFEKSİYON TERZİSİ FIRINI MANAVI MARKETİ PASTANESİ PAZARI TAVUKÇULUK BAYİ YUFKACI HALİ KONUKEVİ MOTEL OTEL OTELİ MOTELİ PANSİYON PANSİYONU MÜZESİ İSKELESİ LİMANI MARİNA MARİNASI HAVAALANI KÖPRÜSÜ GİŞESİ İSTASYONU DURAĞI KİRALAMA PARKI TÜNELİ GEÇİDİ OTOPARKI OTOPARK GARI FİRMASI LÜNAPARKI LUNAPARKI PLAJI AQUAPARK KAPLICASI SAHASI HİPODROMU STADYUMU HAVUZU SAHASI PİSTİ PARKI ORMANI ALANI RENT.A.CAR BELEDİYESİ OKULU SİTELERİ VİLLALARI VİLLALARİ GRUBU OKULLARI TEKNOPARKI EVLERİ LOJMANLARI KONUTLARI KONUTLARİ "
                    .IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.POI;
            }
            else if (
                " YOLU ASFALTI KAVŞAĞI KM YOLU.GİRİŞİ ÇIKMAZI KÖYÜ.KAVŞAĞI MEVKİİ YOLU.ÜZERİ ".IndexOf(
                    mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Yolu;
            }
            else if (" KÖYÜ ".IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Köy;
            }
            else if (" BULVARI ".IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Bulvar;
            }
            else if (mTempAddress.Replace(" NO:", " NO: ").ToUpper().IndexOf(" NO: ") > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Kapı;
            }
            else if (mTempAddress.Replace(" K:", " K: ").ToUpper().IndexOf(" K: ") > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Kat;
            }
            else if (mTempAddress.Replace(" D:", " D: ").ToUpper().IndexOf(" D: ") > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Daire;
            }
            else if (" BLOK -BLOK ".IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Blok;
            }
            else if (" APT. ".IndexOf(mTempAddress.ToUpper()) > -1)
            {
                CheckType = (int)Enums.ParsingAdress.Bina;
            }
            else if ((mTempAddress.ToUpper().IndexOf("/") > -1) && mTempAddress.Trim().Length < 8)
            {
                string pattern = @"(-)|(/)";
                string[] no = Regex.Split(mTempAddress, pattern);

                Regex regex = new Regex("[0-9]");

                for (int i = 0; i < no.Length; i++)
                {
                    if (regex.IsMatch(no[i]))
                    {
                        CheckType = (int)Enums.ParsingAdress.Kapı;
                    }
                }
            }
            else if (Regex.Replace(mTempAddress.ToUpper(), "[^0-9]", "") != "")
            {
                string kapiDigit = "";
                kapiDigit = Regex.Replace(mTempAddress.ToUpper(), "[^0-9]", "");

                if (counter != 0)
                {
                    if ((" " + tokens[counter - 1] + " ").ToUpper().IndexOf(" N ") > -1)
                    {
                        CheckType = (int)Enums.ParsingAdress.Kapı;
                    }
                }
            }
            return CheckType;
        }
    }
}
