//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IMS_V1
{
    using System;
    using System.Collections.Generic;
    
    public partial class Country
    {
        public Country()
        {
            this.StateProvinces = new HashSet<StateProvince>();
        }
    
        public int id { get; set; }
        public string countryName { get; set; }
        public string countryAbbrev { get; set; }
        public Nullable<int> displayOrder { get; set; }
        public System.DateTime ActionDate { get; set; }
    
        public virtual ICollection<StateProvince> StateProvinces { get; set; }
    }
}
