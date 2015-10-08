using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FuzzyString
{
    public static class FuzzyString
    {
        public static string GetPrimaryKeyString(string input)
        {
            var shingedString = ShingingString(input);
            var sortedHashCode = GetHashCode(shingedString);
            var enCodedKeyString = GetEncodedKeyString(sortedHashCode);
            return enCodedKeyString;
        }

        public static Int32[] GetHashCodeToCompare(string input)
        {
            var shingedString = ShingingString(input);
            var sortedHashCode = GetHashCode(shingedString);
            return sortedHashCode;
        }

        public static double GetSimilarIndex(string inputA, string inputB)
        {
            if (inputA.Trim() == inputB.Trim())
                return 100;
            if (inputA.Trim() == string.Empty)
                return 0;
            if (inputB.Trim() == string.Empty)
                return 0;
            return 100 * GetSimilarIndexByHash(GetHashCodeToCompare(inputA), GetHashCodeToCompare(inputB));
        }


        private static IEnumerable<string> ShingingString(string input)
        {
            string[] splitedStringArray = input.Split(new char[] { ' ', '\n', '.', ',', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ret = new string[splitedStringArray.Length - 1];
            for (int i = 0; i < splitedStringArray.Length - 1; i++)
            {
                ret[i] = splitedStringArray[i] + " " + splitedStringArray[i + 1];
            }
            return ret.Distinct();
        }

        private static Int32[] GetHashCode(IEnumerable<string> shingedString)
        {
            List<Int32> hashArray = new List<Int32>();
            foreach (var item in shingedString)
            {
                hashArray.Add(item.GetHashCode());
            }
            return hashArray.OrderBy(i=>i).ToArray();
            
        }

        private static string GetEncodedKeyString(Int32[] hashArray)
        {
            byte[] byteArray = new byte[hashArray.Length * 2];
            for (int i = 0; i < hashArray.Length; i++)
			{
			    byteArray[i * 2] = (byte)(hashArray[i] & 0xFF);
                byteArray[i * 2 + 1] = (byte)((hashArray[i] & 0xff00 ) >> 8);
			}
            string returnValue = System.Convert.ToBase64String(byteArray);
            return returnValue;
        }

        private static Int32[] ParsePrimiaryKeyString(string pkString)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(pkString);
            Debug.Assert(encodedDataAsBytes.Length % 2 == 0, "The byte length has to be odd number");
            Int32[] convertTo32Bit = new Int32[encodedDataAsBytes.Length / 2];
            for (int i = 0; i < encodedDataAsBytes.Length; i += 2)
            {
                convertTo32Bit[i/2] = (Int32)encodedDataAsBytes[i] + (Int32)encodedDataAsBytes[i + 1] << 8;
            }
            return convertTo32Bit;
        }

        private static double GetSimilarIndexByHash(Int32[] setA, Int32[] setB)
        {
            double similarIndex =  (double)(setA.Intersect(setB).Count()) / (double)(setA.Union(setB).Count());
            return similarIndex;
        }
    }
}
