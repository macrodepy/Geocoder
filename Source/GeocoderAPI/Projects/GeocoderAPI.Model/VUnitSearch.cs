using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderAPI.Model
{
    public class VUnitSearch
    {
            public virtual string AuditDeleted { get; set; }
            public virtual long IlceId { get; set; }
            public virtual long IlId { get; set; }
            public virtual long? MahalleId { get; set; }
            public virtual Guid? MapDataId { get; set; }
            public virtual string Name { get; set; }
            public virtual long? PoiId { get; set; }
            public virtual string Sonuc { get; set; }
            public virtual string SonucIlceli { get; set; }
            public virtual string XCoor { get; set; }
            public virtual string YCoor { get; set; }
            public virtual long? YolId { get; set; }
    }
}
