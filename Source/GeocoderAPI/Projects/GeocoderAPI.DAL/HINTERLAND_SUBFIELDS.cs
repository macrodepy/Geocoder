//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GeocoderAPI.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class HINTERLAND_SUBFIELDS
    {
        public System.Guid HINTERLANDID { get; set; }
        public string MOBILE { get; set; }
        public string MOBILE_DAYS { get; set; }
        public string AA { get; set; }
        public string AT { get; set; }
        public Nullable<System.DateTime> AUDIT_CREATE_DATE { get; set; }
        public Nullable<int> AUDITCREATEDBY { get; set; }
        public Nullable<short> AUDITCREATEUNITID { get; set; }
        public Nullable<System.DateTime> AUDIT_MODIFY_DATE { get; set; }
        public Nullable<int> AUDITMODIFIEDBY { get; set; }
        public Nullable<short> AUDITMODIFYUNITID { get; set; }
        public string AUDIT_DELETED { get; set; }
        public string APPLICATION_VERSION { get; set; }
        public Nullable<short> APPLICATIONID { get; set; }
    }
}