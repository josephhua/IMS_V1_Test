using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Data.Entity;
using PagedList;
using System.Data;
using System.Data.SqlClient;

namespace IMS_V1.Controllers
{
    public class ImportedItemController : Controller
    {
        public enum FileCategory { Gun, Ammo, Ims, Marine, NFA, NotDefined };
        private IMS_V1Entities db = new IMS_V1Entities();
        //
        // GET: /ImportedItem/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportItems(string currentFilter, string SearchString, int? page)
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
            ViewBag.userid = userid;
            ViewBag.CurrentFilter = SearchString;
            var items = from r in
                            db.ImportedItems
                        where r.CreatedBy == userid && (r.Imported == false || r.Imported == null)
                        select r;
            //var items = db.ImportedItems.Where(ii => ii.CreatedBy == userid && ii.Imported == false);
            if (!String.IsNullOrEmpty(SearchString))
            {
                items = items.Where(r => r.Item_Description.ToUpper().Contains(SearchString.ToUpper()) || r.MFG_Number.ToUpper().Contains(SearchString.ToUpper()));
            }
            items = items.OrderBy(r => r.ImportItem_id);
            foreach (var it in items)
            {
                var mLogo = db.zManufacturersLogoes.Where(l => l.ManufacturerLogo_Id == it.ManufacturerLogo_Id).FirstOrDefault();
                it.zManufacturersLogo = mLogo;
                string catId = it.CategoryClass_Id.ToString();
                if (catId.Length == 1) catId = "0" + catId;
                var cat = db.CategoryClasses.Where(c => c.Category_Id == catId).FirstOrDefault();
                it.CategoryClass = cat;
            }
            int pageSize = 30;
            int pageNumber = (page ?? 1);

