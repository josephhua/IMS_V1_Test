using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;

namespace IMS_V1 
{
    [Table("Item")]
    [MetadataTypeAttribute(typeof(Item.ItemMetadata))]
    public partial class Item
    {
        /// Gets or sets the WareHouses.
        /// </summary>
        public List<int> WareHousesList { get; set; }
        public List<int> GetWareHouseList { get; set; }
        public List<ReplacementItem> ReplacementItems { get; set; }
        public int ImportedItemId { get; set; }
        public string ImageName { get; set; }
        internal sealed class ItemMetadata
        {
            public int Item_id { get; set; }
            [DisplayAttribute(Name = "Item Number")]
            [Editable(false)]
            public string Itm_Num { get; set; }
            //[Required(ErrorMessage="Please select a Vendor.]
            [Display(Name = "Vendor")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> ManufacturerLogo_Id { get; set; }
            //[StringLength(1000)]
            //public string Item_Description { get; set; }
            private string itemDescription = string.Empty;
            [StringLength(1000)]
            public string Item_Description
            {
                get
                {
                    return itemDescription;
                }
                set
                {               
                    itemDescription = value;
                }
            }
            [StringLength(31)]
            public string APlusDescription1 { get; set; }
            [StringLength(31)]
            public string APlusDescription2 { get; set; }
            //[Required(ErrorMessage="Please enter a MFG. Number.")]
            [RegularExpression("^[0-9a-zA-Z-/.+() ]{2,27}?$", ErrorMessage = "Please enter a MFG Number.")]
            public string MFG_Number { get; set; }
            //[Required(ErrorMessage="Please select a unit of measure.")]
            [Display(Name = "Unit/Measure")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> UM_Id { get; set; }
            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount. MSRP")]
            //[Required(ErrorMessage = "Please enter a MSRP.")]
            public Nullable<decimal> MSRP { get; set; }
            //[Required(ErrorMessage = "Please enter a Level1 Price.")]
            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount. 1")]
            public Nullable<decimal> Level1 { get; set; }
            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount. 2")]
            //[Required(ErrorMessage = "Please enter a Level2 Price.")]
            public Nullable<decimal> Level2 { get; set; }
            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount. 3")]
            //[Required(ErrorMessage = "Please enter a Level3 Price.")]
            public Nullable<decimal> Level3 { get; set; }
            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount.   4")]
            //[Required(ErrorMessage = "Please enter a JSCLevel5 Price.")]
            public Nullable<decimal> JSCLevel5 { get; set; }
            public string Qty_Break { get; set; }
            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount. 5")]
            public Nullable<decimal> Qty_BreakPrice { get; set; }

            //[Required(ErrorMessage = "Please select a Category.")]
            [Display(Name = "Category")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> CategoryClass_Id { get; set; }

            //[Required(ErrorMessage = "Please select a SubClass.")]
            [Display(Name = "SubClass")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> SubClassCode_Id { get; set; }

            //[Required(ErrorMessage = "Please select a Fineline Class.")]
            [Display(Name = "FineLine")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> FineLineCode_Id { get; set; }

            //[Required(ErrorMessage = "Please select a Web Code.")]
            [Display(Name = "Web Code")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> CatWebCode_Id { get; set; }

            //[Required(ErrorMessage = "Please select a Freight.")]
            [Display(Name = "Freight Class")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> Freight_Id { get; set; }

            [Display(Name = "Plan YN")]
            [RegularExpression("^[YN]?$", ErrorMessage = "Please select Plan YN.")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public string Plan_YN { get; set; }

            //[Required(ErrorMessage = "Please select an ABC.")]
            [Display(Name = "ABC")]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public Nullable<int> ABC_Id { get; set; }

            public string DS { get; set; }

            //[Required(ErrorMessage = "Please enter a STD number.")]
            [RegularExpression("^[0-9]{1,5}?$", ErrorMessage = "Please enter a numeric value less than 99999.")]
            public Nullable<int> STD { get; set; }
            //[RegularExpression("^[0-9]{2}?$")]

            [RegularExpression("^[0-9]{1,5}?$", ErrorMessage = "Please enter a numeric value less than 99999.")]
            //[Required(ErrorMessage = "Please enter a MIN number.")]
            public string MIN { get; set; }

            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount. VI")]
            //[Required(ErrorMessage = "Please enter a VI Cost.")]
            public Nullable<decimal> VICost { get; set; }

            [Display(Name = "Hazardous")]
            public string Haz { get; set; }

            //[Required(ErrorMessage = "Please enter a UPC Number.")]
            [RegularExpression(@"^[0-9]{12,16}$", ErrorMessage = "Please enter a numeric value with 12 to 16 digits.")]
            [Display(Name = "Selling UPC")]
            public string UPC { get; set; }

            [RegularExpression(@"^[0-9]{12,16}$", ErrorMessage = "Please enter a numeric value with 12 to 16 digits.")]
            [Display(Name = "Purchasing UPC")]
            public string EDIUPC { get; set; }

            public string Buyer { get; set; }
            public string Exclusive { get; set; }
            public string Allocated { get; set; }
            public string DropShip { get; set; }
            public string SpecialOrder { get; set; }
            public string PreventFromWeb { get; set; }
            public string CreatedBy { get; set; }
            public Nullable<System.DateTime> CreatedDate { get; set; }

            public string Approved { get; set; }
            public string ApprovedBy { get; set; }
            public Nullable<System.DateTime> ApprovedDate { get; set; }
            public string ReadyForApproval { get; set; }
            public string FastTrack { get; set; }
            public string FastTrackBy { get; set; }
            public Nullable<System.DateTime> FastTrackDate { get; set; }

            public Nullable<int> FFLType_Id { get; set; }

            [StringLength(100)]
            public string FFLCaliber { get; set; }

            [StringLength(100)]
            public string FFLModel { get; set; }

            [StringLength(150)]
            public string FFLMFGName { get; set; }

            [StringLength(150)]
            public string FFLMFGImportName { get; set; }

            [StringLength(50)]
            public string FFLGauge { get; set; }

            public string FFLLock { get; set; }
            public Nullable<System.DateTime> FFLLockDate { get; set; }

            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount map.")]
            [Display(Name = "Retail MAP")]
            public Nullable<decimal> MinAdvertisePrice { get; set; }

            [Display(Name = "Wholesale MTP")]
            [RegularExpression("^[YN]?$", ErrorMessage = "Please select WholeSale MTP.")]
            //[Required(ErrorMessage="Please enter Wholesale MTP.")]
            public string WholeSaleMTP { get; set; }
            public string Printed { get; set; }
            public Nullable<int> AssignedBuyer_Id { get; set; }
            [Display(Name = "Min. Advertise Price Flag")]
            [RegularExpression("^[YN]?$", ErrorMessage = "Please select Min. Advertise Price.")]
            //[Required(ErrorMessage="Please enter Wholesale MTP.")]
            public string MinAdvertisePriceFlag { get; set; }


            [RegularExpression("^([0-9][,]{0,1})?([0-9]{1,4})?(.[0-9]{2})?$", ErrorMessage = "Please enter a valid dollar amount. 2")]
            public Nullable<decimal> Level4 { get; set; }

            [StringLength(1000)]
            [Display(Name = "Storefront Item Description")]
            public string SF_Item_Description { get; set; }

            [Display(Name = "Company")]
            public string Company99 { get; set; }
           
        }

    }
}