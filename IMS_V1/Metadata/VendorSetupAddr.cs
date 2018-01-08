using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IMS_V1
{
    [Table("zManufacturersLogo")]
    [MetadataTypeAttribute(typeof(VendorSetupAddr.VendorSetupAddrMetadata))]
    public partial class VendorSetupAddr
    {
        internal sealed class VendorSetupAddrMetadata
        {
            [DisplayAttribute(Name = "PO Box(ADDR1)")]
            public string Addr1 { get; set; }

            [DisplayAttribute(Name = "Street 1(ADDR2)")]
            public string Addr2 { get; set; }

            [DisplayAttribute(Name = "City")]
            public string City { get; set; }

            [DisplayAttribute(Name = "State")]
            public string StateId { get; set; }

            [DisplayAttribute(Name = "Country")]
            public string CountryId { get; set; }

            [DisplayAttribute(Name = "Zip+4")]
            public string Zip4 { get; set; }

        }
    }
}