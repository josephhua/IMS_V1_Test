using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IMS_V1
{
    [Table("VendorEdiContact")]
    [MetadataTypeAttribute(typeof(VendorEdiContact.VendorEdiContactMetadata))]
    public partial class VendorEdiContact
    {
        internal sealed class VendorEdiContactMetadata
        {
            [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter valid email address.")]
            public string Email { get; set; }
        }
    }
}