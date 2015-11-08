using GeocoderAPI.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.DAL
{
    public static class ModelConverter
    {
        public static List<VUnitSearch> DataSetToVUnitSearch(DataSet ds)
        {
            List<VUnitSearch> result = new List<VUnitSearch>();

            if (ds == null)
                return result;

            if (ds.Tables[0] == null)
                return result;

            DataTable dt = ds.Tables[0];

            foreach (DataRow row in dt.Rows)
            {
                VUnitSearch item = new VUnitSearch();

                item.IlceId = row["ILCE_ID"] != DBNull.Value ? Convert.ToInt64(row["ILCE_ID"]) : -1;
                item.IlId = row["IL_ID"] != DBNull.Value ? Convert.ToInt64(row["IL_ID"]) : -1;
                item.MahalleId = row["MAHALLE_ID"] != DBNull.Value ? Convert.ToInt64(row["MAHALLE_ID"]) : -1;
                item.Name = row["NAME"].ToString();
                item.PoiId = row["POI_ID"] != DBNull.Value ? Convert.ToInt64(row["POI_ID"]) : -1;
                item.Sonuc = row["SONUC"].ToString();
                item.XCoor = row["XCOOR"].ToString();
                item.YCoor = row["YCOOR"].ToString();
                item.YolId = row["YOL_ID"] != DBNull.Value ? Convert.ToInt64(row["YOL_ID"]) : -1;

                result.Add(item);
            }

            return result;
        }
    }
}
