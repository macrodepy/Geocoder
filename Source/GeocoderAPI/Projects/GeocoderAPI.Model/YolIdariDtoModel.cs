using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.Model
{
    public class YolIdariDtoModel
    {
        public long IlceId { get; set; }
        public long MahalleId { get; set; }
        public long YolId { get; set; }
        public string IlAdı { get; set; }
        public string IlceAdı { get; set; }
        public string MahalleAdı { get; set; }
        public long IlId { get; set; }
        public string XCoor { get; set; }
        public string YCoor { get; set; }
        public int YolSınıfı { get; set; }
        public string YolAdı { get; set; }
    }
}
