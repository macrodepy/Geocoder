using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.Model
{
    public class TestResultModel
    {
        public string Address { get; set; }
        public string ActualXCoor { get; set; }
        public string ActualYCoor { get; set; }
        public string YandexXCoor { get; set; }
        public string YandexYCoor { get; set; }
        public string YandexTime { get; set; }
        public string GoogleXCoor { get; set; }
        public string GoogleYCoor { get; set; }
        public string GoogleTime { get; set; }
        public string MyXCoor { get; set; }
        public string MyYCoor { get; set; }
        public string MyTime { get; set; }

    }
}
