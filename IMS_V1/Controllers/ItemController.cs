using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace IMS_V1.Controllers
{
    public class ItemController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        public ActionResult Index(string currentFilter, string SearchString, int? page, string orderType)
        {
            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = currentFilter;
            }
            int userid = int.Parse(Session.Contents["UserId"].ToString());
            int usertypeid = int.Parse(Session.Contents["UserTypeId"].ToString());
            string userfullname = Session.Contents["LogedUserFullName"].ToString();
            //string CreateAPlusImport = Session.Contents["CreateAPlusImport"].ToString();
            //ViewBag.CreateAPlusImport = CreateAPlusImport;
            ViewBag.userid = userid;
            ViewBag.CurrentFilter = SearchString;
            var items = from r in
                            db.Items
                        select r;

            if (!String.IsNullOrEmpty(SearchString))
            {
                items = items.Where(r => r.Item_Description.ToUpper().Contains(SearchString.ToUpper()) || r.Itm_Num.ToUpper().Contains(SearchString.ToUpper()) || r.MFG_Number.ToUpper().Contains(SearchString.ToUpper()));
            }
            //            else
            //            {
            //                items = items.Where(r => r.ReadyForApproval == "N");
            //            }

            if (usertypeid != 1)
            {
                items = items.Where(r => (r.CreatedBy == userid) || (r.AssignedBuyer_Id == userid));
            }
            // Changes made 11/14/2015 by DPC to always default order of items to most recent at top.

            //string OT = orderType;
            string OT = "MR";
            //if (OT == "MR")
            //{
            items = items.OrderByDescending(r => r.Item_id);
            ViewBag.OrderType = OT;
            //}
            //else
            //{
            //    items = items.OrderBy(r => r.zManufacturersLogo.APlusVendorName).OrderBy(r => r.CategoryClass.CategoryName).OrderBy(r => r.Item_Description);// ascending;
            //}
            int pageSize = 30;
            int pageNumber = (page ?? 1);
            int RAPlusImportCheck = 0;
            //if(TempData["APlusImportMessage"] != null){
            //    ViewBag.APlusImportMessage = TempData["APlusImportMessage"];
            //}
            //var APlusImportRunning = db.Database.ExecuteSqlCommand("CheckForAPlusImportRunning");
            //if (APlusImportRunning <= 0)
            //{
            //    if (CreateAPlusImport == "M")
            //    {
            //        RAPlusImportCheck = db.CheckForMarineAPlusImportItems().Count();
            //    }
            //    else
            //    {
            //        if (CreateAPlusImport == "S")
            //        {
            //            RAPlusImportCheck = db.CheckForAPlusImportItems().Count();
            //        }
            //    }
            //    if (RAPlusImportCheck > 0)
            //    {
            //        ViewBag.ShowCreateAPlusImportFile = true;
            //    }ap
            //    else
            //    {
            //        ViewBag.ShowCreateAPlusImportFile = false;
            //    }
            //}
            //else {
            //    ViewBag.APlusImportMessage = "APlus import file creation currently running."; 
            //    ViewBag.ShowCreateAPlusImportFile = false;
            //}

            return View(items.ToPagedList(pageNumber, pageSize));
        }

        //[HttpPost]
        //public ActionResult CreateAPlusImportFile(string MarineShooting)
        //{
        //    var APlusImportRunning = db.Database.ExecuteSqlCommand("CheckForAPlusImportRunning");
        //    if (APlusImportRunning <= 0)
        //    {

        //        var Running = db.Database.ExecuteSqlCommand("TurnRunningFlagOn");
        //        string AplusFileLocation = ConfigurationManager.AppSettings["AplusFileLocation"];
        //        string KeyCode = "";
        //        if (MarineShooting == "M") //Marine
        //        {
        //            var RAPlusImportKeyCode = db.GetMarineAPlusImportKeyCode();
        //            foreach (var row in RAPlusImportKeyCode)
        //            {
        //                KeyCode = row.APlusImportKeyCode;
        //            }
        //        }
        //        else // "S" Shooting
        //        {
        //            var RAPlusImportKeyCode = db.GetAPlusImportKeyCode();
        //            foreach (var row in RAPlusImportKeyCode)
        //            {
        //                KeyCode = row.APlusImportKeyCode;
        //            }
        //        }
        //        if (KeyCode.ToString() != "")
        //        {
        //            ViewBag.APlusImportMessage = "APlus import file created successfully.";
        //            TempData["APlusImportMessage"] = "APlus import file created successfully.";
        //            var RunningOff = db.Database.ExecuteSqlCommand("TurnRunningFlagOff");
        //        }
        //        else {
        //            ViewBag.APlusImportMessage = "APlus import file creation failed.";
        //            TempData["APlusImportMessage"] = "APlus import file creation failed.";
        //            var RunningOffError = db.Database.ExecuteSqlCommand("TurnRunningFlagOff");
        //        }
        // string nd = Request.Form["txtItemDescription"];
        //try
        //{
        //    //var RITBAL = db.Database.ExecuteSqlCommand("APlusImport_ITBAL", new SqlParameter("@APlusImportKeyCode", RAPlusImportKeyCode));
        //    var RITBAL = db.APlusImport_ITBAL(KeyCode);
        //    string SystemDate = System.DateTime.Now.ToString();
        //    //                string ITBAL = "C:\\XMLFiles\\ITBAL"+SystemDate.Replace("/","").Replace(":","").Replace(" ","")+".txt";
        //    string ITBAL = AplusFileLocation + "ITBALD" + SystemDate.Replace("/", "").Replace(":", "").Replace(" ", "") + ".csv";
        //    FileInfo FIITBAL = new FileInfo(ITBAL);
        //    if (!FIITBAL.Exists)
        //    {
        //        using (StreamWriter writer = FIITBAL.CreateText())
        //        {
        //            //string Header = "itm_num|Warehouse_ID|FunctionCode|zm.VendorNumber|PrimaryLocation|SecondaryLocation|CycleCountCode|PhysicalInventoryCode|StandardCost|UserCost|AverageCost|LastCost|LastCostDate|MaxOn-HandQtyUM1|MaxOn-HandQtyUM2|MaxOn-HandQtyUM3|MinOn-HandQtyUM1|";
        //            //Header = Header + "MinOn-HandQtyUM2|MinOn-HandQtyUM3|Forecast|SpecialOrderCode|20CharacterUserArea|i.Level1|i.Level2|i.Level3|ListPrice4|i.JSCLevel5|abc.abc_code|PriceClass|i.Qty_Break|AllowCashDiscountCode|DefaultOrderingUM|TimePhasedItem|CommissionCostCode|";
        //            //Header = Header + "CommissionCost|ItemContractCode|BuyerReviewRequired|CountryofOrigin1|CountryofOrigin2|TaxableCode|Re-useCode|ItemTaxClass|30CharacterUserArea|PointofSaleLineType|PromptForPointOfSaleLineType|UpdateDemandPlanning|PutAwayQuantity|MinimumPutAwayQuantity|MaximumPickQuantity|CaseQuantityFlag|CaseDescription|CaseQuantity|OverrideWarehouseLocation|";
        //            //Header = Header + "MaximumPut-AwayQuantity|Put-AwayMessage|QuantityperPallet|PalletID|LocationClassCode|ItemLabelFlag|OverridePOSLocation|ForecastModel|ServiceLevel|ServiceLevelMaintenanceCode|SafetyStockQuantity|SafetyStockQuantityMaintenanceCode|LeadTime|LeadTime MaintenanceCode|OrderingLevel|OrderLevelMaintenanceCode|";
        //            //Header = Header + "OrderingFrequency|OrderFrequencyMaintenanceCode|AdditionalGrowthPercent|GrowthPercentMaintenanceCode|MinimumBalanceMaintenanceCode|MaximumBalanceMaintenanceCode|IMPOrderQuantity|OrderQuantityMaintenanceCode";
        //            //writer.WriteLine(Header);
        //            foreach (var row in RITBAL)
        //            {
        //                string Detail =  row.itm_num + "|" + row.Warehouse_ID + "|" + row.FunctionCode + "|" + row.VendorNumber + "|" + row.PrimaryLocation + "|" + row.SecondaryLocation + "|" + row.CycleCountCode + "|" + row.PhysicalInventoryCode + "|" + row.StandardCost + "|" + row.UserCost + "|" + row.AverageCost + "|" + row.LastCost + "|" + row.LastCostDate + "|" + row.MaxOnHandQtyUM1 + "|" + row.MaxOnHandQtyUM2 + "|" + row.MaxOnHandQtyUM3 + "|" + row.MinOnHandQtyUM1 + "|";
        //                Detail = Detail + row.MinOnHandQtyUM2 + "|" + row.MinOnHandQtyUM3 + "|" + row.Forecast + "|" + row.SpecialOrderCode + "|" + row.C20CharacterUserArea + "|" + row.Level1 + "|" + row.Level2 + "|" + row.Level3 + "|" + row.ListPrice4 + "|" + row.JSCLevel5 + "|" + row.abc_code + "|" + row.PriceClass + "|" + row.Qty_Break + "|" + row.AllowCashDiscountCode + "|" + row.DefaultOrderingUM + "|" + row.TimePhasedItem + "|" + row.CommissionCostCode + "|";
        //                Detail = Detail + row.CommissionCost + "|" + row.ItemContractCode + "|" + row.BuyerReviewRequired + "|" + row.CountryofOrigin1 + "|" + row.CountryofOrigin2 + "|" + row.TaxableCode + "|" + row.ReuseCode + "|" + row.ItemTaxClass + "|" + row.C30CharacterUserArea + "|" + row.PointofSaleLineType + "|" + row.PromptForPointOfSaleLineType + "|" + row.UpdateDemandPlanning + "|" + row.PutAwayQuantity + "|" + row.MinimumPutAwayQuantity + "|" + row.MaximumPickQuantity + "|" + row.CaseQuantityFlag + "|" + row.CaseDescription + "|" + row.CaseQuantity + "|" + row.OverrideWarehouseLocation + "|";
        //                Detail = Detail +  row.MaximumPutAwayQuantity + "|" + row.PutAwayMessage + "|" + row.QuantityperPallet + "|" + row.PalletID + "|" + row.LocationClassCode + "|" + row.ItemLabelFlag + "|" + row.OverridePOSLocation + "|" + row.ForecastModel + "|" + row.ServiceLevel + "|" + row.ServiceLevelMaintenanceCode + "|" + row.SafetyStockQuantity + "|" + row.SafetyStockQuantityMaintenanceCode + "|" + row.LeadTime + "|" + row.LeadTime_MaintenanceCode + "|" + row.OrderingLevel + "|" + row.OrderLevelMaintenanceCode + "|";
        //                Detail = Detail +  row.OrderingFrequency + "|" + row.OrderFrequencyMaintenanceCode + "|" + row.AdditionalGrowthPercent + "|" + row.GrowthPercentMaintenanceCode + "|" + row.MinimumBalanceMaintenanceCode + "|" + row.MaximumBalanceMaintenanceCode + "|" + row.IMPOrderQuantity + "|" + row.OrderQuantityMaintenanceCode ;
        //                writer.WriteLine(Detail.Replace('\\', '"'));
        //            // Original code below
        //            //string Header = "\\itm_num\\,\\Warehouse_ID\\,\\FunctionCode\\,\\zm.VendorNumber\\,\\PrimaryLocation\\,\\SecondaryLocation\\,\\CycleCountCode\\,\\PhysicalInventoryCode\\,\\StandardCost\\,\\UserCost\\,\\AverageCost\\,\\LastCost\\,\\LastCostDate\\,\\MaxOn-HandQtyUM1\\,\\MaxOn-HandQtyUM2\\,\\MaxOn-HandQtyUM3\\,\\MinOn-HandQtyUM1\\,";
        //            //Header = Header + "\\MinOn-HandQtyUM2\\,\\MinOn-HandQtyUM3\\,\\Forecast\\,\\SpecialOrderCode\\,\\20CharacterUserArea\\,\\i.Level1\\,\\i.Level2\\,\\i.Level3\\,\\ListPrice4\\,\\i.JSCLevel5\\,\\abc.abc_code\\,\\PriceClass\\,\\i.Qty_Break\\,\\AllowCashDiscountCode\\,\\DefaultOrderingUM\\,\\TimePhasedItem\\,\\CommissionCostCode\\,";
        //            //Header = Header + "\\CommissionCost\\,\\ItemContractCode\\,\\BuyerReviewRequired\\,\\CountryofOrigin1\\,\\CountryofOrigin2\\,\\TaxableCode\\,\\Re-useCode\\,\\ItemTaxClass\\,\\30CharacterUserArea\\,\\PointofSaleLineType\\,\\PromptForPointOfSaleLineType\\,\\UpdateDemandPlanning\\,\\PutAwayQuantity\\,\\MinimumPutAwayQuantity\\,\\MaximumPickQuantity\\,\\CaseQuantityFlag\\,\\CaseDescription\\,\\CaseQuantity\\,\\OverrideWarehouseLocation\\,";
        //            //Header = Header + "\\MaximumPut-AwayQuantity\\,\\Put-AwayMessage\\,\\QuantityperPallet\\,\\PalletID\\,\\LocationClassCode\\,\\ItemLabelFlag\\,\\OverridePOSLocation\\,\\ForecastModel\\,\\ServiceLevel\\,\\ServiceLevelMaintenanceCode\\,\\SafetyStockQuantity\\,\\SafetyStockQuantityMaintenanceCode\\,\\LeadTime\\,\\LeadTime MaintenanceCode\\,\\OrderingLevel\\,\\OrderLevelMaintenanceCode\\,";
        //            //Header = Header + "\\OrderingFrequency\\,\\OrderFrequencyMaintenanceCode\\,\\AdditionalGrowthPercent\\,\\GrowthPercentMaintenanceCode\\,\\MinimumBalanceMaintenanceCode\\,\\MaximumBalanceMaintenanceCode\\,\\IMPOrderQuantity\\,\\OrderQuantityMaintenanceCode\\";
        //            //writer.WriteLine(Header.Replace('\\', '"'));
        //            //foreach (var row in RITBAL)
        //            //{
        //            //    string Detail = "\\" + row.itm_num + "\\,\\" + row.Warehouse_ID + "\\,\\" + row.FunctionCode + "\\,\\" + row.VendorNumber + "\\,\\" + row.PrimaryLocation + "\\,\\" + row.SecondaryLocation + "\\,\\" + row.CycleCountCode + "\\,\\" + row.PhysicalInventoryCode + "\\,\\" + row.StandardCost + "\\,\\" + row.UserCost + "\\,\\" + row.AverageCost + "\\,\\" + row.LastCost + "\\,\\" + row.LastCostDate + "\\,\\" + row.MaxOnHandQtyUM1 + "\\,\\" + row.MaxOnHandQtyUM2 + "\\,\\" + row.MaxOnHandQtyUM3 + "\\,\\" + row.MinOnHandQtyUM1 + "\\,";
        //            //    Detail = Detail + "\\" + row.MinOnHandQtyUM2 + "\\,\\" + row.MinOnHandQtyUM3 + "\\,\\" + row.Forecast + "\\,\\" + row.SpecialOrderCode + "\\,\\" + row.C20CharacterUserArea + "\\,\\" + row.Level1 + "\\,\\" + row.Level2 + "\\,\\" + row.Level3 + "\\,\\" + row.ListPrice4 + "\\,\\" + row.JSCLevel5 + "\\,\\" + row.abc_code + "\\,\\" + row.PriceClass + "\\,\\" + row.Qty_Break + "\\,\\" + row.AllowCashDiscountCode + "\\,\\" + row.DefaultOrderingUM + "\\,\\" + row.TimePhasedItem + "\\,\\" + row.CommissionCostCode + "\\,";
        //            //    Detail = Detail + "\\" + row.CommissionCost + "\\,\\" + row.ItemContractCode + "\\,\\" + row.BuyerReviewRequired + "\\,\\" + row.CountryofOrigin1 + "\\,\\" + row.CountryofOrigin2 + "\\,\\" + row.TaxableCode + "\\,\\" + row.ReuseCode + "\\,\\" + row.ItemTaxClass + "\\,\\" + row.C30CharacterUserArea + "\\,\\" + row.PointofSaleLineType + "\\,\\" + row.PromptForPointOfSaleLineType + "\\,\\" + row.UpdateDemandPlanning + "\\,\\" + row.PutAwayQuantity + "\\,\\" + row.MinimumPutAwayQuantity + "\\,\\" + row.MaximumPickQuantity + "\\,\\" + row.CaseQuantityFlag + "\\,\\" + row.CaseDescription + "\\,\\" + row.CaseQuantity + "\\,\\" + row.OverrideWarehouseLocation + "\\,";
        //            //    Detail = Detail + "\\" + row.MaximumPutAwayQuantity + "\\,\\" + row.PutAwayMessage + "\\,\\" + row.QuantityperPallet + "\\,\\" + row.PalletID + "\\,\\" + row.LocationClassCode + "\\,\\" + row.ItemLabelFlag + "\\,\\" + row.OverridePOSLocation + "\\,\\" + row.ForecastModel + "\\,\\" + row.ServiceLevel + "\\,\\" + row.ServiceLevelMaintenanceCode + "\\,\\" + row.SafetyStockQuantity + "\\,\\" + row.SafetyStockQuantityMaintenanceCode + "\\,\\" + row.LeadTime + "\\,\\" + row.LeadTime_MaintenanceCode + "\\,\\" + row.OrderingLevel + "\\,\\" + row.OrderLevelMaintenanceCode + "\\,";
        //            //    Detail = Detail + "\\" + row.OrderingFrequency + "\\,\\" + row.OrderFrequencyMaintenanceCode + "\\,\\" + row.AdditionalGrowthPercent + "\\,\\" + row.GrowthPercentMaintenanceCode + "\\,\\" + row.MinimumBalanceMaintenanceCode + "\\,\\" + row.MaximumBalanceMaintenanceCode + "\\,\\" + row.IMPOrderQuantity + "\\,\\" + row.OrderQuantityMaintenanceCode + "\\";
        //            //    writer.WriteLine(Detail.Replace('\\', '"'));
        //            }
        //        }
        //    }
        //    var RITMST = db.APlusImport_ITMST(KeyCode);
        //    //                string ITMST = "C:\\XMLFiles\\ITMST" + SystemDate.Replace("/", "").Replace(":", "").Replace(" ", "") + ".txt";
        //    string ITMST = AplusFileLocation + "ITMSTD" + SystemDate.Replace("/", "").Replace(":", "").Replace(" ", "") + ".csv";
        //    FileInfo FIITMST = new FileInfo(ITMST);
        //    if (!FIITMST.Exists)
        //    {
        //        using (StreamWriter writer = FIITMST.CreateText())
        //        {
        //            //string Header = "itm_num|FunctionCode|APlusDescription1|APlusDescription2|UnitofMeasure1|UnitofMeasure2|UnitofMeasure3|UMConversion1|UMConversion2|DefaultUMCode|PricingUM|PricingUMConversion|UMSurcharge1|UMSurcharge2|UMSurcharge3|SurchargeCode1|SurchargeCode2|";
        //            //Header = Header + "SurchargeCode3|UnitWeightUM1|UnitWeightUM2|UnitWeightUM3|ItemClass|ItemSubclass|PriceClass|QuantityBreakClass|MfgItemNumber|ContainerCharge1|ContainerCharge2|ContainerCharge3|ListPrice1|ListPrice2|ListPrice3|ListPrice4|ListPrice5|TaxableCode|";
        //            //Header = Header + "ReuseCode|BackorderCode|AllowCashDiscountCode|MiscCode1|MiscCode2|MiscCode3|CompanyNumber|UpdateInventoryCode|CatchWeightCode|ItemGLCode|LocationClassCode|FederalExciseTaxAmount|VendorNumber|LocationSize1|LocationSize2|LocationSize3|WHManagementCode|";
        //            //Header = Header + "ExpirationDateRequiredFlag|ShowLotSerialonInvoice|15CharacterUserArea|ProductRestrictionCode|MSDSDate|ItemTaxClass|RebateClass|UserField1|UserField2|UserField3|UserField4|UserField5|UserField6|InquiryUM|ReportingUM|ItemEIDGroup|ProductID|ItemContractCode|BuyerItemClass|";
        //            //Header = Header + "BuyerItemSubclass|ItemCommitmentCode|HarmonizeTariffCode|CommodityCode|UniqueLots| PreventFromWeb|StandardUsage|30CharacterUserArea|TrackCountryofOrigin|DiscontinuedCode|UM1Length|UM1Width |UM1Height|UM2Length|UM2Width |UM2Height|UM3Length|UM3Width |UM3Height|";
        //            //Header = Header + "BoxType|BoxQuantityUM1|BoxQuantityUM2|BoxQuantityUM3|HazardousMaterialGrade|ProperShippingName|PrimaryHazardClassCode|DOTNumber|PackageGroup|HAZMATCasofIngredient1|HAZMATCasofIngredient2|HAZMATCasofIngredient3|HAZMATCasofIngredient4|HAZMATCasofIngredient5|";
        //            //Header = Header + "HAZMATCasofIngredient6|HAZMATCasbyWeight1|HAZMATCasbyWeight2|HAZMATCasbyWeight3|HAZMATCasbyWeight4|HAZMATCasbyWeight5|HAZMATCasbyWeight6|LimitedQuantity|HazardousMaterialMsgCd1|HazardousMaterialMsgCd2|HazardousMaterialMsgCd3|HazardousMaterialMsgCd4|DOTRegulated|";
        //            //Header = Header + "ProperShippingNameLine2|ProperShippingNameLine3|ProperShippingNameLine4|SubsidiaryHazardClassCode1|SubsidiaryHazardClassCode2|PackageType";
        //            //writer.WriteLine(Header);
        //            foreach (var row in RITMST)
        //            {
        //                string Detail =  row.itm_num + "|" + row.FunctionCode + "|" + row.APlusDescription1 + "|" + row.APlusDescription2 + "|" + row.UnitofMeasure1 + "|" + row.UnitofMeasure2 + "|" + row.UnitofMeasure3 + "|" + row.UMConversion1 + "|" + row.UMConversion2 + "|" + row.DefaultUMCode + "|" + row.PricingUM + "|" + row.PricingUMConversion + "|" + row.UMSurcharge1 + "|" + row.UMSurcharge2 + "|" + row.UMSurcharge3 + "|" + row.SurchargeCode1 + "|" + row.SurchargeCode2 + "|";
        //                Detail = Detail + row.SurchargeCode3 + "|" + row.UnitWeightUM1 + "|" + row.UnitWeightUM2 + "|" + row.UnitWeightUM3 + "|" + row.ItemClass + "|" + row.ItemSubclass + "|" + row.PriceClass + "|" + row.QuantityBreakClass + "|" + row.MfgItemNumber + "|" + row.ContainerCharge1 + "|" + row.ContainerCharge2 + "|" + row.ContainerCharge3 + "|" + row.ListPrice1 + "|" + row.ListPrice2 + "|" + row.ListPrice3 + "|" + row.ListPrice4 + "|" + row.ListPrice5 + "|" + row.TaxableCode + "|";
        //                Detail = Detail + row.ReuseCode + "|" + row.BackorderCode + "|" + row.AllowCashDiscountCode + "|" + row.MiscCode1 + "|" + row.MiscCode2 + "|" + row.MiscCode3 + "|" + row.CompanyNumber + "|" + row.UpdateInventoryCode + "|" + row.CatchWeightCode + "|" + row.ItemGLCode + "|" + row.LocationClassCode + "|" + row.FederalExciseTaxAmount + "|" + row.VendorNumber + "|" + row.LocationSize1 + "|" + row.LocationSize2 + "|" + row.LocationSize3 + "|" + row.WHManagementCode + "|,";
        //                Detail = Detail + row.ExpirationDateRequiredFlag + "|" + row.ShowLotSerialonInvoice + "|" + row.C15CharacterUserArea + "|" + row.ProductRestrictionCode + "|" + row.MSDSDate + "|" + row.ItemTaxClass + "|" + row.RebateClass + "|" + row.UserField1 + "|" + row.UserField2 + "|" + row.UserField3 + "|" + row.UserField4 + "|" + row.UserField5 + "|" + row.UserField6 + "|" + row.InquiryUM + "|" + row.ReportingUM + "|" + row.ItemEIDGroup + "|" + row.ProductID + "|" + row.ItemContractCode + "|" + row.BuyerItemClass + "|";
        //                Detail = Detail + row.BuyerItemSubclass + "|" + row.ItemCommitmentCode + "|" + row.HarmonizeTariffCode + "|" + row.CommodityCode + "|" + row.UniqueLots + "|" + row.PreventFromWeb + "|" + row.StandardUsage + "|" + row.C30CharacterUserArea + "|" + row.TrackCountryofOrigin + "|" + row.DiscontinuedCode + "|" + row.UM1Length + "|" + row.UM1Width + "|" + row.UM1Height + "|" + row.UM2Length + "|" + row.UM2Width + "|" + row.UM2Height + "|" + row.UM3Length + "|" + row.UM3Width + "|" + row.UM3Height + "|";
        //                Detail = Detail + row.BoxType + "|" + row.BoxQuantityUM1 + "|" + row.BoxQuantityUM2 + "|" + row.BoxQuantityUM3 + "|" + row.HazardousMaterialGrade + "|" + row.ProperShippingName + "|" + row.PrimaryHazardClassCode + "|" + row.DOTNumber + "|" + row.PackageGroup + "|" + row.HAZMATCasofIngredient1 + "|" + row.HAZMATCasofIngredient2 + "|" + row.HAZMATCasofIngredient3 + "|" + row.HAZMATCasofIngredient4 + "|" + row.HAZMATCasofIngredient5 + "|";
        //                Detail = Detail + row.HAZMATCasofIngredient6 + "|" + row.HAZMATCasbyWeight1 + "|" + row.HAZMATCasbyWeight2 + "|" + row.HAZMATCasbyWeight3 + "|" + row.HAZMATCasbyWeight4 + "|" + row.HAZMATCasbyWeight5 + "|" + row.HAZMATCasbyWeight6 + "|" + row.LimitedQuantity + "|" + row.HazardousMaterialMsgCd1 + "|" + row.HazardousMaterialMsgCd2 + "|" + row.HazardousMaterialMsgCd3 + "|" + row.HazardousMaterialMsgCd4 + "|" + row.DOTRegulated + "|";
        //                Detail = Detail + row.ProperShippingNameLine2 + "|" + row.ProperShippingNameLine3 + "|" + row.ProperShippingNameLine4 + "|" + row.SubsidiaryHazardClassCode1 + "|" + row.SubsidiaryHazardClassCode2 + "|" + row.PackageType ;
        //                writer.WriteLine(Detail);
        //            // Original Code Below
        //            //string Header = "\\itm_num\\,\\FunctionCode\\,\\APlusDescription1\\,\\APlusDescription2\\,\\UnitofMeasure1\\,\\UnitofMeasure2\\,\\UnitofMeasure3\\,\\UMConversion1\\,\\UMConversion2\\,\\DefaultUMCode\\,\\PricingUM\\,\\PricingUMConversion\\,\\UMSurcharge1\\,\\UMSurcharge2\\,\\UMSurcharge3\\,\\SurchargeCode1\\,\\SurchargeCode2\\,";
        //            //Header = Header + "\\SurchargeCode3\\,\\UnitWeightUM1\\,\\UnitWeightUM2\\,\\UnitWeightUM3\\,\\ItemClass\\,\\ItemSubclass\\,\\PriceClass\\,\\QuantityBreakClass\\,\\MfgItemNumber\\,\\ContainerCharge1\\,\\ContainerCharge2\\,\\ContainerCharge3\\,\\ListPrice1\\,\\ListPrice2\\,\\ListPrice3\\,\\ListPrice4\\,\\ListPrice5\\,\\TaxableCode\\,";
        //            //Header = Header + "\\ReuseCode\\,\\BackorderCode\\,\\AllowCashDiscountCode\\,\\MiscCode1\\,\\MiscCode2\\,\\MiscCode3\\,\\CompanyNumber\\,\\UpdateInventoryCode\\,\\CatchWeightCode\\,\\ItemGLCode\\,\\LocationClassCode\\,\\FederalExciseTaxAmount\\,\\VendorNumber\\,\\LocationSize1\\,\\LocationSize2\\,\\LocationSize3\\,\\WHManagementCode\\,";
        //            //Header = Header + "\\ExpirationDateRequiredFlag\\,\\ShowLotSerialonInvoice\\,\\15CharacterUserArea\\,\\ProductRestrictionCode\\,\\MSDSDate\\,\\ItemTaxClass\\,\\RebateClass\\,\\UserField1\\,\\UserField2\\,\\UserField3\\,\\UserField4\\,\\UserField5\\,\\UserField6\\,\\InquiryUM\\,\\ReportingUM\\,\\ItemEIDGroup\\,\\ProductId\\,\\ItemContractCode\\,\\BuyerItemClass\\,";
        //            //Header = Header + "\\BuyerItemSubclass\\,\\ItemCommitmentCode\\,\\HarmonizeTariffCode\\,\\CommodityCode\\,\\UniqueLots\\,\\ PreventFromWeb\\,\\StandardUsage\\,\\30CharacterUserArea\\,\\TrackCountryofOrigin\\,\\DiscontinuedCode\\,\\UM1Length\\,\\UM1Width \\,\\UM1Height\\,\\UM2Length\\,\\UM2Width \\,\\UM2Height\\,\\UM3Length\\,\\UM3Width \\,\\UM3Height\\,";
        //            //Header = Header + "\\BoxType\\,\\BoxQuantityUM1\\,\\BoxQuantityUM2\\,\\BoxQuantityUM3\\,\\HazardousMaterialGrade\\,\\ProperShippingName\\,\\PrimaryHazardClassCode\\,\\DOTNumber\\,\\PackageGroup\\,\\HAZMATCasofIngredient1\\,\\HAZMATCasofIngredient2\\,\\HAZMATCasofIngredient3\\,\\HAZMATCasofIngredient4\\,\\HAZMATCasofIngredient5\\,";
        //            //Header = Header + "\\HAZMATCasofIngredient6\\,\\HAZMATCasbyWeight1\\,\\HAZMATCasbyWeight2\\,\\HAZMATCasbyWeight3\\,\\HAZMATCasbyWeight4\\,\\HAZMATCasbyWeight5\\,\\HAZMATCasbyWeight6\\,\\LimitedQuantity\\,\\HazardousMaterialMsgCd1\\,\\HazardousMaterialMsgCd2\\,\\HazardousMaterialMsgCd3\\,\\HazardousMaterialMsgCd4\\,\\DOTRegulated\\,";
        //            //Header = Header + "\\ProperShippingNameLine2\\,\\ProperShippingNameLine3\\,\\ProperShippingNameLine4\\,\\SubsidiaryHazardClassCode1\\,\\SubsidiaryHazardClassCode2\\,\\PackageType\\";

        //            //writer.WriteLine(Header.Replace('\\', '"'));
        //            //foreach (var row in RITMST)
        //            //{
        //            //    string Detail = "\\" + row.itm_num + "\\,\\" + row.FunctionCode + "\\,\\" + row.APlusDescription1 + "\\,\\" + row.APlusDescription2 + "\\,\\" + row.UnitofMeasure1 + "\\,\\" + row.UnitofMeasure2 + "\\,\\" + row.UnitofMeasure3 + "\\,\\" + row.UMConversion1 + "\\,\\" + row.UMConversion2 + "\\,\\" + row.DefaultUMCode + "\\,\\" + row.PricingUM + "\\,\\" + row.PricingUMConversion + "\\,\\" + row.UMSurcharge1 + "\\,\\" + row.UMSurcharge2 + "\\,\\" + row.UMSurcharge3 + "\\,\\" + row.SurchargeCode1 + "\\,\\" + row.SurchargeCode2 + "\\,";
        //            //    Detail = Detail + "\\" + row.SurchargeCode3 + "\\,\\" + row.UnitWeightUM1 + "\\,\\" + row.UnitWeightUM2 + "\\,\\" + row.UnitWeightUM3 + "\\,\\" + row.ItemClass + "\\,\\" + row.ItemSubclass + "\\,\\" + row.PriceClass + "\\,\\" + row.QuantityBreakClass + "\\,\\" + row.MfgItemNumber + "\\,\\" + row.ContainerCharge1 + "\\,\\" + row.ContainerCharge2 + "\\,\\" + row.ContainerCharge3 + "\\,\\" + row.ListPrice1 + "\\,\\" + row.ListPrice2 + "\\,\\" + row.ListPrice3 + "\\,\\" + row.ListPrice4 + "\\,\\" + row.ListPrice5 + "\\,\\" + row.TaxableCode + "\\,";
        //            //    Detail = Detail + "\\" + row.ReuseCode + "\\,\\" + row.BackorderCode + "\\,\\" + row.AllowCashDiscountCode + "\\,\\" + row.MiscCode1 + "\\,\\" + row.MiscCode2 + "\\,\\" + row.MiscCode3 + "\\,\\" + row.CompanyNumber + "\\,\\" + row.UpdateInventoryCode + "\\,\\" + row.CatchWeightCode + "\\,\\" + row.ItemGLCode + "\\,\\" + row.LocationClassCode + "\\,\\" + row.FederalExciseTaxAmount + "\\,\\" + row.VendorNumber + "\\,\\" + row.LocationSize1 + "\\,\\" + row.LocationSize2 + "\\,\\" + row.LocationSize3 + "\\,\\" + row.WHManagementCode + "\\,";
        //            //    Detail = Detail + "\\" + row.ExpirationDateRequiredFlag + "\\,\\" + row.ShowLotSerialonInvoice + "\\,\\" + row.C15CharacterUserArea + "\\,\\" + row.ProductRestrictionCode + "\\,\\" + row.MSDSDate + "\\,\\" + row.ItemTaxClass + "\\,\\" + row.RebateClass + "\\,\\" + row.UserField1 + "\\,\\" + row.UserField2 + "\\,\\" + row.UserField3 + "\\,\\" + row.UserField4 + "\\,\\" + row.UserField5 + "\\,\\" + row.UserField6 + "\\,\\" + row.InquiryUM + "\\,\\" + row.ReportingUM + "\\,\\" + row.ItemEIDGroup + "\\,\\" + row.ProductId + "\\,\\" + row.ItemContractCode + "\\,\\" + row.BuyerItemClass + "\\,";
        //            //    Detail = Detail + "\\" + row.BuyerItemSubclass + "\\,\\" + row.ItemCommitmentCode + "\\,\\" + row.HarmonizeTariffCode + "\\,\\" + row.CommodityCode + "\\,\\" + row.UniqueLots + "\\,\\" + row.PreventFromWeb + "\\,\\" + row.StandardUsage + "\\,\\" + row.C30CharacterUserArea + "\\,\\" + row.TrackCountryofOrigin + "\\,\\" + row.DiscontinuedCode + "\\,\\" + row.UM1Length + "\\,\\" + row.UM1Width + "\\,\\" + row.UM1Height + "\\,\\" + row.UM2Length + "\\,\\" + row.UM2Width + "\\,\\" + row.UM2Height + "\\,\\" + row.UM3Length + "\\,\\" + row.UM3Width + "\\,\\" + row.UM3Height + "\\,";
        //            //    Detail = Detail + "\\" + row.BoxType + "\\,\\" + row.BoxQuantityUM1 + "\\,\\" + row.BoxQuantityUM2 + "\\,\\" + row.BoxQuantityUM3 + "\\,\\" + row.HazardousMaterialGrade + "\\,\\" + row.ProperShippingName + "\\,\\" + row.PrimaryHazardClassCode + "\\,\\" + row.DOTNumber + "\\,\\" + row.PackageGroup + "\\,\\" + row.HAZMATCasofIngredient1 + "\\,\\" + row.HAZMATCasofIngredient2 + "\\,\\" + row.HAZMATCasofIngredient3 + "\\,\\" + row.HAZMATCasofIngredient4 + "\\,\\" + row.HAZMATCasofIngredient5 + "\\,";
        //            //    Detail = Detail + "\\" + row.HAZMATCasofIngredient6 + "\\,\\" + row.HAZMATCasbyWeight1 + "\\,\\" + row.HAZMATCasbyWeight2 + "\\,\\" + row.HAZMATCasbyWeight3 + "\\,\\" + row.HAZMATCasbyWeight4 + "\\,\\" + row.HAZMATCasbyWeight5 + "\\,\\" + row.HAZMATCasbyWeight6 + "\\,\\" + row.LimitedQuantity + "\\,\\" + row.HazardousMaterialMsgCd1 + "\\,\\" + row.HazardousMaterialMsgCd2 + "\\,\\" + row.HazardousMaterialMsgCd3 + "\\,\\" + row.HazardousMaterialMsgCd4 + "\\,\\" + row.DOTRegulated + "\\,";
        //            //    Detail = Detail + "\\" + row.ProperShippingNameLine2 + "\\,\\" + row.ProperShippingNameLine3 + "\\,\\" + row.ProperShippingNameLine4 + "\\,\\" + row.SubsidiaryHazardClassCode1 + "\\,\\" + row.SubsidiaryHazardClassCode2 + "\\,\\" + row.PackageType + "\\";
        //            //    writer.WriteLine(Detail.Replace('\\', '"'));
        //            }
        //        }
        //    }
        //    var RZADITM = db.APlusImport_ZADITM(KeyCode);
        //    //                string ZADITM = "C:\\XMLFiles\\ZADITM" + SystemDate.Replace("/", "").Replace(":", "").Replace(" ", "") + ".txt";
        //    string ZADITM = AplusFileLocation + "ZADITMD" + SystemDate.Replace("/", "").Replace(":", "").Replace(" ", "") + ".csv";
        //    FileInfo FIZADITM = new FileInfo(ZADITM);
        //    if (!FIZADITM.Exists)
        //    {
        //        using (StreamWriter writer = FIZADITM.CreateText())
        //        {
        //            //string Header = "Itm_num|MSRP|FunctionCode|DropShipSpecialOrder|STD|MIN|VICost|UPC|FFLCaliber|FFLModel|FFLMFGName|FFLMFGImportName|MinAdvertisePrice";
        //            //writer.WriteLine(Header);
        //            foreach (var row in RZADITM)
        //            {
        //                string Detail = row.Itm_num + "|" + row.MSRP + "|" + row.FunctionCode + "|" + row.DropShipSpecialOrder + "|" + row.STD + "|" + row.MIN + "|" + row.VICost + "|" + row.UPC + "|" + row.FFLCaliber + "|" + row.FFLModel + "|" + row.FFLMFGName + "|" + row.FFLMFGImportName + "|" + row.MinAdvertisePrice ;
        //                writer.WriteLine(Detail);
        //            }
        //        }
        //    }
        //    // var resultMarkPrinted = db.Database.ExecuteSqlCommand("APlusItemsPrinted @UserId", new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));
        //    ViewBag.APlusImportMessage = "APlus import file created successfully.";
        //    TempData["APlusImportMessage"] = "APlus import file created successfully.";
        //    var RunningOff = db.Database.ExecuteSqlCommand("TurnRunningFlagOff");
        //}
        //catch
        //{
        //    ViewBag.APlusImportMessage = "APlus import file creation failed.";
        //    TempData["APlusImportMessage"] = "APlus import file creation failed.";
        //    var RunningOffError = db.Database.ExecuteSqlCommand("TurnRunningFlagOff");

        //}

        //    }
        //    else {
        //        ViewBag.APlusImportMessage = "APlus import file creation currently running.";
        //        TempData["APlusImportMessage"] = "APlus import file creation currently running.";
        //    }
        //    return RedirectToAction("Index");

        //}

        //
        // GET: /Item/Details/5

        public ActionResult Details(int id = 0)
        {

            return View();
        }

        //        [HttpPost]
        //        public ActionResult SaveFastTrack([Bind(Exclude = "Item_id")]Item item)
        //        {

        //            var PlanYN = item.Plan_YN;
        //            var Haz = item.Haz;
        //            int userid = int.Parse(Session.Contents["UserID"].ToString());
        ////            if (Request.Form["ddlVendor"])

        ////            item.ManufacturerLogo_Id = Request.Form["ddlVendor"];
        ////ddlVendor
        ////ddlCategory
        ////ddlSubClasses
        ////ddlFineLine
        ////MFG_Number = 99999
        ////UM_Id = 25
        ////VICost = 0.00
        ////MSRP = 0.00
        ////Level1 = 0.00
        ////Level2 = 0.00
        ////Level3 = 0.00
        ////JSCLevel5 = 0.00
        ////ddlWebcode 13
        ////ddlFreight = 16
        ////Plan_YN = "N"
        ////ABC_Id = 9
        ////STD=1
        ////MIN=1
        //            db.Items.Add(item);
        //            item.CreatedBy = userid;
        //            item.CreatedDate = DateTime.Now;
        //            var excl = Request.Form["chkExclusive"];
        //            if (excl == "on")
        //            {

        //                item.Exclusive = "Y";
        //            }
        //            else
        //            {
        //                item.Exclusive = "N";
        //            }

        //            var alloc = Request.Form["chkAllocated"];
        //            if (alloc == "on")
        //            {
        //                item.Allocated = "Y";
        //            }
        //            else
        //            {
        //                item.Allocated = "N";
        //            }

        //            var ds = Request.Form["chkDropShip"];
        //            if (ds == "on")
        //            {
        //                item.DropShip = "Y";
        //            }
        //            else
        //            {
        //                item.DropShip = "N";
        //            }

        //            var pfw = Request.Form["chkPreventFromWeb"];
        //            if (pfw == "on")
        //            {
        //                item.PreventFromWeb = "Y";
        //            }
        //            else
        //            {
        //                item.PreventFromWeb = "N";
        //            }

        //            var so = Request.Form["chkSpecialOrder"];
        //            //string chkspecialorder;
        //            if (so == "on")
        //            {
        //                item.SpecialOrder = "Y";
        //            }
        //            else
        //            {
        //                item.SpecialOrder = "N";
        //            }

        //            item.FastTrack = "Y";
        //            item.ReadyForApproval = "Y";
        //            item.Approved = "Y";
        //            item.ApprovedBy = userid;
        //            item.ApprovedDate = DateTime.Now;
        //            item.FastTrackBy = userid;
        //            item.FastTrackDate = DateTime.Now;
        //                    db.SaveChanges();
        //                    var Item_Id = item.Item_id;
        //                    var CategoryClass_Id = item.CategoryClass_Id;
        ////                    GetCategoryClass(Item_Id);
        //                    if (ViewBag.CategoryClass == "N")
        //                    {
        //                        string nd = item.Item_Description;
        //                        var resultItemDescription = db.Database.ExecuteSqlCommand("UpdateItemDescription @Item_Id,@NewDescription,@UserId", new SqlParameter("@Item_Id", Item_Id),
        //                                                                                                    new SqlParameter("@NewDescription", nd),
        //                                                                                                    new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));
        //                    }
        //                    if (item.WareHousesList != null)
        //                    {
        //                        foreach (int WareHouseId in item.WareHousesList)
        //                        {
        //                            //Stored procedure
        //                            var result = db.Database.ExecuteSqlCommand("AddItemWareHouse @Item_Id,@WareHouse_Id", new SqlParameter("@Item_Id", Item_Id),
        //                                                                                                new SqlParameter("@WareHouse_Id", WareHouseId));
        //                        }
        //                    }
        //                    //Stored procedure
        //                    if ((item.Approved == "Y" || item.FastTrack == "Y") && (item.Itm_Num == "" || item.Itm_Num == null))
        //                    {
        //                        var result1 = db.Database.ExecuteSqlCommand("UpdateItemsWithItm_Num @Item_id,@CategoryClass_Id", new SqlParameter("@Item_id", Item_Id),
        //                                                                                            new SqlParameter("@CategoryClass_Id", CategoryClass_Id));
        //                    }

        //                    //Stored procedure
        //                    var resultAttributes = db.Database.ExecuteSqlCommand("AddNewItemDefaultAttributes @Item_Id,@CategoryClass_Id,@User_Id", new SqlParameter("@Item_Id", Item_Id),
        //                                                                                            new SqlParameter("@CategoryClass_Id", CategoryClass_Id),
        //                                                                                            new SqlParameter("@User_Id", userid));
        //                    return RedirectToAction("Index", "ItemAttribute", new { id = Item_Id });
        ////                }

        ////  If there is an error. 

        //            //var VendorNameList = db.zManufacturersLogoes.Select(z => new
        //            //{
        //            //    ManufacturerLogo_Id = z.ManufacturerLogo_Id,
        //            //    Description = z.APlusVendorName + "(" + z.VendorNumber + ")",
        //            //    OB = z.APlusVendorName
        //            //})
        //            //                                        .ToList();
        //            //ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description");

        //            //var ABC = db.ABC_Lookup.Select(a => new
        //            //{
        //            //    ABC_Id = a.ABC_Id,
        //            //    Description = a.ABC_Code + " - " + a.ABC_Description
        //            //})
        //            //                                        .ToList();
        //            //ViewBag.ABC_Lookup = new SelectList(ABC, "ABC_Id", "Description");
        //            ////            ViewBag.ABC_Lookup = new SelectList(db.ABC_Lookup, "ABC_Id", "ABC_Description");
        //            //var CategoryClass = db.CategoryClasses.Select(c => new
        //            //{
        //            //    CategoryClass_Id = c.CategoryClass_Id,
        //            //    Description = c.Category_Id + " - " + c.CategoryName
        //            //})
        //            //                                        .ToList();
        //            //ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description");
        //            //var SubClass = db.SubClasses.Select(s => new
        //            //{
        //            //    SubClassCode_Id = s.SubClassCode_Id,
        //            //    Description = s.SubClass_Id + " - " + s.SubClassName
        //            //})
        //            //                                        .ToList();
        //            //ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description");
        //            //var FineLineClass = db.FineLineClasses.Select(f => new
        //            //{
        //            //    FineLineCode_Id = f.FineLineCode_Id,
        //            //    Description = f.FineLine_Id + " - " + f.FinelineName
        //            //})
        //            //                                        .ToList();
        //            //ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description");
        //            //var CatWebCode = db.WebCodes.Select(w => new
        //            //{
        //            //    CatWebCode_Id = w.CatWebCode_Id,
        //            //    Description = w.WebCode1 + " - " + w.WebCodeDescription
        //            //})
        //            //                                        .ToList();
        //            //ViewBag.CatWebCode_Id = new SelectList(CatWebCode, "CatWebCode_Id", "Description");


        //            ////ViewBag.CategoryClasses = db.CategoryClasses.ToList();
        //            ////ViewBag.SubClasses = db.SubClasses.ToList();
        //            ////ViewBag.FineLineClasses = db.FineLineClasses.ToList();

        //            //var Freight = db.Freight_Lookup.Select(fl => new
        //            //{
        //            //    FreightLookup_Id = fl.Freight_Id,
        //            //    Description = fl.Freight_APlusClass + " - " + fl.Freight_ItemClass
        //            //})
        //            //                            .ToList();
        //            //ViewBag.Freight_Lookup = new SelectList(Freight, "FreightLookup_Id", "Description");
        //            ////ViewBag.Freight_Lookup = new SelectList(db.Freight_Lookup, "Freight_Id", "Freight_ItemClass");
        //            //ViewBag.UM_Lookup = new SelectList(db.UM_Lookup, "UM_Id", "UM_Description");
        //            //ViewBag.Plan = LoadYesNo(PlanYN);
        //            //ViewBag.Hazardous = LoadHazardous(Haz);
        //            //ViewBag.FFLType = new SelectList(db.FFLTypes, "FFLType_Id", "FFLType_Description");

        //            //ViewBag.WareHousesList = LoadWarehouseList();



        ////            return View(item);
        //        }


        //
        // GET: /Item/CopyNew

        public ActionResult CopyNew(int id = 0, int userid = 0)
        {

            var result2 = db.GetNewItem(id, userid);
            ViewBag.NewItemList = result2.ToList();
            Session.Contents["itemBeingCopied"] = id;

            foreach (var nii in ViewBag.NewItemList)
            {
                ViewBag.NewItemId = nii;
            }

            int usertypeid = int.Parse(Session.Contents["UserTypeId"].ToString());
            Item item = db.Items.Find(ViewBag.NewItemId);
            if (item == null)
            {
                return HttpNotFound();
            }

            ViewBag.Itemid = item.Item_id;
            ViewBag.VendorName = new SelectList(db.zManufacturersLogoes.OrderBy(ml => ml.APlusVendorName), "ManufacturerLogo_Id", "APlusVendorName", item.ManufacturerLogo_Id);
            ViewBag.VendorNumber = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.VendorNumber).FirstOrDefault();
            ViewBag.VendorAbbrev = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.Abbrev).FirstOrDefault();
            ViewBag.Buyer = item.Buyer;

            var ABC = db.ABC_Lookup.Select(a => new
            {
                ABC_Id = a.ABC_Id,
                Description = a.ABC_Code + " - " + a.ABC_Description,
                Enabled = a.Enabled
            }).Where(a => a.Enabled == true)
                                                    .ToList();
            ViewBag.ABC_Lookup = new SelectList(ABC, "ABC_Id", "Description", item.ABC_Id);

            //            ViewBag.ABC_Id = new SelectList(db.ABC_Lookup, "ABC_Id", "ABC_Code", item.ABC_Id);
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", item.CategoryClass_Id);
            var CategoryClassId = db.CategoryClasses.Where(c1 => c1.CategoryClass_Id == item.CategoryClass_Id)
                                    .Select(c1 => c1.Category_Id).FirstOrDefault();
            var SubClass = db.SubClasses.Where(s => s.Category_Id == CategoryClassId)
                .Select(s => new
                {
                    SubClassCode_Id = s.SubClassCode_Id,
                    Description = s.SubClass_Id + " - " + s.SubClassName
                })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", item.SubClassCode_Id);
            var SubClassId = db.SubClasses.Where(s1 => s1.SubClassCode_Id == item.SubClassCode_Id)
                 .Select(s1 => s1.SubClass_Id).FirstOrDefault();
            var FineLineClass = db.FineLineClasses.Where(f => f.Category_Id == CategoryClassId && f.SubClass_id == SubClassId)
                .Select(f => new
                {
                    FineLineCode_Id = f.FineLineCode_Id,
                    Description = f.FineLine_Id + " - " + f.FinelineName

                })
                                                    .ToList();
            ViewBag.Item_Description = ReplaceSpecialCharacters(item.Item_Description);
            ViewBag.SF_Item_Description = ReplaceSpecialCharacters(item.SF_Item_Description);
            ViewBag.APlusDescription1 = ReplaceSpecialCharacters(item.APlusDescription1);
            ViewBag.APlusDescription2 = ReplaceSpecialCharacters(item.APlusDescription2);
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", item.FineLineCode_Id);
            var Freight = db.Freight_Lookup.Select(fl => new
            {
                FreightLookup_Id = fl.Freight_Id,
                Description = fl.Freight_APlusClass + " - " + fl.Freight_ItemClass,
                Enabled = fl.Enabled
            }).Where(fl => fl.Enabled == true)
                                                    .ToList();
            ViewBag.Freight_Lookup = new SelectList(Freight, "FreightLookup_Id", "Description", item.Freight_Id);
            //            ViewBag.Freight_Id = new SelectList(db.Freight_Lookup, "Freight_Id", "Freight_APlusClass", item.Freight_Id);
            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup, "UM_Id", "UM_Description", item.UM_Id);
            var CatWebCode = db.WebCodes.Select(w => new
            {
                CatWebCode_Id = w.CatWebCode_Id,
                Description = w.WebCode1 + " - " + w.WebCodeDescription,
                Enabled = w.Enabled
            }).Where(w => w.Enabled == true)
                                                    .ToList();
            ViewBag.CatWebCode_Lookup = new SelectList(CatWebCode, "CatWebCode_Id", "Description", item.CatWebCode_Id);
            //            ViewBag.CatWebCode_Id = new SelectList(db.WebCodes, "CatWebCode_Id", "WebCode1", item.CatWebCode_Id);
            ViewBag.FFLType = new SelectList(db.FFLTypes.Where(ffl => ffl.Enabled == true), "FFLType_Id", "FFLType_Description", item.FFLType_Id);

            ViewBag.CreatedUser = db.Users.Where(usr => usr.User_id == item.CreatedBy)
                                                       .Select(usr => usr.FirstName + " " + usr.LastName).FirstOrDefault();
            GetExistingWareHousesList(item.Item_id);
            ViewBag.Company99_List = LoadCompany(item.Company99);
            ViewBag.Plan = LoadYesNo(item.Plan_YN);
            ViewBag.WholeSale_MTP = LoadYesNo(item.WholeSaleMTP);
            ViewBag.MinAdvertisePriceFlag = LoadYesNo(item.MinAdvertisePriceFlag);
            ViewBag.Hazardous = LoadHazardous(item.Haz);
            GetRemainingAttributeTypesCount(id);
            if (usertypeid == 6)
            {
                var BuyerList = db.Users.Where(u => (u.UserType_Id == 1) || (u.UserType_Id == 2)).Select(u => new
                {
                    Buyer_Id = u.User_id,
                    BuyerName = u.FirstName + " " + u.LastName
                }).OrderBy(u => u.BuyerName).ToList();
                ViewBag.BuyersList = new SelectList(BuyerList, "Buyer_Id", "BuyerName");

                return RedirectToAction("VendorEdit", "Item", new { id = ViewBag.NewItemId });
            }
            else
            {
                return RedirectToAction("Edit", "Item", new { id = ViewBag.NewItemId });
            }
        }
        //
        // GET: /Item/Create

        public ActionResult Create()
        {
            var ABC = db.ABC_Lookup.Select(a => new
            {
                ABC_Id = a.ABC_Id,
                Description = a.ABC_Code + " - " + a.ABC_Description,
                Enabled = a.Enabled
            }).Where(abc => abc.Enabled == true)
                                                    .ToList();
            ViewBag.ABC_Lookup = new SelectList(ABC, "ABC_Id", "Description");
            //            ViewBag.ABC_Lookup = new SelectList(db.ABC_Lookup, "ABC_Id", "ABC_Description");
            //ViewBag.CategoryClass = new SelectList(db.CategoryClasses, "CategoryClass_Id", "CategoryName");
            ViewBag.UserType = int.Parse(Session.Contents["UserTypeID"].ToString());
            string userfullname = Session.Contents["LogedUserFullName"].ToString();
            ViewBag.Buyer = userfullname;

            var BuyerList = db.Users.Where(u => (u.UserType_Id == 1) || (u.UserType_Id == 2)).Select(u => new
            {
                Buyer_Id = u.User_id,
                BuyerName = u.FirstName + " " + u.LastName
            }).OrderBy(u => u.BuyerName).ToList();
            ViewBag.BuyersList = new SelectList(BuyerList, "Buyer_Id", "BuyerName");

            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description");
            var SubClass = db.SubClasses.Select(s => new
            {
                SubClassCode_Id = s.SubClassCode_Id,
                Description = s.SubClass_Id + " - " + s.SubClassName
            })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description");
            var FineLineClass = db.FineLineClasses.Select(f => new
            {
                FineLineCode_Id = f.FineLineCode_Id,
                Description = f.FineLine_Id + " - " + f.FinelineName
            })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description");


            var VendorNameList = db.zManufacturersLogoes.Select(z => new
            {
                ManufacturerLogo_Id = z.ManufacturerLogo_Id,
                Description = z.APlusVendorName + "(" + z.VendorNumber + ") (" + z.Abbrev + ")",
                OB = z.APlusVendorName,
                Enabled = z.Enabled
            }).Where(z => z.Enabled == true)
                                                    .ToList();
            ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description");

            var CountryList = db.Countries.Select(c => new
            {
                countryOrig_id = c.id,
                CountryName = c.countryName,
                displayOrder = c.displayOrder
            });

            ViewBag.Countries = new SelectList(CountryList.OrderBy(i => i.displayOrder), "countryOrig_id", "CountryName");

            var Freight = db.Freight_Lookup.Select(fl => new
            {
                FreightLookup_Id = fl.Freight_Id,
                Description = fl.Freight_APlusClass + " - " + fl.Freight_ItemClass,
                Enabled = fl.Enabled
            }).Where(fl => fl.Enabled == true)
                                                  .ToList();
            ViewBag.Freight_Lookup = new SelectList(Freight.Where(fr => fr.Enabled == true), "FreightLookup_Id", "Description");
            //ViewBag.Freight_Lookup = new SelectList(db.Freight_Lookup, "Freight_Id", "Freight_ItemClass");
            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup.Where(um => um.Enabled == true), "UM_Id", "UM_Description");

            var CatWebCode = db.WebCodes.Select(w => new
            {
                CatWebCode_Id = w.CatWebCode_Id,
                Description = w.WebCode1 + " - " + w.WebCodeDescription,
                Enabled = w.Enabled
            }).Where(w => w.Enabled == true)
                                                    .ToList();
            ViewBag.CatWebCode_Lookup = new SelectList(CatWebCode, "CatWebCode_Id", "Description");

            //ViewBag.CatWebCode_Id = new SelectList(db.WebCodes, "CatWebCode_Id", "WebCodeDescription");
            ViewBag.WareHousesList = LoadWarehouseList();
            ViewBag.Company99_List = LoadCompany("");
            ViewBag.Plan = LoadYesNo("");
            ViewBag.WholeSale_MTP = LoadYesNo("");
            ViewBag.MinAdvertisePriceFlag = LoadYesNo("");
            ViewBag.Hazardous = LoadHazardous("");

            ViewBag.FFLType = new SelectList(db.FFLTypes.Where(ffl => ffl.Enabled == true), "FFLType_Id", "FFLType_Description");

            //GetRemainingAttributeTypesCount(id);
            Item newItem = new Item();
            //newItem.countryOrig_id = 1;
            //newItem.countryManuf_id = 1;
            //newItem.AltUM_id = 9;
            //newItem.UM_Id = 9;
            //newItem.EDIUM_id = 9;
            return View(newItem);
        }

        //
        // POST: /Item/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "Item_id")]Item model, HttpPostedFileBase image1)
        {
            var Company99 = model.Company99;
            var PlanYN = model.Plan_YN;
            var Haz = model.Haz;
            var WholeSaleMTP = model.WholeSaleMTP;
            var MinAdvertisePriceFlag = model.MinAdvertisePriceFlag;

            var ds = Request.Form["chkDropShip"];

            if ((model.UPC == null) && (ds != "on"))
                ModelState.AddModelError("UPC", "Please enter a selling UPC code.");
            if ((model.EDIUPC == null) && (ds != "on"))
                ModelState.AddModelError("EDIUPC", "Please enter a purchasing UPC code.");
            if (model.ManufacturerLogo_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("ManufacturerLogo_Id", "Please select a Vendor value.");
            }
            else
            {
                var vinfo = (from v in db.zManufacturersLogoes
                             where v.ManufacturerLogo_Id == model.ManufacturerLogo_Id && v.Enabled == true
                             select new { v.APlusVendorName, v.Abbrev, v.VendorNumber, v.ManufacturerLogo_Id, v.WebVendorName }).SingleOrDefault();
                int index = 0;
                if (model.Item_Description != null && model.Item_Description.Trim().Length > 0)
                    index = model.Item_Description.IndexOf(" ");
                else
                    ModelState.AddModelError("Item_Description", "Please enter item description.");
                if (index > 0)
                {
                    string vendorAbbrev = model.Item_Description.Substring(0, index);
                    if (!vendorAbbrev.Equals(vinfo.Abbrev.Trim()))
                        ModelState.AddModelError("Item_Description", "The first word of Item Description must be the vendor abbrevation!");
                    else
                    {
                        model.Item_Description = ReplaceTradeMarker(model.Item_Description);
                    }
                }
                string vendorNumber = vinfo.VendorNumber;
                var DupCheck = (from i in db.Items
                                join u in db.Users on i.CreatedBy equals u.User_id
                                where i.MFG_Number == model.MFG_Number && i.zManufacturersLogo.VendorNumber == vendorNumber
                                select new { i.Itm_Num, i.Item_Description, u.FirstName, u.LastName }).FirstOrDefault();
                if (DupCheck != null)
                    ModelState.AddModelError("MFG_Number", "The MFG_Number entered is a duplicate to an Item.");
            }
            if (model.CategoryClass_Id.ToString().Trim() == "0")
            {
                ModelState.AddModelError("CategoryClass_Id", "Please select a Category Class value.");
            }
            if (model.SubClassCode_Id.ToString().Trim() == "0" || model.SubClassCode_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("SubClassCode_Id", "Please select a SubClass value.");
            }
            if (model.FineLineCode_Id.ToString().Trim() == "0" || model.FineLineCode_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("FineLineCode_Id", "Please select a FineLine Class value.");
            }
            if (model.CategoryClass_Id == 1 || model.CategoryClass_Id == 2 || model.CategoryClass_Id == 28)
            {
                if (int.Parse(Session.Contents["UserTypeID"].ToString()) != 6)
                {
                    if (model.FFLType_Id == null || model.FFLType_Id.Value == 0)
                        ModelState.AddModelError("FFLType_Id", "Please select a FFL type.");
                    if ((model.FFLGauge == null || model.FFLGauge.Trim().Length == 0) && (model.FFLCaliber == null || model.FFLCaliber.Trim().Length == 0))
                        ModelState.AddModelError("FFLCaliber", "Please enter a caliber or a gauge.");
                    if (model.FFLModel == null || model.FFLModel.Trim().Length == 0)
                        ModelState.AddModelError("FFLModel", "Please enter a model.");
                    if (model.FFLMFGName == null || model.FFLMFGName.Trim().Length == 0)
                        ModelState.AddModelError("FFLMFGName", "Please enter a manufacturer name");
                }
            }

            if (model.UM_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("UM_Id", "Please select a Unit/Measure value.");
            }
            if (model.MFG_Number == null)
            {
                ModelState.AddModelError("MFG_Number", "Please enter a MFG Number.");
            }
            if (model.MSRP == null)
            {
                ModelState.AddModelError("MSRP", "Please enter a valid dollar amount.");
            }
            else if (model.MSRP <= 0)
                ModelState.AddModelError("MSRP", "Please enter an amount greater than 0.");
            if (model.WholeSaleMTP == null)
            {
                ModelState.AddModelError("WholeSaleMTP", "Please select a WholeSale MTP value.");
            }
            //if (model.MinAdvertisePriceFlag == null)
            //{
            //    ModelState.AddModelError("MinAdvertisePriceFlag", "Please select a Min. Advertise Price Flag value.");
            //}
            if (model.STD == null)
            {
                ModelState.AddModelError("STD", "Please enter a numeric value less than 99999.");
            }
            if (model.MIN == null)
            {
                ModelState.AddModelError("MIN", "Please enter a numeric value less than 99999.");
            }
            if (model.VICost == null)
            {
                ModelState.AddModelError("VICost", "Please enter a valid dollar amount.");
            }
            if (image1 != null && image1.FileName != null && image1.FileName.Length != 0)
            {
                string fileExtension = Path.GetExtension(image1.FileName.ToLower());
                if (!(fileExtension.Equals(".jpg") || fileExtension.Equals(".jpeg")))
                {
                    ModelState.AddModelError("ItmImage", "Upload only jpg/jpeg file");
                }
            }
            if (int.Parse(Session.Contents["UserTypeID"].ToString()) != 6)
            {

                if (model.Level1 == null)
                {
                    ModelState.AddModelError("Level1", "Please enter a valid dollar amount.");
                }
                else if (model.Level1 <= 0)
                    ModelState.AddModelError("Level1", "Please enter an amount greater than 0.");
                if (model.Level2 == null)
                {
                    ModelState.AddModelError("Level2", "Please enter a valid dollar amount.");
                }
                else if (model.Level2 <= 0)
                    ModelState.AddModelError("Level2", "Please enter an amount greater than 0.");
                if (model.Level3 == null)
                {
                    ModelState.AddModelError("Level3", "Please enter a valid dollar amount.");
                }
                else if (model.Level3 <= 0)
                    ModelState.AddModelError("Level3", "Please enter an amount greater than 0.");
                if (model.Level4 == null)
                {
                    ModelState.AddModelError("Level4", "Please enter a valid dollar amount.");
                }
                else if (model.Level4 <= 0)
                    ModelState.AddModelError("Level4", "Please enter an amount greater than 0.");
                if (model.JSCLevel5 == null)
                {
                    ModelState.AddModelError("JSCLevel5", "Please enter a valid dollar amount.");
                }
                else if (model.JSCLevel5 <= 0)
                    ModelState.AddModelError("JSCLevel5", "Please enter an amount greater than 0.");
                if (model.Plan_YN.ToString().Trim() == "")
                {
                    ModelState.AddModelError("Plan_YN", "Please select a Plan.");
                }
                if (model.CatWebCode_Id.ToString().Trim() == "")
                {
                    ModelState.AddModelError("CatWebCode_Id", "Please select a Web Code.");
                }
                if (model.Freight_Id.ToString().Trim() == "")
                {
                    ModelState.AddModelError("Freight_Id", "Please select a Freight.");
                }
                if (model.ABC_Id.ToString().Trim() == "")
                {
                    ModelState.AddModelError("ABC_Id", "Please select an ABC.");
                }
                if (model.WareHousesList == null)
                {
                    ModelState.AddModelError("WareHousesList", "Please select a warehouse.");
                }
            }
            else
            {
                //                model.WholeSaleMTP = null;
                model.Level1 = 0;
                model.Level2 = 0;
                model.Level3 = 0;
                model.JSCLevel5 = 0;
                model.CatWebCode_Id = 8; // Ellett/JSC WEB/Catalog
                model.Freight_Id = 16;
                model.ABC_Id = 9;
                //                model.STD = 1;
                //                model.MIN = "1";
                model.Plan_YN = "N";
                //                model.VICost = 1;
                //                model.MinAdvertisePrice = 0;

            }
            try
            {
                if (ModelState.IsValid)
                {

                    int userid = int.Parse(Session.Contents["UserID"].ToString());

                    db.Items.Add(model);
                    if (image1 != null)
                    {
                        model.ItmImage = new byte[image1.ContentLength];
                        image1.InputStream.Read(model.ItmImage, 0, image1.ContentLength);
                    }
                    model.CreatedBy = userid;
                    model.CreatedDate = DateTime.Now;
                    var excl = Request.Form["chkExclusive"];
                    if (excl == "on")
                    {

                        model.Exclusive = "Y";
                    }
                    else
                    {
                        model.Exclusive = "N";
                    }

                    var alloc = Request.Form["chkAllocated"];
                    if (alloc == "on")
                    {
                        model.Allocated = "Y";
                    }
                    else
                    {
                        model.Allocated = "N";
                    }

                    //moved higher in the code for use with upc requirements
                    //var ds = Request.Form["chkDropShip"];
                    if (ds == "on")
                    {
                        model.DropShip = "Y";
                    }
                    else
                    {
                        model.DropShip = "N";
                    }
                    string ItemDesc = model.Item_Description;
                    string va = Request.Form["VendorAbbrev"];
                    string APD1, APD2;

                    if (ItemDesc.Length > 3 && ItemDesc.Substring(0, 4).IndexOf(va) == 0)
                    {
                        if (ItemDesc.Trim().Length <= 31)
                        {
                            APD1 = ItemDesc;
                            APD2 = "";
                        }
                        else
                        {
                            var index = FindLastIndex(ItemDesc, 31);
                            APD1 = ItemDesc.Substring(0, index);
                            APD2 = ItemDesc.Substring(index + 1);
                        }
                    }
                    else
                    {
                        if (va.Trim().Length == 3)
                        {
                            if (ItemDesc.Trim().Length <= 27)
                            {
                                APD1 = va.Trim() + " " + ItemDesc;
                                APD2 = "";
                            }
                            else
                            {
                                var index = FindLastIndex(ItemDesc, 27);
                                APD1 = ItemDesc.Substring(0, index);
                                APD2 = ItemDesc.Substring(index + 1);
                            }

                        }
                        else
                        {
                            if (ItemDesc.Trim().Length <= 26)
                            {
                                APD1 = va.Trim() + " " + ItemDesc;
                                APD2 = "";
                            }
                            else
                            {
                                var index = FindLastIndex(ItemDesc, 26);
                                APD1 = ItemDesc.Substring(0, index);
                                APD2 = ItemDesc.Substring(index + 1);
                            }
                        }
                    }
                    var tempStr = "";
                    if (model.Allocated == "Y")
                        tempStr += "**A** ";

                    if (model.Exclusive == "Y")
                        tempStr += "*EXCL* ";
                    if (model.DropShip == "Y")
                        tempStr += "*D-S* ";
                    APD2 = tempStr + APD2;
                    if (APD2.Length > 31) APD2 = APD2.Substring(0, 31);
                    model.APlusDescription1 = APD1;
                    model.APlusDescription2 = APD2;

                    var pfw = Request.Form["chkPreventFromWeb"];
                    if (pfw == "on")
                    {
                        model.PreventFromWeb = "Y";
                    }
                    else
                    {
                        model.PreventFromWeb = "N";
                    }

                    var so = Request.Form["chkSpecialOrder"];
                    //string chkspecialorder;
                    if (so == "on")
                    {
                        model.SpecialOrder = "Y";
                    }
                    else
                    {
                        model.SpecialOrder = "N";
                    }

                    var rfa = Request.Form["chkReadyForApproval"];
                    //string chkspecialorder;
                    if (rfa == "on")
                    {
                        model.ReadyForApproval = "Y";
                    }
                    else
                    {
                        model.ReadyForApproval = "N";
                    }

                    var ft = Request.Form["chkFastTrack"];
                    //string chkspecialorder;
                    if (ft == "on")
                    {
                        model.FastTrack = "Y";
                    }
                    else
                    {
                        model.FastTrack = "N";
                    }

                    if (model.FastTrack == "Y")
                    {
                        model.ReadyForApproval = "Y";
                        model.Approved = "Y";
                        model.ApprovedBy = userid;
                        model.ApprovedDate = DateTime.Now;
                        model.FastTrackBy = userid;
                        model.FastTrackDate = DateTime.Now;
                    }
                    if ((model.Approved == "Y" || model.FastTrack == "Y") && (model.Itm_Num == "" || model.Itm_Num == null))
                    {
                        if (model.APlusDescription1 == null && model.APlusDescription2 == null)
                        {
                            if (model.Item_Description.Length < 31)
                            {
                                model.APlusDescription1 = model.Item_Description;
                                model.APlusDescription2 = "";
                            }
                            else
                            {
                                model.APlusDescription1 = model.Item_Description.Substring(0, 31);
                                model.APlusDescription2 = model.Item_Description.Substring(31);
                            }
                        }
                    }
                    if ((model.Approved == "Y" || model.FastTrack == "Y") && (model.Itm_Num == "" || model.Itm_Num == null))
                    {
                        if (model.APlusDescription1 == null && model.APlusDescription2 == null)
                        {
                            if (model.Item_Description.Length < 31)
                            {
                                model.APlusDescription1 = model.Item_Description;
                                model.APlusDescription2 = "";
                            }
                            else
                            {
                                var index = FindLastIndex(model.Item_Description, 31);
                                model.APlusDescription1 = model.Item_Description.Substring(0, index);
                                model.APlusDescription2 = model.Item_Description.Substring(index + 1);
                            }
                        }
                    }

                    db.SaveChanges();
                    var Item_Id = model.Item_id;
                    var CategoryClass_Id = model.CategoryClass_Id;
                    GetCategoryClass(Item_Id);
                    model.Item_Description = ReplaceSpecialCharacters(model.Item_Description);
                    model.SF_Item_Description = ReplaceSpecialCharacters(model.SF_Item_Description);
                    string nd = model.Item_Description;
                    //var resultItemDescription = db.Database.ExecuteSqlCommand("UpdateItemDescription @Item_Id,@NewDescription,@UserId", new SqlParameter("@Item_Id", Item_Id),
                    //                                                                            new SqlParameter("@NewDescription", nd),
                    //                                                                            new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));
                    //string nsfd = model.SF_Item_Description;
                    //var resultSFItemDescription = db.Database.ExecuteSqlCommand("UpdateSFItemDescription @Item_Id,@NewSFDescription,@UserId", new SqlParameter("@Item_Id", Item_Id),
                    //                                                                            new SqlParameter("@NewSFDescription", nd),
                    //                                                                            new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));
                    if (model.WareHousesList != null)
                    {
                        foreach (int WareHouseId in model.WareHousesList)
                        {
                            //Stored procedure
                            var result = db.Database.ExecuteSqlCommand("AddItemWareHouse @Item_Id,@WareHouse_Id", new SqlParameter("@Item_Id", Item_Id),
                                                                                                new SqlParameter("@WareHouse_Id", WareHouseId));
                        }
                    }
                    //Stored procedure
                    if ((model.Approved == "Y" || model.FastTrack == "Y") && (model.Itm_Num == "" || model.Itm_Num == null))
                    {
                        var result1 = db.Database.ExecuteSqlCommand("UpdateItemsWithItm_Num @Item_id,@CategoryClass_Id", new SqlParameter("@Item_id", Item_Id),
                                                                                            new SqlParameter("@CategoryClass_Id", CategoryClass_Id));
                        var APlusImportItemResult = db.Database.ExecuteSqlCommand("AddItemToAPlusImportItem @ItemId", new SqlParameter("@ItemId", Item_Id));

                    }

                    //Stored procedure
                    var resultAttributes = db.Database.ExecuteSqlCommand("AddNewItemDefaultAttributes @Item_Id,@CategoryClass_Id,@User_Id", new SqlParameter("@Item_Id", Item_Id),
                                                                                            new SqlParameter("@CategoryClass_Id", CategoryClass_Id),
                                                                                            new SqlParameter("@User_Id", userid));
                    return RedirectToAction("Index", "ItemAttribute", new { id = Item_Id });
                }
                else
                {
                    //foreach (ModelState modelState in ViewData.ModelState.Values)
                    //{
                    //    foreach (ModelError error in modelState.Errors)
                    //    {
                    //        //HttpContext.Response.Write(error);
                    //    }
                    //}
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes.  Try, again later please.");
            }

            var VendorNameList = db.zManufacturersLogoes.Select(z => new
            {
                ManufacturerLogo_Id = z.ManufacturerLogo_Id,
                Description = z.APlusVendorName + "(" + z.VendorNumber + ") (" + z.Abbrev + ")",
                OB = z.APlusVendorName,
                Enabled = z.Enabled
            }).Where(z => z.Enabled == true)
                                                    .ToList();
            ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description");

            var CountryList = db.Countries.Select(c => new
            {
                countryOrig_id = c.id,
                CountryName = c.countryName,
                displayOrder = c.displayOrder
            });

            ViewBag.Countries = new SelectList(CountryList.OrderBy(i => i.displayOrder), "countryOrig_id", "CountryName");
            ViewBag.UserType = int.Parse(Session.Contents["UserTypeID"].ToString());
            string userfullname = Session.Contents["LogedUserFullName"].ToString();
            ViewBag.Buyer = userfullname;
            var BuyerList = db.Users.Where(u => (u.UserType_Id == 1) || (u.UserType_Id == 2)).Select(u => new
            {
                Buyer_Id = u.User_id,
                BuyerName = u.FirstName + " " + u.LastName
            }).OrderBy(u => u.BuyerName).ToList();
            ViewBag.BuyersList = new SelectList(BuyerList, "Buyer_Id", "BuyerName");

            var ABC = db.ABC_Lookup.Select(a => new
            {
                ABC_Id = a.ABC_Id,
                Description = a.ABC_Code + " - " + a.ABC_Description,
                Enabled = a.Enabled
            }).Where(a => a.Enabled == true)
                                                    .ToList();
            ViewBag.ABC_Lookup = new SelectList(ABC, "ABC_Id", "Description");
            //            ViewBag.ABC_Lookup = new SelectList(db.ABC_Lookup, "ABC_Id", "ABC_Description");
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", model.CategoryClass_Id);
            var CategoryClassId = db.CategoryClasses.Where(c1 => c1.CategoryClass_Id == model.CategoryClass_Id)
                                    .Select(c1 => c1.Category_Id).FirstOrDefault();
            ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == model.CategoryClass_Id)
                                            .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();
            var SubClass = db.SubClasses.Where(s => s.Category_Id == CategoryClassId)
                .Select(s => new
                {
                    SubClassCode_Id = s.SubClassCode_Id,
                    Description = s.SubClass_Id + " - " + s.SubClassName
                })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", model.SubClassCode_Id);
            var SubClassId = db.SubClasses.Where(s1 => s1.SubClassCode_Id == model.SubClassCode_Id)
                 .Select(s1 => s1.SubClass_Id).FirstOrDefault();
            ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == model.SubClassCode_Id)
                                            .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            var FineLineClass = db.FineLineClasses.Where(f => f.Category_Id == CategoryClassId && f.SubClass_id == SubClassId)
                .Select(f => new
                {
                    FineLineCode_Id = f.FineLineCode_Id,
                    Description = f.FineLine_Id + " - " + f.FinelineName

                })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", model.FineLineCode_Id);
            ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == model.FineLineCode_Id)
                                            .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();
            var CatWebCode = db.WebCodes.Select(w => new
            {
                CatWebCode_Id = w.CatWebCode_Id,
                Description = w.WebCode1 + " - " + w.WebCodeDescription,
                Enabled = w.Enabled
            }).Where(w => w.Enabled == true)
                                                    .ToList();
            ViewBag.CatWebCode_Lookup = new SelectList(CatWebCode.Where(c => c.Enabled == true), "CatWebCode_Id", "Description");


            //ViewBag.CategoryClasses = db.CategoryClasses.ToList();
            //ViewBag.SubClasses = db.SubClasses.ToList();
            //ViewBag.FineLineClasses = db.FineLineClasses.ToList();

            var Freight = db.Freight_Lookup.Select(fl => new
            {
                FreightLookup_Id = fl.Freight_Id,
                Description = fl.Freight_APlusClass + " - " + fl.Freight_ItemClass,
                Enabled = fl.Enabled
            }).Where(fl => fl.Enabled == true)
                                        .ToList();
            ViewBag.Freight_Lookup = new SelectList(Freight, "FreightLookup_Id", "Description");
            //ViewBag.Freight_Lookup = new SelectList(db.Freight_Lookup, "Freight_Id", "Freight_ItemClass");
            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup.Where(um => um.Enabled == true), "UM_Id", "UM_Description");
            ViewBag.Company99_List = LoadCompany(Company99);
            ViewBag.Plan = LoadYesNo(PlanYN);
            ViewBag.WholeSale_MTP = LoadYesNo(WholeSaleMTP);
            ViewBag.MinAdvertisePriceFlag = LoadYesNo(MinAdvertisePriceFlag);
            ViewBag.Hazardous = LoadHazardous(Haz);
            ViewBag.FFLType = new SelectList(db.FFLTypes.Where(ffl => ffl.Enabled == true), "FFLType_Id", "FFLType_Description");

            if (model.WareHousesList == null)
            {
                ViewBag.WareHousesList = LoadWarehouseList();
            }
            else
            {

                List<string> WareHousesSelected = new List<string>();
                foreach (int WareHouseId in model.WareHousesList)
                {
                    WareHousesSelected.Add(WareHouseId.ToString());

                }
                ViewBag.WareHousesList = ReLoadWarehouseList(WareHousesSelected);
            }
            if (Request.Form["chkExclusive"] == "on")
                ViewBag.Exclusive = "checked";
            else
                ViewBag.Exclusive = "";
            if (Request.Form["chkAllocated"] == "on")
                ViewBag.Allocated = "checked";
            else
                ViewBag.Allocated = "";
            if (Request.Form["chkDropShip"] == "on")
                ViewBag.DropShip = "checked";
            else
                ViewBag.DropShip = "";
            if (Request.Form["chkPreventFromWeb"] == "on")
                ViewBag.PreventFromWeb = "checked";
            else
                ViewBag.PreventFromWeb = "";
            if (Request.Form["chkSpecialOrder"] == "on")
                ViewBag.SpecialOrder = "checked";
            else
                ViewBag.SpecialOrder = "";
            if (Request.Form["chkFastTrack"] == "on")
                ViewBag.FastTrack = "checked";
            else
                ViewBag.FastTrack = "";

            return View(model);
        }

        protected int FindLastIndex(string ItemDesc, int index)
        {
            var s27 = "";
            var s28 = "";
            s27 = ItemDesc.Substring(0, index);
            s28 = ItemDesc.Substring(0, index + 1);
            if (s27.Length == s28.Trim().Length) //lucky
            {; }
            else
            {
                index = FindLastIndex(ItemDesc, index - 1);
            }
            return index;
        }
        //
        // GET: /Item/Edit/5

        public ActionResult VendorEdit(int id = 0)
        {
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            if (item.ItmImage == null)
            {
                item.ImageName = "NoImage";
                var image = db.UsefulImages.Where(i => i.Id == 1).FirstOrDefault();
                if (image != null)
                    item.ItmImage = GetNoImageData();
                long imageLength = item.ItmImage.Length;
            }
            ViewBag.UserType = int.Parse(Session.Contents["UserTypeID"].ToString());
            ViewBag.Itemid = item.Item_id;
            var VendorNameList = db.zManufacturersLogoes.Select(z => new
            {
                ManufacturerLogo_Id = z.ManufacturerLogo_Id,
                Description = z.APlusVendorName + "(" + z.VendorNumber + ") (" + z.Abbrev + ")",
                OB = z.APlusVendorName,
                Enabled = z.Enabled
            }).Where(z => z.Enabled == true)
                                                    .ToList();
            ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description", item.ManufacturerLogo_Id);
            ViewBag.VendorNumber = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.VendorNumber).FirstOrDefault();
            ViewBag.VendorAbbrev = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.Abbrev).FirstOrDefault();

            var BuyerList = db.Users.Where(u => (u.UserType_Id == 1) || (u.UserType_Id == 2)).Select(u => new
            {
                Buyer_Id = u.User_id,
                BuyerName = u.FirstName + " " + u.LastName
            }).OrderBy(u => u.BuyerName).ToList();
            ViewBag.BuyersList = new SelectList(BuyerList, "Buyer_Id", "BuyerName", item.AssignedBuyer_Id);
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", item.CategoryClass_Id);
            var CategoryClassId = db.CategoryClasses.Where(c1 => c1.CategoryClass_Id == item.CategoryClass_Id)
                                    .Select(c1 => c1.Category_Id).FirstOrDefault();
            ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == item.CategoryClass_Id)
                                            .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();
            var SubClass = db.SubClasses.Where(s => s.Category_Id == CategoryClassId)
                .Select(s => new
                {
                    SubClassCode_Id = s.SubClassCode_Id,
                    Description = s.SubClass_Id + " - " + s.SubClassName
                })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", item.SubClassCode_Id);
            var SubClassId = db.SubClasses.Where(s1 => s1.SubClassCode_Id == item.SubClassCode_Id)
                 .Select(s1 => s1.SubClass_Id).FirstOrDefault();
            ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == item.SubClassCode_Id)
                                            .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            var FineLineClass = db.FineLineClasses.Where(f => f.Category_Id == CategoryClassId && f.SubClass_id == SubClassId)
                .Select(f => new
                {
                    FineLineCode_Id = f.FineLineCode_Id,
                    Description = f.FineLine_Id + " - " + f.FinelineName

                })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", item.FineLineCode_Id);
            ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == item.FineLineCode_Id)
                                            .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();
            ViewBag.Item_Description = ReplaceSpecialCharacters(item.Item_Description);
            ViewBag.SF_Item_Description = ReplaceSpecialCharacters(item.SF_Item_Description);
            ViewBag.APlusDescription1 = ReplaceSpecialCharacters(item.APlusDescription1);
            ViewBag.APlusDescription2 = ReplaceSpecialCharacters(item.APlusDescription2);
            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup.Where(um => um.Enabled == true), "UM_Id", "UM_Description", item.UM_Id);

            ViewBag.CreatedUser = db.Users.Where(usr => usr.User_id == item.CreatedBy)
                                                       .Select(usr => usr.FirstName + " " + usr.LastName).FirstOrDefault();
            ViewBag.Hazardous = LoadHazardous(item.Haz);
            GetRemainingAttributeTypesCount(id);
            ViewBag.WholeSale_MTP = LoadYesNo(item.WholeSaleMTP);
            ViewBag.MinAdvertisePriceFlag = LoadYesNo(item.MinAdvertisePriceFlag);
            ViewBag.Printed = item.Printed;
            return View(item);
        }

        //
        // POST: /Item/Edit/5

        //[Bind(Exclude = "Item_id")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VendorEdit([Bind(Exclude = "Level1,Level2,Level3,JSCLevel5,Plan_YN,CatWebCode_Id,Approved,ApprovedBy,ApprovedDate,FastTrack,FastTrackBy,FastTrackDate,FFLCaliber,FFLMFGImportName,FFLMFGName,FFLModel,FFLGauge,FFLType_Id,Qty_Break,Qty_BreakPrice,Freight_Id,ABC_Id,ReadyForApproval,Haz")]Item item, HttpPostedFileBase image1)
        {

            if (item.ManufacturerLogo_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("ManufacturerLogo_Id", "Please select a Vendor value.");
            }
            if (item.CategoryClass_Id.ToString().Trim() == "0")
            {
                ModelState.AddModelError("CategoryClass_Id", "Please select a Category Class value.");
            }
            if (item.SubClassCode_Id.ToString().Trim() == "0" || item.SubClassCode_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("SubClassCode_Id", "Please select a SubClass value.");
            }
            if (item.FineLineCode_Id.ToString().Trim() == "0" || item.FineLineCode_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("FineLineCode_Id", "Please select a FineLine Class value.");
            }
            if (item.UM_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("UM_Id", "Please select a Unit/Measure value.");
            }
            if (item.MFG_Number == null)
            {
                ModelState.AddModelError("MFG_Number", "Please enter a MFG Number.");
            }
            if (item.MSRP == null)
            {
                ModelState.AddModelError("MSRP", "Please enter a valid dollar amount.");
            }
            if (item.WholeSaleMTP == null)
            {
                ModelState.AddModelError("WholeSaleMTP", "Please select a WholeSale MTP value.");
            }
            if (item.STD == null)
            {
                ModelState.AddModelError("STD", "Please enter a numeric value less than 99999.");
            }
            if (item.MIN == null)
            {
                ModelState.AddModelError("MIN", "Please enter a numeric value less than 99999.");
            }
            if (item.VICost == null)
            {
                ModelState.AddModelError("VICost", "Please enter a valid dollar amount.");
            }
            if (image1 != null && image1.FileName != null && image1.FileName.Length != 0)
            {
                string fileExtension = Path.GetExtension(image1.FileName.ToLower());
                if (!(fileExtension.Equals(".jpg") || fileExtension.Equals(".jpeg")))
                {
                    ModelState.AddModelError("ItmImage", "Upload only jpg/jpeg file");
                }
            }

            if (ModelState.IsValid)
            {
                var excl = Request.Form["chkExclusive"];
                if (excl == "on")
                {

                    item.Exclusive = "Y";
                }
                else
                {
                    item.Exclusive = "N";
                }

                var alloc = Request.Form["chkAllocated"];
                if (alloc == "on")
                {
                    item.Allocated = "Y";
                }
                else
                {
                    item.Allocated = "N";
                }

                var ds = Request.Form["chkDropShip"];
                if (ds == "on")
                {
                    item.DropShip = "Y";
                }
                else
                {
                    item.DropShip = "N";
                }

                var pfw = Request.Form["chkPreventFromWeb"];
                if (pfw == "on")
                {
                    item.PreventFromWeb = "Y";
                }
                else
                {
                    item.PreventFromWeb = "N";
                }

                var so = Request.Form["chkSpecialOrder"];
                //string chkspecialorder;
                if (so == "on")
                {
                    item.SpecialOrder = "Y";
                }
                else
                {
                    item.SpecialOrder = "N";
                }

                var dbItems = db.Items.FirstOrDefault(p => p.Item_id == item.Item_id);
                if (dbItems == null)
                {
                    return HttpNotFound();
                }
                if (image1 != null)
                {
                    dbItems.ItmImage = new byte[image1.ContentLength];
                    image1.InputStream.Read(dbItems.ItmImage, 0, image1.ContentLength);
                }
                else
                    dbItems.ItmImage = item.ItmImage;
                dbItems.ManufacturerLogo_Id = item.ManufacturerLogo_Id;
                dbItems.Item_Description = ReplaceSpecialCharacters(item.Item_Description);
                dbItems.SF_Item_Description = ReplaceSpecialCharacters(item.SF_Item_Description);
                dbItems.APlusDescription1 = ReplaceSpecialCharacters(item.APlusDescription1);
                dbItems.APlusDescription2 = ReplaceSpecialCharacters(item.APlusDescription2);
                dbItems.AssignedBuyer_Id = item.AssignedBuyer_Id;
                dbItems.MFG_Number = item.MFG_Number;
                dbItems.UM_Id = item.UM_Id;
                dbItems.MSRP = item.MSRP;
                dbItems.VICost = item.VICost;
                dbItems.CategoryClass_Id = item.CategoryClass_Id;
                dbItems.SubClassCode_Id = item.SubClassCode_Id;
                dbItems.FineLineCode_Id = item.FineLineCode_Id;
                dbItems.AssignedBuyer_Id = item.AssignedBuyer_Id;
                dbItems.DS = item.DS;
                dbItems.Haz = item.Haz;
                dbItems.UPC = item.UPC;
                dbItems.EDIUPC = item.EDIUPC;
                dbItems.WholeSaleMTP = item.WholeSaleMTP;
                dbItems.MinAdvertisePriceFlag = item.MinAdvertisePriceFlag;
                dbItems.STD = item.STD;
                dbItems.MIN = item.MIN;
                dbItems.MinAdvertisePrice = item.MinAdvertisePrice;
                dbItems.Buyer = item.Buyer;
                dbItems.Exclusive = item.Exclusive;
                dbItems.Allocated = item.Allocated;
                dbItems.DropShip = item.DropShip;
                dbItems.PreventFromWeb = item.PreventFromWeb;
                dbItems.SpecialOrder = item.SpecialOrder;

                //                    dbItems.CreatedBy = int.Parse(Session.Contents["UserID"].ToString());
                //                    dbItems.CreatedDate = item.CreatedDate;
                try
                {
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                    var CategoryClass_Id = item.CategoryClass_Id;

                    var Item_Id = item.Item_id;

                    GetCategoryClass(Item_Id);
                    string nd = item.Item_Description;
                    //var resultItemDescription = db.Database.ExecuteSqlCommand("UpdateItemDescription @Item_Id,@NewDescription,@UserId", new SqlParameter("@Item_Id", Item_Id),
                    //                                                                            new SqlParameter("@NewDescription", nd),
                    //                                                                            new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));
                    //string nsfd = item.SF_Item_Description;
                    //var resultSFItemDescription = db.Database.ExecuteSqlCommand("UpdateSFItemDescription @Item_Id,@NewSFDescription,@UserId", new SqlParameter("@Item_Id", Item_Id),
                    //                                                                            new SqlParameter("@NewSFDescription", nd),
                    //                                                                            new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));

                    return RedirectToAction("Index", "ItemAttribute", new { id = Item_Id });
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }

                }


            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }
            //}
            //catch (DataException)
            //{
            //    ModelState.AddModelError("", "Unable to save changes.  Try, again later please.");
            //}

            ViewBag.Itemid = item.Item_id;
            if (item.ItmImage == null)
            {
                item.ImageName = "NoImage";
                var image = db.UsefulImages.Where(i => i.Id == 1).FirstOrDefault();
                if (image != null)
                    item.ItmImage = GetNoImageData();
                long imageLength = item.ItmImage.Length;
            }
            ViewBag.UserType = int.Parse(Session.Contents["UserTypeID"].ToString());
            var VendorNameList = db.zManufacturersLogoes.Select(z => new
            {
                ManufacturerLogo_Id = z.ManufacturerLogo_Id,
                Description = z.APlusVendorName + "(" + z.VendorNumber + ") (" + z.Abbrev + ")",
                OB = z.APlusVendorName,
                Enabled = z.Enabled
            }).Where(z => z.Enabled == true)
                                                    .ToList();
            ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description", item.ManufacturerLogo_Id);
            ViewBag.VendorNumber = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                        .Select(vt => vt.VendorNumber).FirstOrDefault();
            ViewBag.VendorAbbrev = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                        .Select(vt => vt.Abbrev).FirstOrDefault();
            var BuyerList = db.Users.Select(u => new
            {
                Buyer_Id = u.User_id,
                BuyerName = u.FirstName + " " + u.LastName
            }).OrderBy(u => u.BuyerName).ToList();
            ViewBag.BuyersList = new SelectList(BuyerList, "Buyer_Id", "BuyerName", item.AssignedBuyer_Id);

            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", item.CategoryClass_Id);
            var CategoryClassId = db.CategoryClasses.Where(c1 => c1.CategoryClass_Id == item.CategoryClass_Id)
                                    .Select(c1 => c1.Category_Id).FirstOrDefault();
            ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == item.CategoryClass_Id)
                                            .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();
            var SubClass = db.SubClasses.Where(s => s.Category_Id == CategoryClassId)
                .Select(s => new
                {
                    SubClassCode_Id = s.SubClassCode_Id,
                    Description = s.SubClass_Id + " - " + s.SubClassName
                })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", item.SubClassCode_Id);
            var SubClassId = db.SubClasses.Where(s1 => s1.SubClassCode_Id == item.SubClassCode_Id)
                 .Select(s1 => s1.SubClass_Id).FirstOrDefault();
            ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == item.SubClassCode_Id)
                                            .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            var FineLineClass = db.FineLineClasses.Where(f => f.Category_Id == CategoryClassId && f.SubClass_id == SubClassId)
                .Select(f => new
                {
                    FineLineCode_Id = f.FineLineCode_Id,
                    Description = f.FineLine_Id + " - " + f.FinelineName

                })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", item.FineLineCode_Id);
            ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == item.FineLineCode_Id)
                                            .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();



            //var CategoryClass = db.CategoryClasses.Select(c => new
            //{
            //    CategoryClass_Id = c.CategoryClass_Id,
            //    Description = c.Category_Id + " - " + c.CategoryName
            //})
            //                                        .ToList();
            //ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", item.CategoryClass_Id);
            //ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == item.CategoryClass_Id)
            //                                .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();
            //var SubClass = db.SubClasses.Select(s => new
            //{
            //    SubClassCode_Id = s.SubClassCode_Id,
            //    Description = s.SubClass_Id + " - " + s.SubClassName
            //})
            //                                        .ToList();
            //ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", item.SubClassCode_Id);
            //ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == item.SubClassCode_Id)
            //                                .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            //var FineLineClass = db.FineLineClasses.Select(f => new
            //{
            //    FineLineCode_Id = f.FineLineCode_Id,
            //    Description = f.FineLine_Id + " - " + f.FinelineName
            //})
            //                                        .ToList();
            //ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", item.FineLineCode_Id);
            //ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == item.FineLineCode_Id)
            //                                .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();
            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup.Where(um => um.Enabled == true), "UM_Id", "UM_Description", item.UM_Id);
            ViewBag.Hazardous = LoadHazardous(item.Haz);
            ViewBag.WholeSale_MTP = LoadYesNo(item.WholeSaleMTP);
            ViewBag.MinAdvertisePriceFlag = LoadYesNo(item.MinAdvertisePriceFlag);

            ViewBag.CreatedUser = db.Users.Where(usr => usr.User_id == item.CreatedBy)
                                                       .Select(usr => usr.FirstName + " " + usr.LastName).FirstOrDefault();


            //            GetRemainingAttributeTypesCount(item.Item_id);



            return View(item);
        }

        public ActionResult Edit(int id = 0)
        {
            Item item = db.Items.Find(id);

            if (item == null)
            {
                return HttpNotFound();
            }
            if (item.ItmImage == null)
            {
                item.ImageName = "NoImage";
                var image = db.UsefulImages.Where(i => i.Id == 1).FirstOrDefault();
                if (image != null)
                    item.ItmImage = GetNoImageData();
                long imageLength = item.ItmImage.Length;
            }
            ViewBag.UserType = int.Parse(Session.Contents["UserTypeID"].ToString());
            ViewBag.Itemid = item.Item_id;
            var VendorNameList = db.zManufacturersLogoes.Select(z => new
            {
                ManufacturerLogo_Id = z.ManufacturerLogo_Id,
                Description = z.APlusVendorName + "(" + z.VendorNumber + ") (" + z.Abbrev + ")",
                OB = z.APlusVendorName,
                Enabled = z.Enabled
            }).Where(z => z.Enabled == true)
                                                    .ToList();
            ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description", item.ManufacturerLogo_Id);
            ViewBag.VendorNumber = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.VendorNumber).FirstOrDefault();
            ViewBag.VendorAbbrev = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.Abbrev).FirstOrDefault();


            var ABC = db.ABC_Lookup.Select(a => new
            {
                ABC_Id = a.ABC_Id,
                Description = a.ABC_Code + " - " + a.ABC_Description,
                Enabled = a.Enabled
            }).Where(a => a.Enabled == true)
                                                    .ToList();
            ViewBag.ABC_Lookup = new SelectList(ABC, "ABC_Id", "Description", item.ABC_Id);
            //            ViewBag.ABC_Id = new SelectList(db.ABC_Lookup, "ABC_Id", "ABC_Code", item.ABC_Id);
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", item.CategoryClass_Id);
            var CategoryClassId = db.CategoryClasses.Where(c1 => c1.CategoryClass_Id == item.CategoryClass_Id)
                                    .Select(c1 => c1.Category_Id).FirstOrDefault();
            ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == item.CategoryClass_Id)
                                            .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();

            var CountryList = db.Countries.Select(c => new
            {
                countryOrig_id = c.id,
                CountryName = c.countryName,
                displayOrder = c.displayOrder
            });

            ViewBag.Countries = new SelectList(CountryList.OrderBy(i => i.displayOrder), "countryOrig_id", "CountryName");
            var ReplacementItems = db.ReplacementItems.Where(r => r.ItmId == item.Item_id);
            if (ReplacementItems == null)
                item.ReplacementItems = new List<ReplacementItem>();
            else
                item.ReplacementItems = new List<ReplacementItem>(ReplacementItems);
            var replacementItemCodes = db.ReplacementItemCodes;
            foreach (var rItem in item.ReplacementItems)
                rItem.replacementCode = rItem.replacementCode + " - " + replacementItemCodes.Where(c => c.replacementCode == rItem.replacementCode).FirstOrDefault().replacementCodeDesc;
            var SubClass = db.SubClasses.Where(s => s.Category_Id == CategoryClassId)
                .Select(s => new
                {
                    SubClassCode_Id = s.SubClassCode_Id,
                    Description = s.SubClass_Id + " - " + s.SubClassName
                })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", item.SubClassCode_Id);
            var SubClassId = db.SubClasses.Where(s1 => s1.SubClassCode_Id == item.SubClassCode_Id)
                 .Select(s1 => s1.SubClass_Id).FirstOrDefault();
            ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == item.SubClassCode_Id)
                                            .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            var FineLineClass = db.FineLineClasses.Where(f => f.Category_Id == CategoryClassId && f.SubClass_id == SubClassId)
                .Select(f => new
                {
                    FineLineCode_Id = f.FineLineCode_Id,
                    Description = f.FineLine_Id + " - " + f.FinelineName

                })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", item.FineLineCode_Id);
            ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == item.FineLineCode_Id)
                                            .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();
            ViewBag.Item_Description = ReplaceSpecialCharacters(item.Item_Description);
            ViewBag.SF_Item_Description = ReplaceSpecialCharacters(item.SF_Item_Description);
            ViewBag.APlusDescription1 = ReplaceSpecialCharacters(item.APlusDescription1);
            ViewBag.APlusDescription2 = ReplaceSpecialCharacters(item.APlusDescription2);
            var Freight = db.Freight_Lookup.Select(fl => new
            {
                FreightLookup_Id = fl.Freight_Id,
                Description = fl.Freight_APlusClass + " - " + fl.Freight_ItemClass,
                Enabled = fl.Enabled
            }).Where(fl => fl.Enabled == true)
                                        .ToList();
            ViewBag.Freight_Lookup = new SelectList(Freight, "FreightLookup_Id", "Description", item.Freight_Id);
            //ViewBag.Freight_Id = new SelectList(db.Freight_Lookup, "Freight_Id", "Freight_APlusClass", item.Freight_Id);
            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup.Where(um => um.Enabled == true), "UM_Id", "UM_Description", item.UM_Id);

            var CatWebCode = db.WebCodes.Select(w => new
            {
                CatWebCode_Id = w.CatWebCode_Id,
                Description = w.WebCode1 + " - " + w.WebCodeDescription,
                Enabled = w.Enabled
            }).Where(w => w.Enabled == true)
                                                    .ToList();
            ViewBag.CatWebCode_Lookup = new SelectList(CatWebCode, "CatWebCode_Id", "Description", item.CatWebCode_Id);
            ViewBag.FFLType = new SelectList(db.FFLTypes, "FFLType_Id", "FFLType_Description", item.FFLType_Id);

            ViewBag.FFLTypeDisplay = db.FFLTypes.Where(ffl => ffl.FFLType_Id == item.FFLType_Id)
                                            .Select(ffl => ffl.FFLType_Code + "-" + ffl.FFLType_Description).FirstOrDefault();
            ViewBag.FFLLock = item.FFLLock;
            if (item.FFLLock == "Y")
            {
                ViewBag.FFLCaliberDisplay = item.FFLCaliber;
                ViewBag.FFLGaugeDisplay = item.FFLGauge;
                ViewBag.FFLModelDisplay = item.FFLModel;
                ViewBag.FFLMFGNameDisplay = item.FFLMFGName;
                ViewBag.FFLMFGImportNameDisplay = item.FFLMFGImportName;

            }
            ViewBag.CreatedUser = db.Users.Where(usr => usr.User_id == item.CreatedBy)
                                                       .Select(usr => usr.FirstName + " " + usr.LastName).FirstOrDefault();
            GetExistingWareHousesList(item.Item_id);

            if (Session.Contents["itemBeingCopied"] != null)
            {
                int itemIdBeingCopied = int.Parse(Session.Contents["itemBeingCopied"].ToString());
                Item itemBeingCopied = db.Items.Find(itemIdBeingCopied);
                ViewBag.Company99_List = LoadCompany(itemBeingCopied.Company99);
                ViewBag.MinAdvertisePrice_Flag = LoadYesNo(itemBeingCopied.MinAdvertisePriceFlag);
                ViewBag.Hazardous = LoadHazardous(itemBeingCopied.Haz);
                ViewBag.Plan = LoadYesNo(itemBeingCopied.Plan_YN);
                ViewBag.WholeSale_MTP = LoadYesNo(itemBeingCopied.WholeSaleMTP);
                Session.Contents["itemBeingCopied"] = null;
            }
            else
            {
                ViewBag.Company99_List = LoadCompany(item.Company99);
                ViewBag.MinAdvertisePrice_Flag = LoadYesNo(item.MinAdvertisePriceFlag);
                ViewBag.Hazardous = LoadHazardous(item.Haz);
                ViewBag.Plan = LoadYesNo(item.Plan_YN);
                ViewBag.WholeSale_MTP = LoadYesNo(item.WholeSaleMTP);
            }

            ViewBag.ReadyForApproval = item.ReadyForApproval;
            GetRemainingAttributeTypesCount(id);
            if (item.MSRP != 0)
            {
                var msrp = Math.Round((((item.MSRP - item.VICost) / item.MSRP) * 100).Value, 0);
                if (msrp != 0)
                {
                    if (msrp.ToString().Substring(0, 1) == "-")
                    {
                        ViewBag.Msrp_Percent = "";
                    }
                    else
                    {
                        ViewBag.Msrp_Percent = msrp.ToString();
                    }
                }
                else
                {
                    ViewBag.Msrp_Percent = "";
                }
            }
            if (int.Parse(Session.Contents["UserTypeID"].ToString()) != 6)
            {
                decimal lvl1;

                if (item.Level1 == 0)
                {
                    lvl1 = 1;
                }
                else
                {
                    lvl1 = (decimal)item.Level1;
                }
                var level1 = Math.Round((((lvl1 - item.VICost) / lvl1) * 100).Value, 0);
                if (level1 != 0)
                {
                    if (level1.ToString().Substring(0, 1) == "-")
                    {
                        ViewBag.Level1_Percent = "";
                    }
                    else
                    {
                        ViewBag.Level1_Percent = level1.ToString();
                    }
                }
                else
                {
                    ViewBag.Level1_Percent = "";
                }
                decimal lvl2;
                if (item.Level2 == 0)
                {
                    lvl2 = 1;
                }
                else
                {
                    lvl2 = (decimal)item.Level2;
                }
                var level2 = Math.Round((((lvl2 - item.VICost) / lvl2) * 100).Value, 0);
                if (level2 != 0)
                {
                    if (level2.ToString().Substring(0, 1) == "-")
                    {
                        ViewBag.Level2_Percent = "";
                    }
                    else
                    {
                        ViewBag.Level2_Percent = level2.ToString();
                    }
                }
                else
                {
                    ViewBag.Level2_Percent = "";
                }

                decimal lvl3;
                if (item.Level3 == 0)
                {
                    lvl3 = 1;
                }
                else
                {
                    lvl3 = (decimal)item.Level3;
                }
                var level3 = Math.Round((((lvl3 - item.VICost) / lvl3) * 100).Value, 0);
                if (level3 != 0)
                {
                    if (level3.ToString().Substring(0, 1) == "-")
                    {
                        ViewBag.Level3_Percent = "";
                    }
                    else
                    {
                        ViewBag.Level3_Percent = level3.ToString();
                    }
                }
                else
                {
                    ViewBag.Level3_Percent = "";
                }
                decimal lvl4;
                if (item.Level4 == 0 || item.Level4 == null)
                {
                    if (item.WholeSaleMTP == "Y")
                    {
                        lvl4 = lvl3;
                        item.Level4 = item.Level3;
                    }
                    else
                        lvl4 = 0;
                }
                else
                {
                    if (item.Level4 != null)
                        lvl4 = (decimal)item.Level4;
                    else
                        lvl4 = 0.0m;
                }
                var level4 = 0.0m;
                if (lvl4 > 0)
                    level4 = Math.Round((((lvl4 - item.VICost) / lvl4) * 100).Value, 0);
                if (level4 != 0)
                {
                    if (level4.ToString().Substring(0, 1) == "-")
                    {
                        ViewBag.Level4_Percent = "";
                    }
                    else
                    {
                        ViewBag.Level4_Percent = level4.ToString();
                    }
                }
                else
                {
                    ViewBag.Level4_Percent = "";
                }
                decimal lvl5;
                if (item.JSCLevel5 == 0)
                {
                    lvl5 = 1;
                }
                else
                {
                    lvl5 = (decimal)item.JSCLevel5;
                }
                var level5 = Math.Round((((lvl5 - item.VICost) / lvl5) * 100).Value, 0);
                if (level5 != 0)
                {
                    if (level5.ToString().Substring(0, 1) == "-")
                    {
                        ViewBag.Level5_Percent = "";
                    }
                    else
                    {
                        ViewBag.Level5_Percent = level5.ToString();
                    }
                }
                else
                {
                    ViewBag.Level5_Percent = "";
                }
            }
            return View(item);
        }

        protected byte[] GetNoImageData()
        {
            var image = db.UsefulImages.Where(i => i.Id == 1).FirstOrDefault();
            if (image != null)
                return image.imgData;
            else
                return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Item item, HttpPostedFileBase image1)
        {
            var ds = Request.Form["chkDropShip"];

            if (item.ManufacturerLogo_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("ManufacturerLogo_Id", "Please select a Vendor value.");
            }
            else
            {
                var vinfo = (from v in db.zManufacturersLogoes
                             where v.ManufacturerLogo_Id == item.ManufacturerLogo_Id && v.Enabled == true
                             select new { v.APlusVendorName, v.Abbrev, v.VendorNumber, v.ManufacturerLogo_Id, v.WebVendorName }).SingleOrDefault();
                int index = item.Item_Description.IndexOf(" ");
                string vendorAbbrev;
                if (index < 0)
                    vendorAbbrev = item.Item_Description.Trim();
                else
                    vendorAbbrev = item.Item_Description.Substring(0, index);
                if (!vendorAbbrev.Equals(vinfo.Abbrev.Trim()))
                    ModelState.AddModelError("Item_Description", "The first word of Item Description must be the vendor abbrevation!");

            }
            if (item.CategoryClass_Id.ToString().Trim() == "0")
            {
                ModelState.AddModelError("CategoryClass_Id", "Please select a Category Class value.");
            }
            else if (item.CategoryClass_Id == 1 || item.CategoryClass_Id == 2 || item.CategoryClass_Id == 28)
            {
                if (int.Parse(Session.Contents["UserTypeID"].ToString()) != 6)
                {
                    if (item.FFLType_Id == null || item.FFLType_Id.Value == 0)
                        ModelState.AddModelError("FFLType_Id", "Please select a FFL type.");
                    if ((item.FFLGauge == null || item.FFLGauge.Trim().Length == 0) && (item.FFLCaliber == null || item.FFLCaliber.Trim().Length == 0))
                        ModelState.AddModelError("FFLCaliber", "Please enter a caliber or a gauge.");
                    if (item.FFLModel == null || item.FFLModel.Trim().Length == 0)
                        ModelState.AddModelError("FFLModel", "Please enter a model.");
                    if (item.FFLMFGName == null || item.FFLMFGName.Trim().Length == 0)
                        ModelState.AddModelError("FFLMFGName", "Please enter a manufacturer name");
                }
            }
            if (item.SubClassCode_Id.ToString().Trim() == "0" || item.SubClassCode_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("SubClassCode_Id", "Please select a SubClass value.");
            }
            if (item.FineLineCode_Id.ToString().Trim() == "0" || item.FineLineCode_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("FineLineCode_Id", "Please select a FineLine Class value.");
            }
            if (item.UM_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("UM_Id", "Please select a Unit/Measure value.");
            }
            if (item.WholeSaleMTP == null)
            {
                ModelState.AddModelError("WholeSaleMTP", "Please select a WholeSale MTP value.");
            }
            if (item.MFG_Number == null)
            {
                ModelState.AddModelError("MFG_Number", "Please enter a MFG Number.");
            }
            if (item.MSRP == null)
            {
                ModelState.AddModelError("MSRP", "Please enter a valid dollar amount.");
            }
            if (item.Level1 == null)
            {
                ModelState.AddModelError("Level1", "Please enter a valid dollar amount.");
            }
            if (item.Level2 == null)
            {
                ModelState.AddModelError("Level2", "Please enter a valid dollar amount.");
            }
            if (item.Level3 == null)
            {
                ModelState.AddModelError("Level3", "Please enter a valid dollar amount.");
            }
            if (item.Level4 == null && item.WholeSaleMTP == "N")
            {
                ModelState.AddModelError("Level4", "Please enter a valid dollar amount.");
            }
            else if (item.Level4 == null && item.WholeSaleMTP == "Y")
            {
                item.Level4 = item.Level3;
            }

            if (item.JSCLevel5 == null)
            {
                ModelState.AddModelError("JSCLevel5", "Please enter a valid dollar amount.");
            }
            if (item.Plan_YN.ToString().Trim() == "")
            {
                ModelState.AddModelError("Plan_YN", "Please select a Plan.");
            }
            if (item.CatWebCode_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("CatWebCode_Id", "Please select a Web Code.");
            }
            if (item.Freight_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("Freight_Id", "Please select a Freight.");
            }
            if (item.ABC_Id.ToString().Trim() == "")
            {
                ModelState.AddModelError("ABC_Id", "Please select an ABC.");
            }
            if (item.STD == null)
            {
                ModelState.AddModelError("STD", "Please enter a numeric value less than 99999.");
            }
            if (item.MIN == null)
            {
                ModelState.AddModelError("MIN", "Please enter a numeric value less than 99999.");
            }
            if (item.VICost == null)
            {
                ModelState.AddModelError("VICost", "Please enter a valid dollar amount.");
            }
            if (item.WareHousesList == null)
            {
                ModelState.AddModelError("WareHousesList", "Please select a warehouse.");
            }
            if ((item.EDIUPC == null) && (ds != "on"))
            {
                ModelState.AddModelError("EDIUPC", "Please enter a value for purchasing UPC.");
            }
            if ((item.UPC == null) && (ds != "on"))
            {
                ModelState.AddModelError("UPC", "Please enter a value for selling UPC.");
            }
            if (image1 != null && image1.FileName != null && image1.FileName.Length != 0)
            {
                string fileExtension = Path.GetExtension(image1.FileName.ToLower());
                if (!(fileExtension.Equals(".jpg") || fileExtension.Equals(".jpeg")))
                {
                    ModelState.AddModelError("ItmImage", "Upload only jpg/jpeg file");
                }
            }
            if (ModelState.IsValid)
            {
                var excl = Request.Form["chkExclusive"];
                if (excl == "on")
                {

                    item.Exclusive = "Y";
                }
                else
                {
                    item.Exclusive = "N";
                }

                var alloc = Request.Form["chkAllocated"];
                if (alloc == "on")
                {
                    item.Allocated = "Y";
                }
                else
                {
                    item.Allocated = "N";
                }

                //moved to top of method for use with UPC logic
                //var ds = Request.Form["chkDropShip"];
                if (ds == "on")
                {
                    item.DropShip = "Y";
                }
                else
                {
                    item.DropShip = "N";
                }

                var pfw = Request.Form["chkPreventFromWeb"];
                if (pfw == "on")
                {
                    item.PreventFromWeb = "Y";
                }
                else
                {
                    item.PreventFromWeb = "N";
                }

                var so = Request.Form["chkSpecialOrder"];
                //string chkspecialorder;
                if (so == "on")
                {
                    item.SpecialOrder = "Y";
                }
                else
                {
                    item.SpecialOrder = "N";
                }

                var rfa = Request.Form["chkReadyForApproval"];
                //string chkspecialorder;
                if (rfa == "on")
                {
                    item.ReadyForApproval = "Y";
                }
                else
                {
                    item.ReadyForApproval = "N";
                }

                var ft = Request.Form["chkFastTrack"];
                //string chkspecialorder;
                if (ft == "on")
                {
                    item.FastTrack = "Y";
                }
                else
                {
                    item.FastTrack = "N";
                }
                var dbItems = db.Items.FirstOrDefault(p => p.Item_id == item.Item_id);
                if (dbItems == null)
                {
                    return HttpNotFound();
                }
                if (image1 != null)
                {
                    dbItems.ItmImage = new byte[image1.ContentLength];
                    image1.InputStream.Read(dbItems.ItmImage, 0, image1.ContentLength);
                }
                else
                    dbItems.ItmImage = item.ItmImage;
                dbItems.ManufacturerLogo_Id = item.ManufacturerLogo_Id;
                dbItems.Item_Description = ReplaceSpecialCharacters(item.Item_Description);
                dbItems.SF_Item_Description = ReplaceSpecialCharacters(item.SF_Item_Description);
                dbItems.APlusDescription1 = ReplaceSpecialCharacters(item.APlusDescription1);
                dbItems.APlusDescription2 = ReplaceSpecialCharacters(item.APlusDescription2);
                dbItems.MFG_Number = item.MFG_Number;
                dbItems.UM_Id = item.UM_Id;
                dbItems.MSRP = item.MSRP;
                dbItems.Level1 = item.Level1;
                dbItems.Level2 = item.Level2;
                dbItems.Level3 = item.Level3;
                dbItems.Level4 = item.Level4;
                dbItems.JSCLevel5 = item.JSCLevel5;
                dbItems.Qty_Break = item.Qty_Break;
                dbItems.Qty_BreakPrice = item.Qty_BreakPrice;
                dbItems.CatWebCode_Id = item.CatWebCode_Id;
                dbItems.Freight_Id = item.Freight_Id;
                dbItems.STD = item.STD;
                dbItems.MIN = item.MIN;
                dbItems.WholeSaleMTP = item.WholeSaleMTP;
                dbItems.MinAdvertisePriceFlag = item.MinAdvertisePriceFlag;
                dbItems.VICost = item.VICost;
                dbItems.ABC_Id = item.ABC_Id;
                dbItems.MinAdvertisePrice = item.MinAdvertisePrice;
                dbItems.CategoryClass_Id = item.CategoryClass_Id;
                dbItems.SubClassCode_Id = item.SubClassCode_Id;
                dbItems.FineLineCode_Id = item.FineLineCode_Id;
                dbItems.Company99 = item.Company99;
                dbItems.DS = item.DS;
                dbItems.Haz = item.Haz;
                dbItems.UPC = item.UPC;
                dbItems.EDIUPC = item.EDIUPC;
                dbItems.Buyer = item.Buyer;
                dbItems.Exclusive = item.Exclusive;
                dbItems.Allocated = item.Allocated;
                dbItems.DropShip = item.DropShip;
                dbItems.PreventFromWeb = item.PreventFromWeb;
                dbItems.SpecialOrder = item.SpecialOrder;

                //                    dbItems.CreatedBy = int.Parse(Session.Contents["UserID"].ToString());
                //                    dbItems.CreatedDate = item.CreatedDate;
                dbItems.ReadyForApproval = item.ReadyForApproval;
                if (item.ReadyForApproval == "Y" && item.ApprovedBy == null)
                {
                    dbItems.Approved = item.ReadyForApproval;
                    dbItems.ApprovedBy = int.Parse(Session.Contents["UserId"].ToString());
                    dbItems.ApprovedDate = DateTime.Now;
                }
                if (item.FastTrack == "Y" && item.FastTrackBy == null)
                {
                    dbItems.FastTrack = item.FastTrack;
                    dbItems.FastTrackBy = int.Parse(Session.Contents["UserId"].ToString());
                    dbItems.FastTrackDate = DateTime.Now;
                }
                dbItems.FFLCaliber = item.FFLCaliber;
                dbItems.FFLMFGImportName = item.FFLMFGImportName;
                dbItems.FFLMFGName = item.FFLMFGName;
                dbItems.FFLModel = item.FFLModel;
                dbItems.FFLGauge = item.FFLGauge;
                dbItems.FFLType_Id = item.FFLType_Id;
                dbItems.Plan_YN = item.Plan_YN;
                dbItems.countryOrig_id = item.countryOrig_id;
                dbItems.countryManuf_id = item.countryManuf_id;
                dbItems.AltUM_id = item.AltUM_id;
                dbItems.EDIUM_id = item.EDIUM_id;
                if ((dbItems.Approved == "Y" || dbItems.FastTrack == "Y"))
                {
                    if (dbItems.APlusDescription1 == null && dbItems.APlusDescription2 == null)
                    {
                        if (dbItems.Item_Description.Length < 31)
                        {
                            dbItems.APlusDescription1 = dbItems.Item_Description;
                            dbItems.APlusDescription2 = "";
                        }
                        else
                        {
                            var index = FindLastIndex(dbItems.Item_Description, 31);
                            dbItems.APlusDescription1 = dbItems.Item_Description.Substring(0, index);
                            dbItems.APlusDescription2 = dbItems.Item_Description.Substring(index + 1);
                            if (dbItems.APlusDescription2.Length > 31)
                                dbItems.APlusDescription2 = dbItems.APlusDescription2.Substring(0, 31);
                        }
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }


                var CategoryClass_Id = item.CategoryClass_Id;

                //db.Entry(item).State = EntityState.Modified;
                //db.SaveChanges();
                var Item_Id = item.Item_id;

                GetCategoryClass(Item_Id);
                string nd = item.Item_Description;

                //var resultItemDescription = db.Database.ExecuteSqlCommand("UpdateItemDescription @Item_Id,@NewDescription,@UserId", new SqlParameter("@Item_Id", Item_Id),
                //                                                                            new SqlParameter("@NewDescription", nd),
                //                                                                            new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));
                //string nsfd = item.SF_Item_Description;
                //var resultSFItemDescription = db.Database.ExecuteSqlCommand("UpdateSFItemDescription @Item_Id,@NewSFDescription,@UserId", new SqlParameter("@Item_Id", Item_Id),
                //                                                                            new SqlParameter("@NewSFDescription", nd),
                //                                                                            new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));


                //Stored procedure
                if ((dbItems.Approved == "Y" || dbItems.FastTrack == "Y") && (dbItems.Itm_Num == "" || dbItems.Itm_Num == null))
                {    //the following line be commented out because IMS2 credential can't access storefront database
                    var result1 = db.Database.ExecuteSqlCommand("UpdateItemsWithItm_Num @Item_id,@CategoryClass_Id", new SqlParameter("@Item_id", Item_Id),
                                                                                         new SqlParameter("@CategoryClass_Id", CategoryClass_Id));
                    var APlusImportItemResult = db.Database.ExecuteSqlCommand("AddItemToAPlusImportItem @ItemId", new SqlParameter("@ItemId", Item_Id));
                }


                var DeleteItemWareHouses = db.Database.ExecuteSqlCommand("DeleteItemWareHouses @Item_Id", new SqlParameter("@Item_Id", Item_Id));
                if (item.WareHousesList != null)
                {
                    foreach (int WareHouseId in item.WareHousesList)
                    {
                        //Stored procedure
                        var result = db.Database.ExecuteSqlCommand("AddItemWareHouse @Item_Id,@WareHouse_Id", new SqlParameter("@Item_Id", Item_Id),
                                                                                             new SqlParameter("@WareHouse_Id", WareHouseId));
                    }
                }
                return RedirectToAction("Index", "ItemAttribute", new { id = Item_Id });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }

            //}
            //catch (DataException)
            //{
            //    ModelState.AddModelError("", "Unable to save changes.  Try, again later please.");
            //}

            ViewBag.Itemid = item.Item_id;
            if (item.ItmImage == null)
            {
                item.ImageName = "NoImage";
                var image = db.UsefulImages.Where(i => i.Id == 1).FirstOrDefault();
                if (image != null)
                    item.ItmImage = GetNoImageData();
                long imageLength = item.ItmImage.Length;
            }
            var ReplacementItems = db.ReplacementItems.Where(r => r.ItmId == item.Item_id);
            if (ReplacementItems == null)
                item.ReplacementItems = new List<ReplacementItem>();
            else
                item.ReplacementItems = new List<ReplacementItem>(ReplacementItems);
            var replacementItemCodes = db.ReplacementItemCodes;
            foreach (var rItem in item.ReplacementItems)
                rItem.replacementCode = rItem.replacementCode + " - " + replacementItemCodes.Where(c => c.replacementCode == rItem.replacementCode).FirstOrDefault().replacementCodeDesc;
            ViewBag.UserType = int.Parse(Session.Contents["UserTypeID"].ToString());
            var VendorNameList = db.zManufacturersLogoes.Select(z => new
            {
                ManufacturerLogo_Id = z.ManufacturerLogo_Id,
                Description = z.APlusVendorName + "(" + z.VendorNumber + ") (" + z.Abbrev + ")",
                OB = z.APlusVendorName,
                Enabled = z.Enabled
            }).Where(z => z.Enabled == true)
                                                    .ToList();
            ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description", item.ManufacturerLogo_Id);

            var CountryList = db.Countries.Select(c => new
            {
                countryOrig_id = c.id,
                CountryName = c.countryName,
                displayOrder = c.displayOrder
            });

            ViewBag.Countries = new SelectList(CountryList.OrderBy(i => i.displayOrder), "countryOrig_id", "CountryName");
            ViewBag.VendorNumber = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                        .Select(vt => vt.VendorNumber).FirstOrDefault();
            ViewBag.VendorAbbrev = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                        .Select(vt => vt.Abbrev).FirstOrDefault();

            var ABC = db.ABC_Lookup.Select(a => new
            {
                ABC_Id = a.ABC_Id,
                Description = a.ABC_Code + " - " + a.ABC_Description,
                Enabled = a.Enabled
            }).Where(a => a.Enabled == true)
                                                    .ToList();
            ViewBag.ABC_Lookup = new SelectList(ABC, "ABC_Id", "Description", item.ABC_Id);
            //ViewBag.ABC_Lookup = new SelectList(db.ABC_Lookup, "ABC_Id", "ABC_Description", item.ABC_Id);

            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", item.CategoryClass_Id);
            var CategoryClassId = db.CategoryClasses.Where(c1 => c1.CategoryClass_Id == item.CategoryClass_Id)
                                    .Select(c1 => c1.Category_Id).FirstOrDefault();
            ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == item.CategoryClass_Id)
                                            .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();
            var SubClass = db.SubClasses.Where(s => s.Category_Id == CategoryClassId)
                .Select(s => new
                {
                    SubClassCode_Id = s.SubClassCode_Id,
                    Description = s.SubClass_Id + " - " + s.SubClassName
                })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", item.SubClassCode_Id);
            var SubClassId = db.SubClasses.Where(s1 => s1.SubClassCode_Id == item.SubClassCode_Id)
                 .Select(s1 => s1.SubClass_Id).FirstOrDefault();
            ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == item.SubClassCode_Id)
                                            .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            var FineLineClass = db.FineLineClasses.Where(f => f.Category_Id == CategoryClassId && f.SubClass_id == SubClassId)
                .Select(f => new
                {
                    FineLineCode_Id = f.FineLineCode_Id,
                    Description = f.FineLine_Id + " - " + f.FinelineName

                })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", item.FineLineCode_Id);
            ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == item.FineLineCode_Id)
                                            .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();




            //var CategoryClass = db.CategoryClasses.Select(c => new
            //{
            //    CategoryClass_Id = c.CategoryClass_Id,
            //    Description = c.Category_Id + " - " + c.CategoryName
            //})
            //                                        .ToList();
            //ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", item.CategoryClass_Id);
            //ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == item.CategoryClass_Id)
            //                                .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();
            //var SubClass = db.SubClasses.Select(s => new
            //{
            //    SubClassCode_Id = s.SubClassCode_Id,
            //    Description = s.SubClass_Id + " - " + s.SubClassName
            //})
            //                                        .ToList();
            //ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", item.SubClassCode_Id);
            //ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == item.SubClassCode_Id)
            //                                .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            //var FineLineClass = db.FineLineClasses.Select(f => new
            //{
            //    FineLineCode_Id = f.FineLineCode_Id,
            //    Description = f.FineLine_Id + " - " + f.FinelineName
            //})
            //                                        .ToList();
            //ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", item.FineLineCode_Id);
            //ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == item.FineLineCode_Id)
            //                                .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();
            ViewBag.Freight_Lookup = new SelectList(db.Freight_Lookup.Where(fr => fr.Enabled == true), "Freight_Id", "Freight_ItemClass", item.Freight_Id);
            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup.Where(um => um.Enabled == true), "UM_Id", "UM_Description", item.UM_Id);
            var CatWebCode = db.WebCodes.Select(w => new
            {
                CatWebCode_Id = w.CatWebCode_Id,
                Description = w.WebCode1 + " - " + w.WebCodeDescription,
                Enabled = w.Enabled
            }).Where(w => w.Enabled == true)
                                                    .ToList();
            ViewBag.CatWebCode_Lookup = new SelectList(CatWebCode, "CatWebCode_Id", "Description", item.CatWebCode_Id);
            //ViewBag.CatWebCode_Id = new SelectList(db.WebCodes, "CatWebCode_Id", "WebCode1", item.CatWebCode_Id);
            ViewBag.FFLType = new SelectList(db.FFLTypes.Where(ffl => ffl.Enabled == true), "FFLType_Id", "FFLType_Description", item.FFLType_Id);
            ViewBag.Company99_List = LoadCompany(item.Company99);
            ViewBag.Plan = LoadYesNo(item.Plan_YN);
            ViewBag.WholeSale_MTP = LoadYesNo(item.WholeSaleMTP);
            ViewBag.MinAdvertisePriceFlag = LoadYesNo(item.MinAdvertisePriceFlag);
            ViewBag.Hazardous = LoadHazardous(item.Haz);

            ViewBag.CreatedUser = db.Users.Where(usr => usr.User_id == item.CreatedBy)
                                                       .Select(usr => usr.FirstName + " " + usr.LastName).FirstOrDefault();

            if (item.WareHousesList == null)
                GetExistingWareHousesList(item.Item_id);
            else
            {
                var results1 = db.GetExistingWareHousesList(item.Item_id);
                var oldValue = results1.ToList();
                foreach (var w in oldValue)
                {
                    if (WidInWareHousesList(w.WareHouse_id, item.WareHousesList))
                        w.Selected = 1;
                    else
                        w.Selected = 0;
                }
                ViewBag.WareHousesList = oldValue;
            }
            GetRemainingAttributeTypesCount(item.Item_id);

            //            return RedirectToAction("Edit", "Item", new { id =item.Item_id });
            if (Request.Form["chkExclusive"] == "on")
                item.Exclusive = "Y";
            else
                item.Exclusive = "";
            if (Request.Form["chkAllocated"] == "on")
                item.Allocated = "Y";
            else
                item.Allocated = "";
            if (Request.Form["chkDropShip"] == "on")
                item.DropShip = "Y";
            else
                item.DropShip = "";
            if (Request.Form["chkPreventFromWeb"] == "on")
                item.PreventFromWeb = "Y";
            else
                item.PreventFromWeb = "";
            if (Request.Form["chkSpecialOrder"] == "on")
                item.SpecialOrder = "Y";
            else
                item.SpecialOrder = "";
            if (Request.Form["chkFastTrack"] == "on")
                item.FastTrack = "Y";
            else
                item.FastTrack = "";
            if (Request.Form["chkReadyForApproval"] == "on")
                item.ReadyForApproval = "Y";
            else
                item.ReadyForApproval = "";
            return View(item);
        }

        private bool WidInWareHousesList(int wid, List<int> list)
        {
            int w = list.Where(i => i == wid).FirstOrDefault();
            return w != 0;
        }

        //
        // GET: /Item/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            GetWarehouseList(item.Item_id);
            return View(item);
        }

        //
        // POST: /Item/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {

            var DeleteItemWareHouses = db.Database.ExecuteSqlCommand("DeleteItemWareHouses @Item_Id", new SqlParameter("@Item_Id", id));
            var DeleteItemAttributes = db.Database.ExecuteSqlCommand("DeleteItemAttributes @Item_Id", new SqlParameter("@Item_Id", id));
            var DeletePrinted = db.Database.ExecuteSqlCommand("DeletePrintedItemsForAPlus @Item_Id", new SqlParameter("@Item_Id", id));

            Item item = db.Items.Find(id);
            db.Items.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private IList<SubClass> GetSubClasses(int categoryclassid)
        {
            return (from s in db.SubClasses
                    join c in db.CategoryClasses on s.Category_Id equals c.Category_Id
                    where c.CategoryClass_Id == categoryclassid
                    select s).ToList();
        }


        //        private IList<FineLineClass> GetFineLineClasses(string categoryid, string subclassid)
        private IList<FineLineClass> GetFineLineClasses(int categoryclassid, int subclasscodeid)
        {
            return (from f in db.FineLineClasses
                    join c in db.CategoryClasses on f.Category_Id equals c.Category_Id
                    join s in db.SubClasses on f.SubClass_id equals s.SubClass_Id
                    where c.CategoryClass_Id == categoryclassid && s.SubClassCode_Id == subclasscodeid
                    select f).ToList();
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadSubClassesByCategoryId(int categoryclassid)
        {

            var SubClassList = this.GetSubClasses(categoryclassid);
            var SubClassData = SubClassList.Select(s => new
            {
                Text = s.SubClass_Id + " - " + s.SubClassName,
                Value = s.SubClassCode_Id,
            });
            return Json(SubClassData, JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadFineLineClassesByCateogryIdSubClassId(int categoryclassid, int subclasscodeid)
        {
            var FineLineList = this.GetFineLineClasses(categoryclassid, subclasscodeid);
            var FineLineData = FineLineList.Select(f => new
            {
                Text = f.FineLine_Id + " - " + f.FinelineName,
                Value = f.FineLineCode_Id,
            });
            return Json(FineLineData, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadVendorInfo(int vtid)
        {
            var vinfo = (from v in db.zManufacturersLogoes
                         where v.ManufacturerLogo_Id == vtid && v.Enabled == true
                         select new { v.APlusVendorName, v.Abbrev, v.VendorNumber, v.ManufacturerLogo_Id, v.WebVendorName }).SingleOrDefault();
            return Json(vinfo, JsonRequestBehavior.AllowGet);
        }


        private MultiSelectList LoadWarehouseList(List<int> selectedValues = null)
        {
            var list = db.WareHouse_Lookup.Select(whl => new
            {
                Id = whl.WareHouse_id,
                Name = whl.WareHouseName,
                Active = whl.Active
            }).Where(wl => wl.Active == true).ToList();

            return new MultiSelectList(list, "Id", "Name", selectedValues);
        }


        private MultiSelectList ReLoadWarehouseList(List<string> SelectedWH)
        {
            var list = db.WareHouse_Lookup.Select(whl => new
            {
                Id = whl.WareHouse_id,
                Name = whl.WareHouseName
            }).ToList();

            return new MultiSelectList(list, "Id", "Name", SelectedWH);
        }


        private void GetWarehouseList(int Itemid)
        {
            //var list = (from t in db.WareHouse_Lookup
            //            select new { Id = t.WareHouse_id, Name = t.WareHouseNumber + " - " + t.WareHouseName }).ToList();
            var list = (from whl in db.WareHouse_Lookup
                        join iwh in db.ItemWareHouses on whl.WareHouse_id equals iwh.WareHouse_id
                        where iwh.Item_id == Itemid && whl.Active == true
                        select whl).ToList();
            ViewBag.ExistingWareHousesList = list;
        }



        private List<SelectListItem> LoadHazardous(string defaultValue)
        {
            //            if (defaultValue != null)
            //            {
            //                defaultValue = defaultValue.Trim();
            //            }
            var Hazardous = new List<SelectListItem>
                                    {new SelectListItem { Text = "Yes", Value = "Y ", Selected = (defaultValue == "Y ")},
                                            new SelectListItem {Text = "No", Value  = "N ", Selected = (defaultValue == "N ")},
                                            new SelectListItem {Text = "CS", Value  = "CS", Selected = (defaultValue == "CS")},
                                            new SelectListItem {Text = "CC", Value = "CC", Selected = (defaultValue == "CC")}
                                        };
            return Hazardous.ToList();
        }

        private List<SelectListItem> LoadCompany(string defaultValue)
        {
            //          if (defaultValue != null){
            //              defaultValue = defaultValue.Trim();
            //          }
            var Company99 = new List<SelectListItem>
                                    {
                                        //new SelectListItem { Text = "", Value = "", Selected = (defaultValue == "")},
                                            new SelectListItem {Text = "99", Value  = "99", Selected = (defaultValue == "99")}
                                        };
            return Company99.ToList();
        }

        private List<SelectListItem> LoadYesNo(string defaultValue)
        {
            //          if (defaultValue != null){
            //              defaultValue = defaultValue.Trim();
            //          }
            var YesNo = new List<SelectListItem>
                                    {new SelectListItem { Text = "Yes", Value = "Y", Selected = (defaultValue == "Y")},
                                            new SelectListItem {Text = "No", Value  = "N", Selected = (defaultValue == "N")}
                                        };
            return YesNo.ToList();
        }

        public void GetCategoryClass(int Itemid)
        {
            var categoryClassid = db.Items.Where(i => i.Item_id == Itemid).Select(i => i.CategoryClass_Id).Single();
            if (categoryClassid == 1 || categoryClassid == 2 || categoryClassid == 3)//|| categoryClassid == 9) Optics Removed.
            {
                ViewBag.CategoryClass = "Y";
            }
            else
            {
                ViewBag.CategoryClass = "N";
            }
        }

        public void GetRemainingAttributeTypes(int Itemid)
        {
            var results1 = db.GetRemainingAttributeTypes(Itemid);
            ViewBag.RemainingAttributeTypes = results1.ToList();
        }
        public void GetRemainingAttributeTypesCount(int Itemid)
        {
            var results1 = db.GetRemainingAttributeTypes(Itemid).Where(r => r.Required == true);

            ViewBag.AllRequired = results1.Count();
        }

        public void GetExistingWareHousesList(int Itemid)
        {
            var results1 = db.GetExistingWareHousesList(Itemid);
            ViewBag.WareHousesList = results1.ToList();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult CalculatePercentage(decimal percent, decimal vicost)
        {
            if (percent == 100)
            {
                percent = 99;
            }
            var model = new
            {
                Cost = Math.Round(vicost / (1 - (percent / 100)), 2).ToString("#.00"),
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult CheckForMFG_Number(string mfgnumber, string vendornumber, int itemid)
        {
            var DupCheck = (from i in db.Items
                            join u in db.Users on i.CreatedBy equals u.User_id
                            where i.MFG_Number == mfgnumber && i.zManufacturersLogo.VendorNumber == vendornumber && i.Item_id != itemid
                            select new { i.Itm_Num, i.Item_Description, u.FirstName, u.LastName }).FirstOrDefault();
            if (DupCheck == null)
            {
                var DupCheck2 = (from i in db.APlusItems
                                 where i.MFG_Number == mfgnumber && i.VendorNumber == vendornumber
                                 select new { i.Itm_Num, i.Item_Desc1, i.Item_Desc2 }).FirstOrDefault();
                if (DupCheck2 == null)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(DupCheck2, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(DupCheck, JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult CheckForMFG_NumberCreate(string mfgnumber, string vendornumber)
        {
            var DupCheck = (from i in db.Items
                            join u in db.Users on i.CreatedBy equals u.User_id
                            where i.MFG_Number == mfgnumber && i.zManufacturersLogo.VendorNumber == vendornumber
                            select new { i.Itm_Num, i.Item_Description, u.FirstName, u.LastName }).FirstOrDefault();
            if (DupCheck == null)
            {
                var DupCheck2 = (from i in db.APlusItems
                                 where i.MFG_Number == mfgnumber && i.VendorNumber == vendornumber
                                 select new { i.Itm_Num, i.Item_Desc1, i.Item_Desc2 }).FirstOrDefault();
                if (DupCheck2 == null)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(DupCheck2, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(DupCheck, JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult CheckForEDIUPC(string ediupc, int itemid)
        {
            var EDIUPCCheck = (from i in db.Items
                               join u in db.Users on i.CreatedBy equals u.User_id
                               where i.UPC == ediupc && i.Item_id != itemid
                               select new { i.Itm_Num, i.Item_Description, u.FirstName, u.LastName }).FirstOrDefault();
            if (EDIUPCCheck == null)
            {
                var EDIUPCCheck2 = (from i in db.APlusItems
                                    where i.UPC == ediupc
                                    select new { i.Itm_Num, i.Item_Desc1, i.Item_Desc2 }).FirstOrDefault();
                if (EDIUPCCheck2 == null)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(EDIUPCCheck2, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(EDIUPCCheck, JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult CheckForEDIUPCCreate(string ediupc)
        {
            var EDIUPCCheck = (from i in db.Items
                               join u in db.Users on i.CreatedBy equals u.User_id
                               where i.UPC == ediupc
                               select new { i.Itm_Num, i.Item_Description, u.FirstName, u.LastName }).FirstOrDefault();
            if (EDIUPCCheck == null)
            {
                var EDIUPCCheck2 = (from i in db.APlusItems
                                    where i.UPC == ediupc
                                    select new { i.Itm_Num, i.Item_Desc1, i.Item_Desc2 }).FirstOrDefault();
                if (EDIUPCCheck2 == null)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(EDIUPCCheck2, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(EDIUPCCheck, JsonRequestBehavior.AllowGet);
            }

        }

        protected string ReplaceSpecialCharacters(string input)
        {
            if (input != null)
            {
                char[] charArr = input.ToCharArray();
                StringBuilder sb = new StringBuilder();
                foreach (char ch in charArr)
                {
                    if ((int)ch < 32 || (int)ch > 126)
                    {
                        if ((int)ch == 8217)
                        {
                            sb.Append((char)39);
                        }
                        else
                            sb.Append(" ");
                    }
                    else
                        sb.Append(ch);
                }
                return ReplaceTradeMarker(sb.ToString());
            }
            else
                return input;
        }

        /// <summary>
        /// This should allow all the keyboard characters except brackets [] since [] are reserved characters for regular expression
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected string ReplaceTradeMarker(string input) //mainly these three ©™®
        {
            string[] words = input.Split(' ');
            string newStr = "";
            foreach (var s in words)
            {
                if (s.Trim().Length != 0)
                    newStr += s + " ";
            }
            newStr = newStr.Trim();
            return newStr;
        }
    }
}