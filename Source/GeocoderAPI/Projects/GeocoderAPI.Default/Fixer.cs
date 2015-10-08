using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.Default
{
    public static class Fixer
    {
        static CultureInfo cultureInfo = new CultureInfo("tr-TR");

        public static string Prepare(string address)
        {
            address = address.Trim().ToUpper(cultureInfo);

            while (address.IndexOf("  ") != -1)
            {
                address = address.Replace("  ", " ");
                address = address.Replace("  ", " ");
            }

            address = " " + address + " ";
            address = address.Replace(" BİNA NO ", " NO:");
            address = address.Replace(" BİNA NO:", " NO:");
            address = address.Replace(" BİNA NO.", " NO:");
            address = address.Replace(" DAİRE NO ", " D:");
            address = address.Replace(" DAİRE NO:", " D:");
            address = address.Replace(" DAİRE NO.", " D:");
            address = address.Replace(" DAİRE.NO.", " D:");
            address = address.Replace(" DAİRE.NO:", " D:");
            address = address.Replace(" D:NO ", " D:");
            address = address.Replace(" D.NO ", " D:");
            address = address.Replace(" D:NO:", " D:");
            address = address.Replace(" D:NO.", " D:");
            address = address.Replace(" D.NO.", " D:");
            address = address.Replace(" NO.:", " NO:");
            address = address.Replace(" NO.;", " NO:");
            address = address.Replace(" NO;", " NO:");
            address = address.Replace(" NO.,", " NO:");
            address = address.Replace(" NO,", " NO:");
            address = address.Replace(" NO.", " NO:");
            address = address.Replace(" NOÇ.", " NO:");
            address = address.Replace(" NO.Ç", " NO:");

            address = address.Replace(" NO :", " NO:");
            address = address.Replace(" N0 :", " NO:");
            address = address.Replace(" NO : ", " NO:");
            address = address.Replace(" N0 : ", " NO:");
            address = address.Replace(" D :", " D:");
            address = address.Replace(" D : ", " D:");
            address = address.Replace(" K :", " K:");
            address = address.Replace(" K : ", " K:");
            address = address.Replace(" KAT :", " K:");
            address = address.Replace(" KAT : ", " K:");
            address = address.Replace(" DAİRE :", " D:");
            address = address.Replace(" DAİRE : ", " D:");
            address = address.Replace(" N :", " NO:");
            address = address.Replace(" N : ", " NO:");

            address = address.Replace(" NO/", " NO:");
            address = address.Replace(".NO/", " NO:");
            address = address.Replace(",NO/", " NO:");

            address = address.Replace(" N.1", " NO:1 ");
            address = address.Replace(" N.2", " NO:2 ");
            address = address.Replace(" N.3", " NO:3 ");
            address = address.Replace(" N.4", " NO:4 ");
            address = address.Replace(" N.5", " NO:5 ");
            address = address.Replace(" N.6", " NO:6 ");
            address = address.Replace(" N.7", " NO:7 ");
            address = address.Replace(" N.8", " NO:8 ");
            address = address.Replace(" N.9", " NO:9 ");

            address = address.Replace(" D;", " D:");
            address = address.Replace(" D,", " D:");

            address = address.Replace(" D.1", " D:1 ");
            address = address.Replace(" D.2", " D:2 ");
            address = address.Replace(" D.3", " D:3 ");
            address = address.Replace(" D.4", " D:4 ");
            address = address.Replace(" D.5", " D:5 ");
            address = address.Replace(" D.6", " D:6 ");
            address = address.Replace(" D.7", " D:7 ");
            address = address.Replace(" D.8", " D:8 ");
            address = address.Replace(" D.9", " D:9 ");

            address = address.Replace(" DAİRE;", " D:");
            address = address.Replace(" DAİRE,", " D:");

            address = address.Replace(" DAİRE BAŞKANLIĞI ", " DAİRE-BAŞKANLIĞI ");

            address = address.Replace(" DAİRE ", " D:");
            address = address.Replace(" DAİRE:", " D:");
            address = address.Replace(" DAİRE.", " D:");

            address = address.Replace(" KAT;", " K:");
            address = address.Replace(" KAT,", " K:");
            address = address.Replace(" KAT.;", " K:");
            address = address.Replace(" KAT.,", " K:");

            address = address.Replace(" KAT.:", " K:");
            address = address.Replace(" KAT.", " K:");

            address = address.Replace(" K;", " K:");
            address = address.Replace(" K,", " K:");

            address = address.Replace(" K.1", " K:1 ");
            address = address.Replace(" K.2", " K:2 ");
            address = address.Replace(" K.3", " K:3 ");
            address = address.Replace(" K.4", " K:4 ");
            address = address.Replace(" K.5", " K:5 ");
            address = address.Replace(" K.6", " K:6 ");
            address = address.Replace(" K.7", " K:7 ");
            address = address.Replace(" K.8", " K:8 ");
            address = address.Replace(" K.9", " K:9 ");

            address = address.Replace(" NUMARA.", " NO:");
            address = address.Replace(".NUMARA.", " NO:");
            address = address.Replace(".NUMARA ", " NO:");

            address = address.Replace(" NUMARA;", " NO:");
            address = address.Replace(" NUMARA,", " NO:");
            address = address.Replace(".NUMARA,", " NO:");

            address = address.Replace(".NO.:", " NO:");

            address = address.Replace(".NO.;", " NO:");
            address = address.Replace(".NO.,", " NO:");

            address = address.Replace(".NOÇ.", " NO:");
            address = address.Replace(".NO.Ç", " NO:");

            address = address.Replace(" NO ", " NO:");
            address = address.Replace(".NO1 ", " NO:1 ");
            address = address.Replace(".NO2 ", " NO:2 ");
            address = address.Replace(".NO3 ", " NO:3 ");
            address = address.Replace(".NO4 ", " NO:4 ");
            address = address.Replace(".NO5 ", " NO:5 ");
            address = address.Replace(".NO6 ", " NO:6 ");
            address = address.Replace(".NO7 ", " NO:7 ");
            address = address.Replace(".NO8 ", " NO:8 ");
            address = address.Replace(".NO9 ", " NO:9 ");

            address = address.Replace(" NO1", " NO:1");
            address = address.Replace(" NO2", " NO:3");
            address = address.Replace(" NO3", " NO:3");
            address = address.Replace(" NO4", " NO:4");
            address = address.Replace(" NO5", " NO:5");
            address = address.Replace(" NO6", " NO:6");
            address = address.Replace(" NO7", " NO:7");
            address = address.Replace(" NO8", " NO:8");
            address = address.Replace(" NO9", " NO:9");

            address = address.Replace(".NO.:", " NO:");
            address = address.Replace(".NO.", " NO:");
            address = address.Replace(".NOÇ.", " NO:");

            address = address.Replace(".NO.;", " NO:");
            address = address.Replace(".NO,", " NO:");
            address = address.Replace(".NOÇ,", " NO:");

            address = address.Replace(".NO.Ç", " NO:");
            address = address.Replace(".N.1", " NO:1 ");
            address = address.Replace(".N.2", " NO:2 ");
            address = address.Replace(".N.3", " NO:3 ");
            address = address.Replace(".N.4", " NO:4 ");
            address = address.Replace(".N.5", " NO:5 ");
            address = address.Replace(".N.6", " NO:6 ");
            address = address.Replace(".N.7", " NO:7 ");
            address = address.Replace(".N.8", " NO:8 ");
            address = address.Replace(".N.9", " NO:9 ");
            address = address.Replace(".D.1", " D:1 ");
            address = address.Replace(".D.2", " D:2 ");
            address = address.Replace(".D.3", " D:3 ");
            address = address.Replace(".D.4", " D:4 ");
            address = address.Replace(".D.5", " D:5 ");
            address = address.Replace(".D.6", " D:6 ");
            address = address.Replace(".D.7", " D:7 ");
            address = address.Replace(".D.8", " D:8 ");
            address = address.Replace(".D.9", " D:9 ");
            address = address.Replace(".DAİRE:", " D:");
            address = address.Replace(".DAİRE ", " D:");
            address = address.Replace(".DAİRE.", " D:");
            address = address.Replace(".KAT.:", " K:");
            address = address.Replace(".KAT.", " K:");
            address = address.Replace(".K.1", " K:1 ");
            address = address.Replace(".K.2", " K:2 ");
            address = address.Replace(".K.3", " K:3 ");
            address = address.Replace(".K.4", " K:4 ");
            address = address.Replace(".K.5", " K:5 ");
            address = address.Replace(".K.6", " K:6 ");
            address = address.Replace(".K.7", " K:7 ");
            address = address.Replace(".K.8", " K:8 ");
            address = address.Replace(".K.9", " K:9 ");

            address = address.Replace(" A.Ş. ", " AŞ ");
            address = address.Replace("A.Ş. ", " AŞ ");
            address = address.Replace("AŞ. ", " AŞ ");
            address = address.Replace(" AŞ. ", " AŞ ");

            address = address.Replace(" NO: ", " NO:");
            address = address.Replace(" D: ", " D:");
            address = address.Replace(" K: ", " K:");

            address = address.Replace(".", " ");
            address = address.Replace(",", " ");

            while (address.IndexOf("  ") != -1)
            {
                address = address.Replace("  ", " ");
                address = address.Replace("  ", " ");
            }

            while ((address.IndexOf("\n") < address.Length) && (address.IndexOf("\n") != -1))
            {
                address = address.Replace("\n", " ");
            }

            while (address.IndexOf("  ") != -1)
            {
                address = address.Replace("  ", " ");
                address = address.Replace("  ", " ");
            }

            address = address.Replace(" MAHALLESI ", " MAHALLESİ ");
            address = address.Replace(" MAHALLE ", " MAHALLESİ ");
            address = address.Replace(" MAHAL ", " MAHALLESİ ");
            address = address.Replace(" MAH ", " MAHALLESİ ");
            address = address.Replace(" MH ", " MAHALLESİ ");

            address = address.Replace(" CADDESI ", " CADDESİ ");
            address = address.Replace(" CADDE ", " CADDESİ ");
            address = address.Replace(" CADD ", " CADDESİ ");
            address = address.Replace(" CAD ", " CADDESİ ");
            address = address.Replace(" CD ", " CADDESİ ");

            address = address.Replace(" SOKAĞI ", " SOKAK ");
            address = address.Replace(" SOKAKI ", " SOKAK ");
            address = address.Replace(" SOKAGI ", " SOKAK ");
            address = address.Replace(" SOKAĞ ", " SOKAK ");
            address = address.Replace(" SOKAH ", " SOKAK ");
            address = address.Replace(" SKAK ", " SOKAK ");
            address = address.Replace(" SOK ", " SOKAK ");
            address = address.Replace(" SK ", " SOKAK ");
            address = address.Replace(" SOKAKA ", " SOKAK ");

            address = address.Replace(" KOYÜ ", " KÖYÜ ");

            address = address.Replace(" BULVAR ", " BULVARI ");
            address = address.Replace(" BULVA ", " BULVARI ");
            address = address.Replace(" BULV ", " BULVARI ");
            address = address.Replace(" BUL ", " BULVARI ");
            address = address.Replace(" BLV ", " BULVARI ");
            address = address.Replace(" BLVR ", " BULVARI ");
            address = address.Replace(" BLVRI ", " BULVARI ");
            address = address.Replace(" BULVR ", " BULVARI ");
            address = address.Replace(" BULVRI ", " BULVARI ");

            address = address.Replace(" APRT ", " APT. ");
            address = address.Replace(" APRTMN ", " APT. ");
            address = address.Replace(" APART ", " APT. ");
            address = address.Replace(" APRTMANI ", " APT. ");
            address = address.Replace(" APARTMANI ", " APT. ");
            address = address.Replace(" APARTMAN ", " APT. ");
            address = address.Replace(" APARTMN ", " APT. ");
            address = address.Replace(" APT ", " APT. ");
            address = address.Replace(" AP ", " APT. ");

            address = address.Replace(" A-BLOK ", " A BLOK ");
            address = address.Replace(" B-BLOK ", " B BLOK ");
            address = address.Replace(" C-BLOK ", " C BLOK ");
            address = address.Replace(" D-BLOK ", " D BLOK ");
            address = address.Replace(" E-BLOK ", " E BLOK ");
            address = address.Replace(" F-BLOK ", " F BLOK ");
            address = address.Replace(" G-BLOK ", " G BLOK ");
            address = address.Replace(" H-BLOK ", " H BLOK ");
            address = address.Replace(" I-BLOK ", " I BLOK ");
            address = address.Replace(" J-BLOK ", " J BLOK ");
            address = address.Replace(" K-BLOK ", " K BLOK ");
            address = address.Replace(" L-BLOK ", " L BLOK ");
            address = address.Replace(" M-BLOK ", " M BLOK ");
            address = address.Replace(" N-BLOK ", " N BLOK ");
            address = address.Replace(" O-BLOK ", " O BLOK ");
            address = address.Replace(" P-BLOK ", " P BLOK ");
            address = address.Replace(" R-BLOK ", " R BLOK ");
            address = address.Replace(" S-BLOK ", " S BLOK ");
            address = address.Replace(" T-BLOK ", " T BLOK ");
            address = address.Replace(" U-BLOK ", " U BLOK ");
            address = address.Replace(" V-BLOK ", " V BLOK ");
            address = address.Replace(" Y-BLOK ", " Y BLOK ");
            address = address.Replace(" Z-BLOK ", " Z BLOK ");

            address = address.Replace(" İŞ MERKEZİ ", " İŞ.MRK. ");
            address = address.Replace(" İŞ MRKZ ", " İŞ.MRK. ");
            address = address.Replace(" İŞ MRK ", " İŞ.MRK. ");
            address = address.Replace(" İŞ MER ", " İŞ.MRK. ");
            address = address.Replace(" İŞ MR ", " İŞ.MRK. ");
            address = address.Replace(" İŞM ", " İŞ.MRK. ");
            address = address.Replace(" TİCARET MERKEZİ ", " TİC.MRK. ");
            address = address.Replace(" TİCARET MERK ", " TİC.MRK. ");
            address = address.Replace(" TİCARET MRKZ ", " TİC.MRK. ");
            address = address.Replace(" TİCARET MRK ", " TİC.MRK. ");
            address = address.Replace(" TİCARET MER ", " TİC.MRK. ");
            address = address.Replace(" TİCARET MR ", " TİC.MRK. ");
            address = address.Replace(" TİC MRKEZİ ", " TİC.MRK. ");
            address = address.Replace(" TİC MRKZ ", " TİC.MRK. ");
            address = address.Replace(" TİC MRK ", " TİC.MRK. ");
            address = address.Replace(" TİC MER ", " TİC.MRK. ");
            address = address.Replace(" TİC MR ", " TİC.MRK. ");
            address = address.Replace(" TİC AŞ ", " TİC.AŞ. ");
            address = address.Replace(" TİCARET AŞ ", " TİC.AŞ. ");
            address = address.Replace(" İŞ HANI ", " İŞHANI ");
            address = address.Replace(" İŞ HAN ", " İŞHANI ");
            address = address.Replace(" İŞ HN ", " İŞHANI ");
            address = address.Replace(" ORG SAN BÖLGESİ ", " ORG.SAN. ");
            address = address.Replace(" ORG SAN BÖLG ", " ORG.SAN. ");
            address = address.Replace(" ORG SAN BÖL ", " ORG.SAN. ");
            address = address.Replace(" ORGANİZE SAN ", " ORG.SAN. ");
            address = address.Replace(" ORGANİZE ", " ORG.SAN. ");
            address = address.Replace(" ORG SAN", " ORG.SAN.");
            address = address.Replace(" SAN ", " SANAYİİ ");
            address = address.Replace(" DÜKKANLARI ", " DÜK. ");
            address = address.Replace(" DÜKKANL ", " DÜK. ");
            address = address.Replace(" DÜKKAN ", " DÜK. ");
            address = address.Replace(" DÜKK ", " DÜK. ");
            address = address.Replace(" DÜK ", " DÜK. ");
            address = address.Replace(" ÇAR ", " ÇARŞISI ");
            address = address.Replace(" N:", " NO:");
            address = address.Replace(" NOÇ: ", " NO:");
            address = address.Replace(" NO: ", " NO:");
            address = address.Replace(" NO::", " NO:");
            address = address.Replace(" NUMARA 1", " NO:1");
            address = address.Replace(" NUMARA 2", " NO:2");
            address = address.Replace(" NUMARA 3", " NO:3");
            address = address.Replace(" NUMARA 4", " NO:4");
            address = address.Replace(" NUMARA 5", " NO:5");
            address = address.Replace(" NUMARA 6", " NO:6");
            address = address.Replace(" NUMARA 7", " NO:7");
            address = address.Replace(" NUMARA 8", " NO:8");
            address = address.Replace(" NUMARA 9", " NO:9");
            address = address.Replace(" D: ", " D:");
            address = address.Replace(" D::", " D:");
            address = address.Replace(" DAİRE 1", " D:1");
            address = address.Replace(" DAİRE 2", " D:2");
            address = address.Replace(" DAİRE 3", " D:3");
            address = address.Replace(" DAİRE 4", " D:4");
            address = address.Replace(" DAİRE 5", " D:5");
            address = address.Replace(" DAİRE 6", " D:6");
            address = address.Replace(" DAİRE 7", " D:7");
            address = address.Replace(" DAİRE 8", " D:8");
            address = address.Replace(" DAİRE 9", " D:9");
            address = address.Replace(" DAİRE:1", " D:1");
            address = address.Replace(" DAİRE:2", " D:2");
            address = address.Replace(" DAİRE:3", " D:3");
            address = address.Replace(" DAİRE:4", " D:4");
            address = address.Replace(" DAİRE:5", " D:5");
            address = address.Replace(" DAİRE:6", " D:6");
            address = address.Replace(" DAİRE:7", " D:7");
            address = address.Replace(" DAİRE:8", " D:8");
            address = address.Replace(" DAİRE:9", " D:9");

            address = address.Replace(" KAT:", " K:");
            address = address.Replace(" KAT 1", " K:1");
            address = address.Replace(" KAT 2", " K:2");
            address = address.Replace(" KAT 3", " K:3");
            address = address.Replace(" KAT 4", " K:4");
            address = address.Replace(" KAT 5", " K:5");
            address = address.Replace(" KAT 6", " K:6");
            address = address.Replace(" KAT 7", " K:7");
            address = address.Replace(" KAT 8", " K:8");
            address = address.Replace(" KAT 9", " K:9");
            address = address.Replace(" K: ", " K:");
            address = address.Replace(" K::", " K:");

            address = address.Replace(" DAİRE-BAŞKANLIĞI ", " DAİRE BAŞKANLIĞI ");

            address = address.Replace(" RENT A CAR ", " RENT.A.CAR ");
            address = address.Replace(" TATİL KÖYÜ ", " TATİL.KÖYÜ ");
            address = address.Replace(" KOOP ", " KOOPERATİF ");

            address = address.Replace(" KÖYÜ KAVŞAĞI ", " KÖYÜ.KAVŞAĞI ");
            address = address.Replace(" YOLU GİRİŞİ ", " YOLU.GİRİŞİ ");
            address = address.Replace(" YOLU ÜZERİ ", " YOLU.ÜZERİ ");
            address = address.TrimStart().TrimEnd();

            int Fbrackets = address.IndexOf("(");
            int Lbrackets = address.IndexOf(")");

            if (Fbrackets > 0 && Lbrackets > 0)
            {
                address.Remove(Fbrackets, Lbrackets - Fbrackets);
            }
            else if (Fbrackets > 0 && Lbrackets == -1)
            {
                address.Remove(Fbrackets);
            }

            return address;
        }
    }
}
