using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IMS_V1
{
    [Table("zManufacturersLogo")]
    [MetadataTypeAttribute(typeof(zManufacturersLogo.zManufacturersLogoMetadata))]
    public partial class zManufacturersLogo
    {
        public VendorSetup vSetup { get; set; }

        [Required(ErrorMessage = "Please select a state.")]
        public int VendorAddrStateId { get; set; }
        internal sealed class zManufacturersLogoMetadata
        {
            [DisplayAttribute(Name = "Vendor Number")]
            public string VendorNumber { get; set; }

            [Required(ErrorMessage = "Please enter APlus Vendor Name.")]
            [DisplayAttribute(Name = "APlus Vendor Name")]
            public string APlusVendorName { get; set; }

            [DisplayAttribute(Name = "Logo Name")]
            public string LogoName { get; set; }

            [Required(ErrorMessage = "Please enter Web Vendor Name.")]
            [DisplayAttribute(Name = "Web Vendor Name")]
            public string WebVendorName { get; set; }

            [Required(ErrorMessage = "Please enter Abbreviation.")]
            public string Abbrev { get; set; }

        }
    }
}