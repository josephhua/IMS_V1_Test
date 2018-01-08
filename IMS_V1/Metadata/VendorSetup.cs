using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IMS_V1 
{
    [Table("VendorSetup")]
    [MetadataTypeAttribute(typeof(VendorSetup.VendorSetupMetadata))]
    public partial class VendorSetup
    {
        public VendorSetupAddr VendorAddr { get; set; }
        public VendorPhoneFax PhoneFax { get; set; }

        public VendorSetupAddr DiffAddr { get; set; }

        public VendorPhoneFax DiffPhoneFax { get; set; }

        public VendorEdiContact ITContact { get; set; }
        public VendorEdiContact CSContact { get; set; } //CS=Customer Service contact for EDI
        public VendorEdiContact ACContact { get; set; }  //AC=Accounting Contact for EDI

        internal sealed class VendorSetupMetadata
        {
            [DisplayAttribute(Name = "Contact Name")]
            public string ContactName { get; set; }

            [DisplayAttribute(Name = "Different Payee Name.Address")]
            public Nullable<bool> DiffPayee { get; set; }

            [DisplayAttribute(Name = "Min Value")]
            public string MinValue { get; set; }

            [DisplayAttribute(Name = "Min Units")]
            public string MinUnits { get; set; }

            [DisplayAttribute(Name = "Min Weight")]
            public string MinWeight { get; set; }

            [DisplayAttribute(Name = "Min Size")]
            public string MinSize { get; set; }

            [DisplayAttribute(Name = "CO-OP % (Flyers, Ads, Promotions, etx.)")]
            public string CO_OPPercent { get; set; }

            [DisplayAttribute(Name = "Vendor Lead Time")]
            public string VendorLeadTime { get; set; }

            [DisplayAttribute(Name = "Payment Terms DSC%")]
            [RegularExpression(@"^[0-9]{1,2}$", ErrorMessage = "Please enter a number less than 100.")]
            public Nullable<int> PTDscPercent { get; set; }

            [DisplayAttribute(Name = "DSC Days")]
            [RegularExpression(@"^[0-9]{1,3}$", ErrorMessage = "Please enter a number less than 1000.")]
            public Nullable<int> PTDscDays { get; set; }

            [DisplayAttribute(Name = "Pay Days")]
            [RegularExpression(@"^[0-9]{1,3}$", ErrorMessage = "Please enter a number less than 1000.")]
            public Nullable<int> PTPayDays { get; set; }

            [DisplayAttribute(Name = "Are Financial Statements or Applications required from USC?")]
            public Nullable<bool> FSAUsc { get; set; }

            [DisplayAttribute(Name = "Have you provided a certificate of Insurance to USC?")]
            public Nullable<bool> CIToUsc { get; set; }

            [DisplayAttribute(Name = "Have you provided a copy of your W9 to USC?")]
            public Nullable<bool> W9ToUsc { get; set; }

            [DisplayAttribute(Name = "Are any of your products considered hazardous?")]
            public Nullable<bool> Hazardous { get; set; }

            [DisplayAttribute(Name = "If you have hazardous products, have you provided a MSDS sheet for each product?")]
            public Nullable<bool> MSDS { get; set; }

            [DisplayAttribute(Name = "Accounting Contact Name")]
            public string AcctContactName { get; set; }

            [DisplayAttribute(Name = "Comments")]
            public string AcctComments { get; set; }

            [DisplayAttribute(Name = "Have you provided a copy of your FFL to USC?")]
            public Nullable<bool> FFLToUsc { get; set; }

            [DisplayAttribute(Name = "Freight Terms")]
            public string FreightTerms { get; set; }

        }
    }
}