            return View(items.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult CreateItem(int id = 0, int userid = 0)
        {
            ImportedItem item = db.ImportedItems.Find(id);
            Item newItem = new Item();
            newItem.ImportedItemId = id;
            newItem.ManufacturerLogo_Id = item.ManufacturerLogo_Id;
            newItem.Buyer = item.Buyer;
            newItem.CategoryClass_Id = item.CategoryClass_Id;
            newItem.SubClassCode_Id = item.SubClassCode_Id;
            newItem.FineLineCode_Id = item.FineLineCode_Id;
            newItem.Item_Description = item.Item_Description;
            newItem.SF_Item_Description = item.Item_Description;
            newItem.FFLType_Id = item.FFLType_Id;
            if (item.FFLGauge == "Caliber")
            {
                newItem.FFLGauge = "";
                newItem.FFLCaliber = item.FFLCaliber;
            }
            else
            {
                newItem.FFLCaliber = "";
                newItem.FFLGauge = item.FFLCaliber;
            }
            newItem.FFLModel = item.FFLModel;
            newItem.FFLMFGName = item.FFLMFGName;
            newItem.FFLMFGImportName = item.FFLMFGImportName;
            newItem.MFG_Number = item.MFG_Number;
            newItem.UPC = item.UPC;
            newItem.EDIUPC = item.UPC;
            newItem.UM_Id = item.UM_Id;
            newItem.AltUM_id = item.VdUM_id;
            newItem.VICost = item.VICost;
            newItem.WholeSaleMTP = GetTrueYN(item.WholeSaleMTP);
            newItem.MSRP = item.MSRP;
            newItem.Level1 = item.Level1;
            newItem.Level2 = item.Level2;
            newItem.Level3 = item.Level3;
            newItem.Level4 = item.Level4;
            newItem.JSCLevel5 = item.Level5;
            newItem.MinAdvertisePrice = item.MinAdvertisePrice;
            newItem.Qty_Break = item.Qty_Break;
            newItem.Qty_BreakPrice = item.Qty_BreakPrice;
            newItem.CatWebCode_Id = item.CatWebCode_Id;
            newItem.STD = item.STD;
            newItem.Freight_Id = item.Freight_Id;
            newItem.ABC_Id = item.ABC_Id;
            newItem.MIN = item.MIN;
            List<int> whlist = GetWareHouseList(item.WareHouses);
            newItem.Company99 = item.Company99;
            newItem.Haz = item.Haz;
            newItem.Plan_YN = GetTrueYN(item.Plan_YN);
            newItem.MinAdvertisePriceFlag = GetTrueYN(item.MinAdvertisePriceFlag);
            newItem.Exclusive = item.Exclusive;
            newItem.Allocated = item.Allocated;
            newItem.DropShip = item.DropShip;
            newItem.PreventFromWeb = item.PreventFromWeb;
            newItem.SpecialOrder = item.SpecialOrder;
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
            int vendorCount = db.zManufacturersLogoes.Count();

            var VendorNameList = db.zManufacturersLogoes.Select(z => new
            {
                ManufacturerLogo_Id = z.ManufacturerLogo_Id,
                Description = z.APlusVendorName + "(" + z.VendorNumber + ") (" + z.Abbrev + ")",
                OB = z.APlusVendorName,
                Enabled = z.Enabled
            }).Where(z => z.Enabled == true)
                                                    .ToList();
            var vendorList = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description");


            ViewBag.VendorName = new SelectList(VendorNameList.OrderBy(ml => ml.OB), "ManufacturerLogo_Id", "Description");

            //var CountryList = db.Countries.Select(c => new
            //{
            //    countryOrig_id = c.id,
            //    CountryName = c.countryName,
            //    displayOrder = c.displayOrder
            //});

            //ViewBag.Countries = new SelectList(CountryList.OrderBy(i => i.displayOrder), "countryOrig_id", "CountryName");

            var Freight = db.Freight_Lookup.Select(fl => new
            {
                FreightLookup_Id = fl.Freight_Id,
                Description = fl.Freight_APlusClass + " - " + fl.Freight_ItemClass,
                Enabled = fl.Enabled
            }).Where(fl => fl.Enabled == true)
                                                  .ToList();
            ViewBag.Freight_Lookup = new SelectList(Freight.Where(fr => fr.Enabled == true), "FreightLookup_Id", "Description");

            ViewBag.UM_Lookup = new SelectList(db.UM_Lookup.Where(um => um.Enabled == true), "UM_Id", "UM_Description");

            var CatWebCode = db.WebCodes.Select(w => new
            {
                CatWebCode_Id = w.CatWebCode_Id,
                Description = w.WebCode1 + " - " + w.WebCodeDescription,
                Enabled = w.Enabled
            }).Where(w => w.Enabled == true).ToList();
            ViewBag.CatWebCode_Lookup = new SelectList(CatWebCode, "CatWebCode_Id", "Description", newItem.CatWebCode_Id);
            ViewBag.WareHousesList = LoadWarehouseList(whlist);
            ViewBag.Company99_List = LoadCompany(newItem.Company99);
            ViewBag.Plan = LoadYesNo("");
            ViewBag.WholeSale_MTP = LoadYesNo("");
            ViewBag.MinAdvertiseFlag = LoadYesNo("");
            ViewBag.Hazardous = LoadHazardous(newItem.Haz);

            ViewBag.FFLType = new SelectList(db.FFLTypes.Where(ffl => ffl.Enabled == true), "FFLType_Id", "FFLType_Description");
            ViewBag.VendorNumber = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.VendorNumber).FirstOrDefault();
            ViewBag.VendorAbbrev = db.zManufacturersLogoes.Where(vt => vt.ManufacturerLogo_Id == item.ManufacturerLogo_Id)
                                                       .Select(vt => vt.Abbrev).FirstOrDefault();
            return View(newItem);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateItem([Bind(Exclude = "Item_id")]Item model)
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
                    var importedItem = db.ImportedItems.Find(model.ImportedItemId);
                    importedItem.Item_id = Item_Id;
                    importedItem.Imported = true;
                    importedItem.ImportedDt = DateTime.Now;
                    db.SaveChanges();
                    var CategoryClass_Id = model.CategoryClass_Id;
                    GetCategoryClass(Item_Id);

                    string nd = model.Item_Description;

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
                    //var resultAttributes = db.Database.ExecuteSqlCommand("AddNewItemDefaultAttributes @Item_Id,@CategoryClass_Id,@User_Id", new SqlParameter("@Item_Id", Item_Id),
                    //                                                                        new SqlParameter("@CategoryClass_Id", CategoryClass_Id),
                    //                                                                        new SqlParameter("@User_Id", userid));
                    //db.ItemAttributes;
                    if (NeedInsertItemAttributes(importedItem))
                    {
                        InsertItemAttributes(importedItem);
                    }
                    return RedirectToAction("Index", "ItemAttribute", new { id = Item_Id });
                }
                else
                {

                }
            }
            catch (DataException ex)
            {
                ModelState.AddModelError("", "Unable to save changes.  Try, again later please." + ex.ToString());
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

            //var CountryList = db.Countries.Select(c => new
            //{
            //    countryOrig_id = c.id,
            //    CountryName = c.countryName,
            //    displayOrder = c.displayOrder
            //});

            //ViewBag.Countries = new SelectList(CountryList.OrderBy(i => i.displayOrder), "countryOrig_id", "CountryName");
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


            var Freight = db.Freight_Lookup.Select(fl => new
            {
                FreightLookup_Id = fl.Freight_Id,
                Description = fl.Freight_APlusClass + " - " + fl.Freight_ItemClass,
                Enabled = fl.Enabled
            }).Where(fl => fl.Enabled == true)
                                        .ToList();
            ViewBag.Freight_Lookup = new SelectList(Freight, "FreightLookup_Id", "Description");
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

        protected bool NeedInsertItemAttributes(ImportedItem impItem)
        {
            if (impItem.CategoryClass_Id < 4)
                return true;
            else
                return false;
        }

        protected void InsertItemAttributes(ImportedItem impItem)
        {
            if (impItem.CategoryClass_Id == 1 || impItem.CategoryClass_Id == 2)
                InsertGunAttributes(impItem);
            else
                InsertAmmoAttributes(impItem);
        }

        protected void InsertGunAttributes(ImportedItem impItem)
        {
            int userId = int.Parse(Session.Contents["UserId"].ToString());
            ItemAttribute at = GetItemAttribute(impItem, userId, impItem.ActionId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.CaliberId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.CapacityId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.FinishId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.BarrelLengthId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.ModelId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            if (impItem.CategoryClass_Id == 2)
            {
                at = GetItemAttribute(impItem, userId, impItem.MiscId.Value);
                db.ItemAttributes.Add(at);
                db.SaveChanges();
            }
        }

        protected ItemAttribute GetItemAttribute(ImportedItem impItem, int userId, int atlId)
        {
            ItemAttribute at = new ItemAttribute();
            at.Item_Id = impItem.Item_id;
            at.AttributeLookup_Id = atlId;
            at.AddedBy = userId;
            at.AddedDate = DateTime.Now;
            return at;
        }

        protected void InsertAmmoAttributes(ImportedItem impItem)
        {
            int userId = int.Parse(Session.Contents["UserId"].ToString());
            ItemAttribute at = GetItemAttribute(impItem, userId, impItem.BullettTypeId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.CaliberId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.CountPerBoxId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.GrainWeightId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.FamilyNameId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.CountPerCaseId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.MiscId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

            at = GetItemAttribute(impItem, userId, impItem.FeetPerSecondId.Value);
            db.ItemAttributes.Add(at);
            db.SaveChanges();

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

        private MultiSelectList ReLoadWarehouseList(List<string> SelectedWH)
        {
            var list = db.WareHouse_Lookup.Select(whl => new
            {
                Id = whl.WareHouse_id,
                Name = whl.WareHouseName
            }).ToList();

            return new MultiSelectList(list, "Id", "Name", SelectedWH);
        }

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

        protected string GetTrueYN(string yn)
        {
            if (yn == "Y")
                return "Y";
            else
                return "N";
        }

        protected List<int> GetWareHouseList(string lst) //Chapin:Pittston:Simmons:OSHI:P1:Newberry on spreadsheet
        {
            List<int> rst = new List<int>();
            if (lst.Substring(0, 1) == "Y")
                rst.Add(1);
            else
                rst.Add(0);
            if (lst.Substring(1, 1) == "Y")
                rst.Add(3);
            else
                rst.Add(0);
            if (lst.Substring(2, 1) == "Y")
                rst.Add(4);
            else
                rst.Add(0);
            if (lst.Substring(3, 1) == "Y")
                rst.Add(5);
            else
                rst.Add(0);
            if (lst.Substring(4, 1) == "Y")
                rst.Add(8);
            else
                rst.Add(0);
            if (lst.Substring(5, 1) == "Y")
                rst.Add(7);
            else
                rst.Add(0);
            return rst;
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

        private List<SelectListItem> LoadCompany(string defaultValue)
        {
            var Company99 = new List<SelectListItem>
                                    {
                                        new SelectListItem {Text = "99", Value  = "99", Selected = (defaultValue == "99")}
                                    };
            return Company99.ToList();
        }

        private List<SelectListItem> LoadYesNo(string defaultValue)
        {
            var YesNo = new List<SelectListItem>
                                    {
                                        new SelectListItem { Text = "Yes", Value = "Y", Selected = (defaultValue == "Y")},
                                        new SelectListItem {Text = "No", Value  = "N", Selected = (defaultValue == "N")}
                                    };
            return YesNo.ToList();
        }

        private List<SelectListItem> LoadHazardous(string defaultValue)
        {
            var Hazardous = new List<SelectListItem>
                                    {
                                        new SelectListItem { Text = "Yes", Value = "Y", Selected = (defaultValue == "Y")},
                                        new SelectListItem {Text = "No", Value  = "N", Selected = (defaultValue == "N")},
                                        new SelectListItem {Text = "CS", Value  = "CS", Selected = (defaultValue == "CS")},
                                        new SelectListItem {Text = "CC", Value = "CC", Selected = (defaultValue == "CC")}
                                    };
            return Hazardous.ToList();
        }

        [HttpPost]
        public ActionResult SaveUpload(HttpPostedFileBase file)
        {
            if (FileIsValid(file))
            {
                if (!FileIsInDatabase(file))
                {
                    string path = SaveUploadFile(file);
                    FileCategory fileCategory = FindFileCategory(path);
                    switch (fileCategory)
                    {
                        case FileCategory.Gun:
                            SaveGuns(file, path);
                            break;
                        case FileCategory.Ammo:
                            SaveAmmos(file, path);
                            break;
                        case FileCategory.Ims:
                            SaveIms(file, path);
                            break;
                        case FileCategory.NFA:
                            SaveNFA(file, path);
                            break;
                        case FileCategory.Marine:
                            SaveMarine(file, path);
                            break;
                    }
                }

            }
            return View();
        }

        protected void SaveAmmos(HttpPostedFileBase file, string path)
        {
            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel._Worksheet worksheet = null;
            Excel.Range range = null;
            Excel._Worksheet worksheet1 = null;
            Excel.Range range1 = null;
            string itemMessage = "";
            int lastIndex = file.FileName.LastIndexOf("\\");
            string fileName = file.FileName.Substring(lastIndex + 1);
            ImportFile f = new ImportFile();
            f.FileName = fileName;
            ImportedItem impItem = new ImportedItem();

            object misValue = System.Reflection.Missing.Value;
            try
            {
                f.UserId = int.Parse(Session.Contents["UserId"].ToString());
                f.LoadDate = DateTime.Now;
                using (IMS_V1Entities db1 = new IMS_V1Entities())
                {
                    app = new Excel.Application();
                    app.DisplayAlerts = false;
                    workbook = app.Workbooks.Open(path);
                    worksheet = workbook.Sheets["OrderItems"];
                    worksheet1 = workbook.Sheets["Vendors"];
                    range = worksheet.UsedRange;
                    f.fileDate = ((Excel.Range)range.Cells[2, 1]).Text;

                    db1.ImportFiles.Add(f);
                    db1.SaveChanges();
                    impItem.FileId = f.File_id;
                    range1 = worksheet1.UsedRange;

                    var UMs = from u in db1.UM_Lookup
                              select u;
                    LineData logoIds; // = new LineData();
                    int i = 5;
                    string buyer = workbook.Sheets["OrderItems"].Buyer.Value;

                    while (((Excel.Range)range1.Cells[i, 9]).Text.Trim().Length > 0)
                    {
                        impItem.Buyer = buyer;
                        logoIds = new LineData();
                        logoIds.line = i;
                        for (int j = 9; j < 25; j++)
                        {
                            logoIds.listData.Add(((Excel.Range)range1.Cells[i, j]).Text);
                        }
                        for (int k = 1; k < 58; k++)
                        {
                            switch (k)
                            {
                                case 1:
                                    impItem.MFG_Number = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 2:
                                    impItem.ManufacturerLogo_Id = Int32.Parse(logoIds.listData.First());
                                    break;
                                case 6:
                                    impItem.CategoryClass_Id = Int32.Parse(logoIds.listData.ElementAt(1));
                                    break;
                                case 7:
                                    impItem.SubClassCode_Id = Int32.Parse(logoIds.listData.ElementAt(2));
                                    break;
                                case 8:
                                    impItem.FineLineCode_Id = Int32.Parse(logoIds.listData.ElementAt(3));
                                    break;
                                case 9:
                                    impItem.Item_Description = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 10:
                                    impItem.UPC = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 11:
                                    string unitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    unitOfMeasure = unitOfMeasure.Trim();
                                    var um = UMs.Where(u => u.UM_Description == unitOfMeasure).FirstOrDefault();
                                    if (um != null)
                                        impItem.UM_Id = um.UM_Id;
                                    else
                                        impItem.UM_Id = 0;
                                    break;
                                case 12:
                                    string vunitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    vunitOfMeasure = vunitOfMeasure.Trim();
                                    var vum = UMs.Where(u => u.UM_Description == vunitOfMeasure).FirstOrDefault();
                                    if (vum != null)
                                        impItem.VdUM_id = vum.UM_Id;
                                    else
                                        impItem.VdUM_id = 0;
                                    break;
                                case 13:
                                    string vsCost = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.VICost = Decimal.Parse(vsCost);
                                    break;
                                case 14:
                                    impItem.WholeSaleMTP = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.WholeSaleMTP = impItem.WholeSaleMTP.Trim();
                                    break;
                                case 15:
                                    string wholeSaleMTPPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wholeSaleMTPPrice.Trim().Length == 0)
                                        impItem.WholeSaleMTPPrice = 0;
                                    else
                                    {
                                        impItem.WholeSaleMTPPrice = Decimal.Parse(wholeSaleMTPPrice);
                                    }
                                    break;
                                case 16:
                                    impItem.MinAdvertisePriceFlag = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 17:
                                    string minAdvPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (minAdvPrice.Trim().Length == 0)
                                        impItem.MinAdvertisePrice = 0;
                                    else
                                    {
                                        impItem.MinAdvertisePrice = Decimal.Parse(minAdvPrice);
                                    }
                                    break;
                                case 18:
                                    string msrpPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.MSRP = Decimal.Parse(msrpPrice);
                                    break;
                                case 20:
                                    string level1Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level1 = Decimal.Parse(level1Price);
                                    break;
                                case 22:
                                    string level2Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level2 = Decimal.Parse(level2Price);
                                    break;
                                case 24:
                                    string level3Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level3 = Decimal.Parse(level3Price);
                                    break;
                                case 26:
                                    string level4Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level4 = Decimal.Parse(level4Price);
                                    break;
                                case 28:
                                    string level5Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level5 = Decimal.Parse(level5Price);
                                    break;
                                case 30:
                                    impItem.Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 31:
                                    string Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (Qty_Break.Trim().Length == 0)
                                        impItem.Qty_BreakPrice = 0;
                                    else
                                    {
                                        impItem.Qty_BreakPrice = Decimal.Parse(Qty_Break);
                                    }
                                    break;
                                case 32:
                                    impItem.CatWebCode_Id = Int32.Parse(logoIds.listData.ElementAt(5));
                                    break;
                                case 33:
                                    impItem.Plan_YN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 34:
                                    impItem.Freight_Id = Int32.Parse(logoIds.listData.ElementAt(6));
                                    break;
                                case 35:
                                    impItem.ABC_Id = Int32.Parse(logoIds.listData.ElementAt(7));
                                    break;
                                case 36:
                                    impItem.STD = Int32.Parse(((Excel.Range)range.Cells[i, k]).Text);
                                    break;
                                case 37:
                                    impItem.MIN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 38:
                                    string wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses = "N";
                                    else
                                        impItem.WareHouses = wareHouse.Trim();
                                    break;
                                case 39:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 40:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 41:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 42:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "NN";
                                    else
                                        impItem.WareHouses += wareHouse.Trim() + "N";
                                    break;
                                case 43:
                                    impItem.Haz = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (impItem.Haz == "Yes")
                                        impItem.Haz = "Y";
                                    else if (impItem.Haz == "No")
                                        impItem.Haz = "N";
                                    break;
                                case 44:
                                    impItem.Exclusive = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 45:
                                    impItem.Allocated = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 46:
                                    impItem.DropShip = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 47:
                                    impItem.PreventFromWeb = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 48:
                                    impItem.SpecialOrder = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 49:
                                    impItem.Company99 = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 50:
                                    impItem.BullettTypeId = Int32.Parse(logoIds.listData.ElementAt(8));
                                    break;
                                case 51:
                                    impItem.CaliberId = Int32.Parse(logoIds.listData.ElementAt(9));
                                    break;
                                case 52:
                                    impItem.CountPerBoxId = Int32.Parse(logoIds.listData.ElementAt(10));
                                    break;
                                case 53:
                                    impItem.GrainWeightId = Int32.Parse(logoIds.listData.ElementAt(11));
                                    break;
                                case 54:
                                    impItem.FamilyNameId = Int32.Parse(logoIds.listData.ElementAt(12));
                                    break;
                                case 55:
                                    impItem.CountPerCaseId = Int32.Parse(logoIds.listData.ElementAt(13));
                                    break;
                                case 56:
                                    impItem.MiscId = Int32.Parse(logoIds.listData.ElementAt(14));
                                    break;
                                case 57:
                                    impItem.FeetPerSecondId = Int32.Parse(logoIds.listData.ElementAt(15));
                                    break;
                            }
                        }
                        bool goodForImport = true;
                        if (MFG_NumberInIMS(db1, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in IMS already for manufacturer " + GetManufacturerName(db1, impItem);
                            goodForImport = false;
                        }
                        if (MFG_NumberInImportedTable(db1, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in imported table already for manufacturer " + GetManufacturerName(db1, impItem);
                            goodForImport = false;
                        }
                        if (UpcInIMS(db1, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in IMS already";
                            goodForImport = false;
                        }
                        if (UpcInImportedTable(db1, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in imported table already";
                            goodForImport = false;
                        }
                        if (goodForImport)
                        {
                            impItem.CreatedBy = int.Parse(Session.Contents["UserId"].ToString());
                            impItem.CreatedDate = DateTime.Now;
                            db1.ImportedItems.Add(impItem);
                            db1.SaveChanges();
                            itemMessage += " Import item " + impItem.MFG_Number + " saved.";
                        }
                        else
                        {
                            var fileToDelete = db1.ImportFiles.Where(fl => fl.File_id == impItem.FileId).FirstOrDefault();
                            db1.ImportFiles.Remove(fileToDelete);
                            db1.SaveChanges();
                        }
                        i++;
                    }
                    worksheet1.Cells[1, 25] = int.Parse(Session.Contents["UserId"].ToString());
                    worksheet1.Cells[1, 26] = DateTime.Now;
                    ViewBag.Message = itemMessage;
                }  //end of using
                workbook.Save();
                workbook.Close(true, misValue, misValue);
                app.Quit();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
                ImportFile fileToDelete = db.ImportFiles.Where(fl => fl.File_id == impItem.FileId).FirstOrDefault();
                if (fileToDelete != null)
                {
                    db.ImportFiles.Remove(fileToDelete);
                    db.SaveChanges();
                }
            }
            finally
            {
                releaseObject(app);
                releaseObject(workbook);
                releaseObject(worksheet);
                releaseObject(range);
                releaseObject(worksheet1);
                releaseObject(range1);
            }
            Process[] excelProcs = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in excelProcs)
            {
                proc.Kill();
            }
        }

        protected void SaveNFA(HttpPostedFileBase file, string path)
        {
            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel._Worksheet worksheet = null;
            Excel.Range range = null;
            Excel._Worksheet worksheet1 = null;
            Excel.Range range1 = null;
            object misValue = System.Reflection.Missing.Value;
            try
            {
                string itemMessage = "";
                int lastIndex = file.FileName.LastIndexOf("\\");
                string fileName = file.FileName.Substring(lastIndex + 1);
                ImportFile f = new ImportFile();
                f.FileName = fileName;
                f.UserId = int.Parse(Session.Contents["UserId"].ToString());
                f.LoadDate = DateTime.Now;
                using (IMS_V1Entities db = new IMS_V1Entities())
                {
                    app = new Excel.Application();
                    app.DisplayAlerts = false;
                    workbook = app.Workbooks.Open(path);
                    worksheet = workbook.Sheets["OrderItems"];
                    worksheet1 = workbook.Sheets["Vendors"];
                    range = worksheet.UsedRange;
                    f.fileDate = ((Excel.Range)range.Cells[2, 1]).Text;

                    db.ImportFiles.Add(f);
                    db.SaveChanges();
                    range1 = worksheet1.UsedRange;

                    var UMs = from u in db.UM_Lookup
                              select u;
                    LineData logoIds; // = new LineData();
                    int i = 5;
                    string buyer = workbook.Sheets["OrderItems"].Buyer.Value;

                    while (((Excel.Range)range1.Cells[i, 9]).Text.Trim().Length > 0)
                    {
                        ImportedItem impItem = new ImportedItem();
                        impItem.FileId = f.File_id;
                        impItem.Buyer = buyer;
                        logoIds = new LineData();
                        logoIds.line = i;
                        for (int j = 9; j < 25; j++)
                        {
                            logoIds.listData.Add(((Excel.Range)range1.Cells[i, j]).Text);
                        }
                        for (int k = 1; k < 56; k++)
                        {
                            switch (k)
                            {
                                case 1:
                                    impItem.MFG_Number = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 2:
                                    impItem.ManufacturerLogo_Id = Int32.Parse(logoIds.listData.First());
                                    break;
                                case 6:
                                    impItem.CategoryClass_Id = Int32.Parse(logoIds.listData.ElementAt(1));
                                    break;
                                case 7:
                                    impItem.SubClassCode_Id = Int32.Parse(logoIds.listData.ElementAt(2));
                                    break;
                                case 8:
                                    impItem.FineLineCode_Id = Int32.Parse(logoIds.listData.ElementAt(3));
                                    break;
                                case 9:
                                    impItem.Item_Description = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 10:
                                    impItem.FFLType_Id = Int32.Parse(logoIds.listData.ElementAt(4));
                                    break;
                                case 11:
                                    if (((Excel.Range)range.Cells[i, k]).Text == "Caliber")
                                    {
                                        impItem.FFLCaliber = ((Excel.Range)range.Cells[i, k + 1]).Text;
                                        impItem.FFLGauge = "Caliber";
                                    }
                                    else
                                    {
                                        string fflGauge = ((Excel.Range)range.Cells[i, k + 1]).Text;
                                        impItem.FFLGauge = "Gauge";
                                        if (fflGauge.Length < 50)
                                            impItem.FFLCaliber = fflGauge;
                                        else
                                            impItem.FFLCaliber = fflGauge.Substring(0, 50);
                                    }
                                    break;
                                case 13:
                                    impItem.FFLModel = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 14:
                                    impItem.FFLMFGName = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 15:
                                    impItem.FFLMFGImportName = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 16:
                                    impItem.UPC = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;

                                case 17:
                                    string unitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    unitOfMeasure = unitOfMeasure.Trim();
                                    var um = UMs.Where(u => u.UM_Description == unitOfMeasure).FirstOrDefault();
                                    if (um != null)
                                        impItem.UM_Id = um.UM_Id;
                                    else
                                        impItem.UM_Id = 0;
                                    break;
                                case 18:
                                    string vunitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    vunitOfMeasure = vunitOfMeasure.Trim();
                                    var vum = UMs.Where(u => u.UM_Description == vunitOfMeasure).FirstOrDefault();
                                    if (vum != null)
                                        impItem.VdUM_id = vum.UM_Id;
                                    else
                                        impItem.VdUM_id = 0;
                                    break;
                                case 19:
                                    string vsCost = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.VICost = Decimal.Parse(vsCost);
                                    break;
                                case 20:
                                    impItem.WholeSaleMTP = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.WholeSaleMTP = impItem.WholeSaleMTP.Trim();
                                    break;
                                case 21:
                                    string wholeSaleMTPPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wholeSaleMTPPrice.Trim().Length == 0)
                                        impItem.WholeSaleMTPPrice = 0;
                                    else
                                    {
                                        impItem.WholeSaleMTPPrice = Decimal.Parse(wholeSaleMTPPrice);
                                    }
                                    break;
                                case 22:
                                    impItem.MinAdvertisePriceFlag = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 23:
                                    string minAdvPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (minAdvPrice.Trim().Length == 0)
                                        impItem.MinAdvertisePrice = 0;
                                    else
                                    {
                                        impItem.MinAdvertisePrice = Decimal.Parse(minAdvPrice);
                                    }
                                    break;
                                case 24:
                                    string msrpPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.MSRP = Decimal.Parse(msrpPrice);
                                    break;
                                case 26:
                                    string level1Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level1 = Decimal.Parse(level1Price);
                                    break;
                                case 28:
                                    string level2Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level2 = Decimal.Parse(level2Price);
                                    break;
                                case 30:
                                    string level3Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level3 = Decimal.Parse(level3Price);
                                    break;
                                case 32:
                                    string level4Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level4 = Decimal.Parse(level4Price);
                                    break;
                                case 34:
                                    string level5Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level5 = Decimal.Parse(level5Price);
                                    break;
                                case 36:
                                    impItem.Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 37:
                                    string Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (Qty_Break.Trim().Length == 0)
                                        impItem.Qty_BreakPrice = 0;
                                    else
                                    {
                                        impItem.Qty_BreakPrice = Decimal.Parse(Qty_Break);
                                    }
                                    break;
                                case 38:
                                    impItem.CatWebCode_Id = Int32.Parse(logoIds.listData.ElementAt(5));
                                    break;
                                case 39:
                                    impItem.Plan_YN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 40:
                                    impItem.Freight_Id = Int32.Parse(logoIds.listData.ElementAt(6));
                                    break;
                                case 41:
                                    impItem.ABC_Id = Int32.Parse(logoIds.listData.ElementAt(7));
                                    break;
                                case 42:
                                    impItem.STD = Int32.Parse(((Excel.Range)range.Cells[i, k]).Text);
                                    break;
                                case 43:
                                    impItem.MIN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 44:
                                    string wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses = "N";
                                    else
                                        impItem.WareHouses = wareHouse.Trim();
                                    break;
                                case 45:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 46:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 47:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 48:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    impItem.WareHouses += "N"; //for NewBerry
                                    break;
                                case 49:
                                    impItem.Haz = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (impItem.Haz == "Yes")
                                        impItem.Haz = "Y";
                                    else if (impItem.Haz == "No")
                                        impItem.Haz = "N";
                                    break;
                                case 50:
                                    impItem.Exclusive = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 51:
                                    impItem.Allocated = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 52:
                                    impItem.DropShip = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 53:
                                    impItem.PreventFromWeb = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 54:
                                    impItem.SpecialOrder = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 55:
                                    impItem.Company99 = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                            }
                        }
                        bool goodForImport = true;
                        if (MFG_NumberInIMS(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in IMS already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (MFG_NumberInImportedTable(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in imported table already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (UpcInIMS(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in IMS already";
                            goodForImport = false;
                        }
                        if (UpcInImportedTable(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in imported table already";
                            goodForImport = false;
                        }
                        if (goodForImport)
                        {
                            impItem.CreatedBy = int.Parse(Session.Contents["UserId"].ToString());
                            impItem.CreatedDate = DateTime.Now;
                            db.ImportedItems.Add(impItem);
                            db.SaveChanges();
                            itemMessage += " Import item " + impItem.MFG_Number + " saved.";
                        }
                        else
                        {
                            var fileToDelete = db.ImportFiles.Where(fl => fl.File_id == impItem.FileId).FirstOrDefault();
                            db.ImportFiles.Remove(fileToDelete);
                            db.SaveChanges();
                        }
                        i++;
                    }
                    worksheet1.Cells[1, 25] = int.Parse(Session.Contents["UserId"].ToString());
                    worksheet1.Cells[1, 26] = DateTime.Now;
                    ViewBag.Message = itemMessage;
                }
                workbook.Save();
                workbook.Close(true, misValue, misValue);
                app.Quit();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }
            finally
            {
                releaseObject(app);
                releaseObject(workbook);
                releaseObject(worksheet);
                releaseObject(range);
                releaseObject(worksheet1);
                releaseObject(range1);
            }
            Process[] excelProcs = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in excelProcs)
            {
                proc.Kill();
            }
        }
        protected void SaveIms(HttpPostedFileBase file, string path)
        {
            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel._Worksheet worksheet = null;
            Excel.Range range = null;
            Excel._Worksheet worksheet1 = null;
            Excel.Range range1 = null;
            object misValue = System.Reflection.Missing.Value;
            try
            {
                string itemMessage = "";
                int lastIndex = file.FileName.LastIndexOf("\\");
                string fileName = file.FileName.Substring(lastIndex + 1);
                ImportFile f = new ImportFile();
                f.FileName = fileName;
                f.UserId = int.Parse(Session.Contents["UserId"].ToString());
                f.LoadDate = DateTime.Now;
                using (IMS_V1Entities db = new IMS_V1Entities())
                {
                    app = new Excel.Application();
                    app.DisplayAlerts = false;
                    workbook = app.Workbooks.Open(path);
                    worksheet = workbook.Sheets["OrderItems"];
                    worksheet1 = workbook.Sheets["Vendors"];
                    range = worksheet.UsedRange;
                    f.fileDate = ((Excel.Range)range.Cells[2, 1]).Text;

                    db.ImportFiles.Add(f);
                    db.SaveChanges();
                    range1 = worksheet1.UsedRange;

                    var UMs = from u in db.UM_Lookup
                              select u;
                    LineData logoIds; // = new LineData();
                    int i = 5;
                    string buyer = workbook.Sheets["OrderItems"].Buyer.Value;

                    while (((Excel.Range)range1.Cells[i, 9]).Text.Trim().Length > 0)
                    {
                        ImportedItem impItem = new ImportedItem();
                        impItem.FileId = f.File_id;
                        impItem.Buyer = buyer;
                        logoIds = new LineData();
                        logoIds.line = i;
                        for (int j = 9; j < 25; j++)
                        {
                            logoIds.listData.Add(((Excel.Range)range1.Cells[i, j]).Text);
                        }
                        for (int k = 1; k < 51; k++)
                        {
                            switch (k)
                            {
                                case 1:
                                    impItem.MFG_Number = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 2:
                                    impItem.ManufacturerLogo_Id = Int32.Parse(logoIds.listData.First());
                                    break;
                                case 6:
                                    impItem.CategoryClass_Id = Int32.Parse(logoIds.listData.ElementAt(1));
                                    break;
                                case 7:
                                    impItem.SubClassCode_Id = Int32.Parse(logoIds.listData.ElementAt(2));
                                    break;
                                case 8:
                                    impItem.FineLineCode_Id = Int32.Parse(logoIds.listData.ElementAt(3));
                                    break;
                                case 9:
                                    impItem.Item_Description = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 10:
                                    impItem.UPC = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 11:
                                    string unitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    unitOfMeasure = unitOfMeasure.Trim();
                                    var um = UMs.Where(u => u.UM_Description == unitOfMeasure).FirstOrDefault();
                                    if (um != null)
                                        impItem.UM_Id = um.UM_Id;
                                    else
                                        impItem.UM_Id = 0;
                                    break;
                                case 12:
                                    string vunitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    vunitOfMeasure = vunitOfMeasure.Trim();
                                    var vum = UMs.Where(u => u.UM_Description == vunitOfMeasure).FirstOrDefault();
                                    if (vum != null)
                                        impItem.VdUM_id = vum.UM_Id;
                                    else
                                        impItem.VdUM_id = 0;
                                    break;
                                case 13:
                                    string vsCost = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.VICost = Decimal.Parse(vsCost);
                                    break;
                                case 14:
                                    impItem.WholeSaleMTP = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.WholeSaleMTP = impItem.WholeSaleMTP.Trim();
                                    break;
                                case 15:
                                    string wholeSaleMTPPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wholeSaleMTPPrice.Trim().Length == 0)
                                        impItem.WholeSaleMTPPrice = 0;
                                    else
                                    {
                                        impItem.WholeSaleMTPPrice = Decimal.Parse(wholeSaleMTPPrice);
                                    }
                                    break;
                                case 16:
                                    impItem.MinAdvertisePriceFlag = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 17:
                                    string minAdvPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (minAdvPrice.Trim().Length == 0)
                                        impItem.MinAdvertisePrice = 0;
                                    else
                                    {
                                        impItem.MinAdvertisePrice = Decimal.Parse(minAdvPrice);
                                    }
                                    break;
                                case 18:
                                    string msrpPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.MSRP = Decimal.Parse(msrpPrice);
                                    break;
                                case 20:
                                    string level1Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level1 = Decimal.Parse(level1Price);
                                    break;
                                case 22:
                                    string level2Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level2 = Decimal.Parse(level2Price);
                                    break;
                                case 24:
                                    string level3Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level3 = Decimal.Parse(level3Price);
                                    break;
                                case 26:
                                    string level4Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level4 = Decimal.Parse(level4Price);
                                    break;
                                case 28:
                                    string level5Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level5 = Decimal.Parse(level5Price);
                                    break;
                                case 30:
                                    impItem.Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 31:
                                    string Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (Qty_Break.Trim().Length == 0)
                                        impItem.Qty_BreakPrice = 0;
                                    else
                                    {
                                        impItem.Qty_BreakPrice = Decimal.Parse(Qty_Break);
                                    }
                                    break;
                                case 32:
                                    impItem.CatWebCode_Id = Int32.Parse(logoIds.listData.ElementAt(5));
                                    break;
                                case 33:
                                    impItem.Plan_YN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 34:
                                    impItem.Freight_Id = Int32.Parse(logoIds.listData.ElementAt(6));
                                    break;
                                case 35:
                                    impItem.ABC_Id = Int32.Parse(logoIds.listData.ElementAt(7));
                                    break;
                                case 36:
                                    impItem.STD = Int32.Parse(((Excel.Range)range.Cells[i, k]).Text);
                                    break;
                                case 37:
                                    impItem.MIN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 38:
                                    string wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses = "N";
                                    else
                                        impItem.WareHouses = wareHouse.Trim();
                                    break;
                                case 39:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 40:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 41:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 42:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 43:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 44:
                                    impItem.Haz = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (impItem.Haz == "Yes")
                                        impItem.Haz = "Y";
                                    else if (impItem.Haz == "No")
                                        impItem.Haz = "N";
                                    break;
                                case 45:
                                    impItem.Exclusive = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 46:
                                    impItem.Allocated = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 47:
                                    impItem.DropShip = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 48:
                                    impItem.PreventFromWeb = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 49:
                                    impItem.SpecialOrder = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 50:
                                    impItem.Company99 = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                            }
                        }
                        bool goodForImport = true;
                        if (MFG_NumberInIMS(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in IMS already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (MFG_NumberInImportedTable(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in imported table already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (UpcInIMS(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in IMS already";
                            goodForImport = false;
                        }
                        if (UpcInImportedTable(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in imported table already";
                            goodForImport = false;
                        }
                        if (goodForImport)
                        {
                            impItem.CreatedBy = int.Parse(Session.Contents["UserId"].ToString());
                            impItem.CreatedDate = DateTime.Now;
                            db.ImportedItems.Add(impItem);
                            db.SaveChanges();
                            itemMessage += " Import item " + impItem.MFG_Number + " saved.";
                        }
                        else
                        {
                            var fileToDelete = db.ImportFiles.Where(fl => fl.File_id == impItem.FileId).FirstOrDefault();
                            db.ImportFiles.Remove(fileToDelete);
                            db.SaveChanges();
                        }
                        i++;
                    }
                    worksheet1.Cells[1, 25] = int.Parse(Session.Contents["UserId"].ToString());
                    worksheet1.Cells[1, 26] = DateTime.Now;
                    ViewBag.Message = itemMessage;
                }
                workbook.Save();
                workbook.Close(true, misValue, misValue);
                app.Quit();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }
            finally
            {
                releaseObject(app);
                releaseObject(workbook);
                releaseObject(worksheet);
                releaseObject(range);
                releaseObject(worksheet1);
                releaseObject(range1);
            }
            Process[] excelProcs = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in excelProcs)
            {
                proc.Kill();
            }
        }

        protected void SaveMarine(HttpPostedFileBase file, string path)
        {
            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel._Worksheet worksheet = null;
            Excel.Range range = null;
            Excel._Worksheet worksheet1 = null;
            Excel.Range range1 = null;
            object misValue = System.Reflection.Missing.Value;
            try
            {
                string itemMessage = "";
                int lastIndex = file.FileName.LastIndexOf("\\");
                string fileName = file.FileName.Substring(lastIndex + 1);
                ImportFile f = new ImportFile();
                f.FileName = fileName;
                f.UserId = int.Parse(Session.Contents["UserId"].ToString());
                f.LoadDate = DateTime.Now;
                using (IMS_V1Entities db = new IMS_V1Entities())
                {
                    app = new Excel.Application();
                    app.DisplayAlerts = false;
                    workbook = app.Workbooks.Open(path);
                    worksheet = workbook.Sheets["OrderItems"];
                    worksheet1 = workbook.Sheets["Vendors"];
                    range = worksheet.UsedRange;
                    f.fileDate = ((Excel.Range)range.Cells[2, 1]).Text;

                    db.ImportFiles.Add(f);
                    db.SaveChanges();
                    range1 = worksheet1.UsedRange;

                    var UMs = from u in db.UM_Lookup
                              select u;
                    LineData logoIds; // = new LineData();
                    int i = 5;
                    string buyer = workbook.Sheets["OrderItems"].Buyer.Value;

                    while (((Excel.Range)range1.Cells[i, 9]).Text.Trim().Length > 0)
                    {
                        ImportedItem impItem = new ImportedItem();
                        impItem.FileId = f.File_id;
                        impItem.Buyer = buyer;
                        logoIds = new LineData();
                        logoIds.line = i;
                        for (int j = 9; j < 25; j++)
                        {
                            logoIds.listData.Add(((Excel.Range)range1.Cells[i, j]).Text);
                        }
                        for (int k = 1; k < 51; k++) //long gun 61, hand gun 62, ammo 63
                        {
                            switch (k)
                            {
                                case 1:
                                    impItem.MFG_Number = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 2:
                                    impItem.ManufacturerLogo_Id = Int32.Parse(logoIds.listData.First());
                                    break;
                                case 6:
                                    impItem.CategoryClass_Id = Int32.Parse(logoIds.listData.ElementAt(1));
                                    break;
                                case 7:
                                    impItem.SubClassCode_Id = Int32.Parse(logoIds.listData.ElementAt(2));
                                    break;
                                case 8:
                                    impItem.FineLineCode_Id = Int32.Parse(logoIds.listData.ElementAt(3));
                                    break;
                                case 9:
                                    impItem.Item_Description = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 10:
                                    impItem.UPC = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 11:
                                    string unitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    unitOfMeasure = unitOfMeasure.Trim();
                                    var um = UMs.Where(u => u.UM_Description == unitOfMeasure).FirstOrDefault();
                                    if (um != null)
                                        impItem.UM_Id = um.UM_Id;
                                    else
                                        impItem.UM_Id = 0;
                                    break;
                                case 12:
                                    string vunitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    vunitOfMeasure = vunitOfMeasure.Trim();
                                    var vum = UMs.Where(u => u.UM_Description == vunitOfMeasure).FirstOrDefault();
                                    if (vum != null)
                                        impItem.VdUM_id = vum.UM_Id;
                                    else
                                        impItem.VdUM_id = 0;
                                    break;
                                case 13:
                                    string vsCost = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.VICost = Decimal.Parse(vsCost);
                                    break;
                                case 14:
                                    impItem.WholeSaleMTP = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.WholeSaleMTP = impItem.WholeSaleMTP.Trim();
                                    break;
                                case 15:
                                    string wholeSaleMTPPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wholeSaleMTPPrice.Trim().Length == 0)
                                        impItem.WholeSaleMTPPrice = 0;
                                    else
                                    {
                                        impItem.WholeSaleMTPPrice = Decimal.Parse(wholeSaleMTPPrice);
                                    }
                                    break;
                                case 16:
                                    impItem.MinAdvertisePriceFlag = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 17:
                                    string minAdvPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (minAdvPrice.Trim().Length == 0)
                                        impItem.MinAdvertisePrice = 0;
                                    else
                                    {
                                        impItem.MinAdvertisePrice = Decimal.Parse(minAdvPrice);
                                    }
                                    break;
                                case 18:
                                    string msrpPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.MSRP = Decimal.Parse(msrpPrice);
                                    break;
                                case 20:
                                    string level1Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level1 = Decimal.Parse(level1Price);
                                    break;
                                case 22:
                                    string level2Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level2 = Decimal.Parse(level2Price);
                                    break;
                                case 24:
                                    string level3Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level3 = Decimal.Parse(level3Price);
                                    break;
                                case 26:
                                    string level4Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level4 = Decimal.Parse(level4Price);
                                    break;
                                case 28:
                                    string level5Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level5 = Decimal.Parse(level5Price);
                                    break;
                                case 30:
                                    impItem.Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 31:
                                    string Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (Qty_Break.Trim().Length == 0)
                                        impItem.Qty_BreakPrice = 0;
                                    else
                                    {
                                        impItem.Qty_BreakPrice = Decimal.Parse(Qty_Break);
                                    }
                                    break;
                                case 32:
                                    impItem.CatWebCode_Id = Int32.Parse(logoIds.listData.ElementAt(5));
                                    break;
                                case 33:
                                    impItem.Plan_YN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 34:
                                    impItem.Freight_Id = Int32.Parse(logoIds.listData.ElementAt(6));
                                    break;
                                case 35:
                                    impItem.ABC_Id = Int32.Parse(logoIds.listData.ElementAt(7));
                                    break;
                                case 36:
                                    impItem.STD = Int32.Parse(((Excel.Range)range.Cells[i, k]).Text);
                                    break;
                                case 37:
                                    impItem.MIN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 38:
                                    string wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses = "N";
                                    else
                                        impItem.WareHouses = wareHouse.Trim();
                                    break;
                                case 39:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 40:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 41:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 42:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "NN";
                                    else
                                        impItem.WareHouses += "N" + wareHouse.Trim(); //P1 warehouse is not showing
                                    break;
                                case 43:
                                    impItem.Haz = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (impItem.Haz == "Yes")
                                        impItem.Haz = "Y";
                                    else if (impItem.Haz == "No")
                                        impItem.Haz = "N";
                                    break;
                                case 44:
                                    impItem.Exclusive = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 45:
                                    impItem.Allocated = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 46:
                                    impItem.DropShip = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 47:
                                    impItem.PreventFromWeb = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 48:
                                    impItem.SpecialOrder = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 49:
                                    impItem.Company99 = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                            }
                        }
                        bool goodForImport = true;
                        if (MFG_NumberInIMS(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in IMS already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (MFG_NumberInImportedTable(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in imported table already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (UpcInIMS(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in IMS already";
                            goodForImport = false;
                        }
                        if (UpcInImportedTable(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in imported table already";
                            goodForImport = false;
                        }
                        if (goodForImport)
                        {
                            impItem.CreatedBy = int.Parse(Session.Contents["UserId"].ToString());
                            impItem.CreatedDate = DateTime.Now;
                            db.ImportedItems.Add(impItem);
                            db.SaveChanges();
                            itemMessage += " Import item " + impItem.MFG_Number + " saved.";
                        }
                        else
                        {
                            var fileToDelete = db.ImportFiles.Where(fl => fl.File_id == impItem.FileId).FirstOrDefault();
                            db.ImportFiles.Remove(fileToDelete);
                            db.SaveChanges();
                        }
                        i++;
                    }
                    worksheet1.Cells[1, 25] = int.Parse(Session.Contents["UserId"].ToString());
                    worksheet1.Cells[1, 26] = DateTime.Now;
                    ViewBag.Message = itemMessage;
                }
                workbook.Save();
                workbook.Close(true, misValue, misValue);
                app.Quit();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }
            finally
            {
                releaseObject(app);
                releaseObject(workbook);
                releaseObject(worksheet);
                releaseObject(range);
                releaseObject(worksheet1);
                releaseObject(range1);
            }
            Process[] excelProcs = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in excelProcs)
            {
                proc.Kill();
            }
        }

        protected void SaveGuns(HttpPostedFileBase file, string path)
        {
            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel._Worksheet worksheet = null;
            Excel.Range range = null;
            Excel._Worksheet worksheet1 = null;
            Excel.Range range1 = null;
            object misValue = System.Reflection.Missing.Value;
            try
            {
                string itemMessage = "";
                int lastIndex = file.FileName.LastIndexOf("\\");
                string fileName = file.FileName.Substring(lastIndex + 1);
                ImportFile f = new ImportFile();
                f.FileName = fileName;
                f.UserId = int.Parse(Session.Contents["UserId"].ToString());
                f.LoadDate = DateTime.Now;
                using (IMS_V1Entities db = new IMS_V1Entities())
                {
                    app = new Excel.Application();
                    app.DisplayAlerts = false;
                    workbook = app.Workbooks.Open(path);
                    worksheet = workbook.Sheets["OrderItems"];
                    worksheet1 = workbook.Sheets["Vendors"];
                    range = worksheet.UsedRange;
                    f.fileDate = ((Excel.Range)range.Cells[2, 1]).Text;

                    db.ImportFiles.Add(f);
                    db.SaveChanges();
                    range1 = worksheet1.UsedRange;

                    var UMs = from u in db.UM_Lookup
                              select u;
                    LineData logoIds; // = new LineData();
                    int i = 5;
                    string buyer = workbook.Sheets["OrderItems"].Buyer.Value;

                    while (((Excel.Range)range1.Cells[i, 9]).Text.Trim().Length > 0)
                    {
                        ImportedItem impItem = new ImportedItem();
                        impItem.FileId = f.File_id;
                        impItem.Buyer = buyer;
                        logoIds = new LineData();
                        logoIds.line = i;
                        for (int j = 9; j < 25; j++)
                        {
                            logoIds.listData.Add(((Excel.Range)range1.Cells[i, j]).Text);
                        }
                        for (int k = 1; k < 63; k++) //long gun 61, hand gun 62, ammo 63
                        {
                            switch (k)
                            {
                                case 1:
                                    impItem.MFG_Number = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 2:
                                    impItem.ManufacturerLogo_Id = Int32.Parse(logoIds.listData.First());
                                    break;
                                case 6:
                                    impItem.CategoryClass_Id = Int32.Parse(logoIds.listData.ElementAt(1));
                                    break;
                                case 7:
                                    impItem.SubClassCode_Id = Int32.Parse(logoIds.listData.ElementAt(2));
                                    break;
                                case 8:
                                    impItem.FineLineCode_Id = Int32.Parse(logoIds.listData.ElementAt(3));
                                    break;
                                case 9:
                                    impItem.Item_Description = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 10:
                                    impItem.FFLType_Id = Int32.Parse(logoIds.listData.ElementAt(4));
                                    break;
                                case 11:
                                    if (((Excel.Range)range.Cells[i, k]).Text == "Caliber")
                                    {
                                        impItem.FFLCaliber = ((Excel.Range)range.Cells[i, k + 1]).Text;
                                        impItem.FFLGauge = "Caliber";
                                    }
                                    else
                                    {
                                        string fflGauge = ((Excel.Range)range.Cells[i, k + 1]).Text;
                                        impItem.FFLGauge = "Gauge";
                                        if (fflGauge.Length < 50)
                                            impItem.FFLCaliber = fflGauge;
                                        else
                                            impItem.FFLCaliber = fflGauge.Substring(0, 50);
                                    }
                                    break;

                                case 13:
                                    impItem.FFLModel = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 14:
                                    impItem.FFLMFGName = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 15:
                                    impItem.FFLMFGImportName = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 16:
                                    impItem.UPC = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;

                                case 17:
                                    string unitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    unitOfMeasure = unitOfMeasure.Trim();
                                    var um = UMs.Where(u => u.UM_Description == unitOfMeasure).FirstOrDefault();
                                    if (um != null)
                                        impItem.UM_Id = um.UM_Id;
                                    else
                                        impItem.UM_Id = 0;

                                    break;
                                case 18:
                                    string vunitOfMeasure = ((Excel.Range)range.Cells[i, k]).Text;
                                    vunitOfMeasure = vunitOfMeasure.Trim();
                                    var vum = UMs.Where(u => u.UM_Description == vunitOfMeasure).FirstOrDefault();
                                    if (vum != null)
                                        impItem.VdUM_id = vum.UM_Id;
                                    else
                                        impItem.VdUM_id = 0;
                                    break;
                                case 19:
                                    string vsCost = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.VICost = Decimal.Parse(vsCost);
                                    break;
                                case 20:
                                    impItem.WholeSaleMTP = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.WholeSaleMTP = impItem.WholeSaleMTP.Trim();
                                    break;
                                case 21:
                                    string wholeSaleMTPPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wholeSaleMTPPrice.Trim().Length == 0)
                                        impItem.WholeSaleMTPPrice = 0;
                                    else
                                    {
                                        impItem.WholeSaleMTPPrice = Decimal.Parse(wholeSaleMTPPrice);
                                    }
                                    break;
                                case 22:
                                    impItem.MinAdvertisePriceFlag = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 23:
                                    string minAdvPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (minAdvPrice.Trim().Length == 0)
                                        impItem.MinAdvertisePrice = 0;
                                    else
                                    {
                                        impItem.MinAdvertisePrice = Decimal.Parse(minAdvPrice);
                                    }
                                    break;
                                case 24:
                                    string msrpPrice = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.MSRP = Decimal.Parse(msrpPrice);
                                    break;
                                case 26:
                                    string level1Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level1 = Decimal.Parse(level1Price);
                                    break;
                                case 28:
                                    string level2Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level2 = Decimal.Parse(level2Price);
                                    break;
                                case 30:
                                    string level3Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level3 = Decimal.Parse(level3Price);
                                    break;
                                case 32:
                                    string level4Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level4 = Decimal.Parse(level4Price);
                                    break;
                                case 34:
                                    string level5Price = ((Excel.Range)range.Cells[i, k]).Text;
                                    impItem.Level5 = Decimal.Parse(level5Price);
                                    break;
                                case 36:
                                    impItem.Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 37:
                                    string Qty_Break = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (Qty_Break.Trim().Length == 0)
                                        impItem.Qty_BreakPrice = 0;
                                    else
                                    {
                                        impItem.Qty_BreakPrice = Decimal.Parse(Qty_Break);
                                    }
                                    break;
                                case 38:
                                    impItem.CatWebCode_Id = Int32.Parse(logoIds.listData.ElementAt(5));
                                    break;
                                case 39:
                                    impItem.Plan_YN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 40:
                                    impItem.Freight_Id = Int32.Parse(logoIds.listData.ElementAt(6));
                                    break;
                                case 41:
                                    impItem.ABC_Id = Int32.Parse(logoIds.listData.ElementAt(7));
                                    break;
                                case 42:
                                    impItem.STD = Int32.Parse(((Excel.Range)range.Cells[i, k]).Text);
                                    break;
                                case 43:
                                    impItem.MIN = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 44:
                                    string wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses = "N";
                                    else
                                        impItem.WareHouses = wareHouse.Trim();
                                    break;
                                case 45:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 46:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 47:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    break;
                                case 48:
                                    wareHouse = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (wareHouse.Trim().Length == 0)
                                        impItem.WareHouses += "N";
                                    else
                                        impItem.WareHouses += wareHouse.Trim();
                                    impItem.WareHouses += "N";  //This is for marine warehouse 02-Newberry
                                    break;
                                case 49:
                                    impItem.Haz = ((Excel.Range)range.Cells[i, k]).Text;
                                    if (impItem.Haz == "Yes")
                                        impItem.Haz = "Y";
                                    else if (impItem.Haz == "No")
                                        impItem.Haz = "N";
                                    break;
                                case 50:
                                    impItem.Exclusive = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 51:
                                    impItem.Allocated = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 52:
                                    impItem.DropShip = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 53:
                                    impItem.PreventFromWeb = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 54:
                                    impItem.SpecialOrder = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 55:
                                    impItem.Company99 = ((Excel.Range)range.Cells[i, k]).Text;
                                    break;
                                case 56:
                                    impItem.ActionId = Int32.Parse(logoIds.listData.ElementAt(8));
                                    break;
                                case 57:
                                    impItem.CaliberId = Int32.Parse(logoIds.listData.ElementAt(9));
                                    break;
                                case 58:
                                    impItem.CapacityId = Int32.Parse(logoIds.listData.ElementAt(10));
                                    break;
                                case 59:
                                    impItem.FinishId = Int32.Parse(logoIds.listData.ElementAt(11));
                                    break;
                                case 60:
                                    impItem.BarrelLengthId = Int32.Parse(logoIds.listData.ElementAt(12));
                                    break;
                                case 61:
                                    impItem.ModelId = Int32.Parse(logoIds.listData.ElementAt(13));
                                    break;
                                case 62:
                                    if (logoIds.listData.ElementAt(14).Trim().Length > 0)
                                        impItem.MiscId = Int32.Parse(logoIds.listData.ElementAt(14));
                                    break;
                            }
                        }

                        bool goodForImport = true;
                        if (MFG_NumberInIMS(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in IMS already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (MFG_NumberInImportedTable(db, impItem))
                        {
                            itemMessage += " Manufacturer number " + impItem.MFG_Number + " is in imported table already for manufacturer " + GetManufacturerName(db, impItem);
                            goodForImport = false;
                        }
                        if (UpcInIMS(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in IMS already";
                            goodForImport = false;
                        }
                        if (UpcInImportedTable(db, impItem))
                        {
                            itemMessage += " UPC " + impItem.UPC + " is in imported table already";
                            goodForImport = false;
                        }
                        if (goodForImport)
                        {
                            impItem.CreatedBy = int.Parse(Session.Contents["UserId"].ToString());
                            impItem.CreatedDate = DateTime.Now;
                            db.ImportedItems.Add(impItem);
                            db.SaveChanges();
                            itemMessage += " Import item " + impItem.MFG_Number + " saved.";
                        }
                        else
                        {
                            var fileToDelete = db.ImportFiles.Where(fl => fl.File_id == impItem.FileId).FirstOrDefault();
                            db.ImportFiles.Remove(fileToDelete);
                            db.SaveChanges();
                        }
                        i++;
                    }
                    worksheet1.Cells[1, 25] = int.Parse(Session.Contents["UserId"].ToString());
                    worksheet1.Cells[1, 26] = DateTime.Now;
                    ViewBag.Message = itemMessage;
                }
                workbook.Save();
                workbook.Close(true, misValue, misValue);
                app.Quit();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }
            finally
            {
                releaseObject(app);
                releaseObject(workbook);
                releaseObject(worksheet);
                releaseObject(range);
                releaseObject(worksheet1);
                releaseObject(range1);
            }
            Process[] excelProcs = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in excelProcs)
            {
                proc.Kill();
            }
        }

        protected string GetManufacturerName(IMS_V1Entities db, ImportedItem impItem)
        {
            var m = db.zManufacturersLogoes.Where(z => z.ManufacturerLogo_Id == impItem.ManufacturerLogo_Id).FirstOrDefault();
            if (m != null)
                return m.WebVendorName;
            else
                return "";
        }

        protected bool MFG_NumberInIMS(IMS_V1Entities db, ImportedItem impItem)
        {
            bool nInIms = false;
            var DupCheck = (from i in db.Items
                            where i.MFG_Number == impItem.MFG_Number && i.ManufacturerLogo_Id == impItem.ManufacturerLogo_Id
                            select new { i.Itm_Num, i.Item_Description }).FirstOrDefault();
            if (DupCheck == null)
            {
                var vendor = db.zManufacturersLogoes.Where(v => v.ManufacturerLogo_Id == impItem.ManufacturerLogo_Id).FirstOrDefault();
                string vendorNum = "";
                if (vendor != null)
                    vendorNum = vendor.VendorNumber;
                var DupCheck2 = (from i in db.APlusItems
                                 where i.MFG_Number == impItem.MFG_Number && i.VendorNumber == vendorNum
                                 select new { i.Itm_Num, i.Item_Desc1, i.Item_Desc2 }).FirstOrDefault();
                if (DupCheck2 != null)
                    nInIms = true;
            }
            else
                nInIms = true;
            return nInIms;
        }

        protected bool MFG_NumberInImportedTable(IMS_V1Entities db, ImportedItem impItem)
        {
            bool nInIms = false;
            var DupCheck = (from i in db.ImportedItems
                            where i.MFG_Number == impItem.MFG_Number && i.ManufacturerLogo_Id == impItem.ManufacturerLogo_Id
                            select new { i.Item_Description }).FirstOrDefault();
            if (DupCheck != null)
                nInIms = true;
            return nInIms;
        }

        protected bool UpcInIMS(IMS_V1Entities db, ImportedItem impItem)
        {
            bool upcInIms = false;
            var DupCheck = (from i in db.Items
                            where i.UPC == impItem.UPC
                            select new { i.Itm_Num, i.Item_Description }).FirstOrDefault();
            if (DupCheck == null)
            {
                var EDIUPCCheck2 = (from i in db.APlusItems
                                    where i.UPC == impItem.UPC
                                    select new { i.Itm_Num, i.Item_Desc1, i.Item_Desc2 }).FirstOrDefault();
                if (EDIUPCCheck2 != null)
                    upcInIms = true;
            }
            else
                upcInIms = true;
            return upcInIms;
        }

        protected bool UpcInImportedTable(IMS_V1Entities db, ImportedItem impItem)
        {
            bool upcInImportedTable = false;
            var DupCheck = (from i in db.ImportedItems
                            where i.UPC == impItem.UPC
                            select new { i.Item_Description }).FirstOrDefault();
            if (DupCheck != null)
                upcInImportedTable = true;
            return upcInImportedTable;
        }
        protected FileCategory FindFileCategory(string path)
        {
            FileCategory category = FileCategory.NotDefined;
            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel._Worksheet worksheet = null;
            Excel.Range range = null;

            object misValue = System.Reflection.Missing.Value;
            try
            {
                app = new Excel.Application();
                app.DisplayAlerts = false;
                workbook = app.Workbooks.Open(path);
                worksheet = workbook.Sheets["OrderItems"];
                range = worksheet.UsedRange;
                string catId = ((Excel.Range)range.Cells[5, 6]).Text;
                int icatId = Int16.Parse(catId.Substring(0, 2));
                switch (icatId)
                {
                    case 1:
                        category = FileCategory.Gun;
                        break;
                    case 2:
                        category = FileCategory.Gun;
                        break;
                    case 3:
                        category = FileCategory.Ammo;
                        break;
                    default:
                        if (icatId == 32)
                            category = FileCategory.NFA;
                        else if (icatId > 3 && icatId < 50)
                            category = FileCategory.Ims;
                        else
                            category = FileCategory.Marine;
                        break;
                }
                workbook.Close(true, misValue, misValue);
                app.Quit();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }
            finally
            {
                releaseObject(app);
                releaseObject(workbook);
                releaseObject(worksheet);
                releaseObject(range);
            }
            Process[] excelProcs = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in excelProcs)
            {
                proc.Kill();
            }
            return category;
        }

        private string SaveUploadFile(HttpPostedFileBase file)
        {
            int lastIndex = file.FileName.LastIndexOf("\\");
            string fileName = file.FileName.Substring(lastIndex + 1);
            string path = Path.Combine(Server.MapPath("~/Uploads"), DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetFileName(file.FileName));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            file.SaveAs(path);
            return path;
        }

        private bool FileIsValid(HttpPostedFileBase file)
        {
            bool fileValid = false;
            if (file != null && file.ContentLength > 0)
            {
                if (file.FileName.EndsWith("xlsm"))
                {
                    fileValid = true;
                }
                else
                {
                    ViewBag.Message = "It must be a .xlsm file. Other file types are not accepted.";
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return fileValid;
        }

        private bool FileIsInDatabase(HttpPostedFileBase file)
        {
            bool isInDatabase = false;
            try
            {
                int lastIndex = file.FileName.LastIndexOf("\\");
                string fileName = file.FileName.Substring(lastIndex + 1);
                using (IMS_V1Entities db = new IMS_V1Entities())
                {
                    var existFile = db.ImportFiles.Where(af => af.FileName == fileName).FirstOrDefault();
                    if (existFile != null)
                    {
                        isInDatabase = true;
                        ViewBag.Message = "ERROR: the file you uploaded has been used, or you need change the file name and try upload again.";
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return isInDatabase;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }

    public class LineData
    {
        public LineData()
        {
            this.listData = new List<string>();
        }
        public int line { get; set; }
        public List<string> listData { get; set; }
    }
}