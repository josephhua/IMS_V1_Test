using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class zManufacturersLogoController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /zManufacturersLogo/

        public ActionResult Index()
        {
            return View(db.zManufacturersLogoes.ToList().OrderBy(z => z.APlusVendorName));
        }

        //
        // GET: /zManufacturersLogo/Details/5

        public ActionResult Details(int id = 0)
        {
            zManufacturersLogo zmanufacturerslogo = db.zManufacturersLogoes.Find(id);
            if (zmanufacturerslogo == null)
            {
                return HttpNotFound();
            }
            return View(zmanufacturerslogo);
        }

        //
        // GET: /zManufacturersLogo/Create

        public ActionResult Create()
        {
            zManufacturersLogo v = new zManufacturersLogo();
            v.vSetup = GetNewVendor();
            v.vSetup.DiffPayee = true;
            v.vSetup.FSAUsc = false;
            v.vSetup.CIToUsc = false;
            v.vSetup.FFLToUsc = false;
            v.vSetup.W9ToUsc = false;
            v.vSetup.Hazardous = false;
            v.vSetup.MSDS = false;
            v.Enabled = true;
            int vendorNum = db.GetNewVendorNum().FirstOrDefault().Value;
            v.VendorNumber = vendorNum.ToString();
            var states = db.StateProvinces.Select(a => new
            {
                VendorAddrStateId = a.Id,
                Name = a.Name,
                CId = a.CountryId
            }).Where(a => a.CId == 1).ToList();
            ViewBag.States = new SelectList(states, "VendorAddrStateId", "Name");
            var countries = db.Countries.Select(a => new
            {
                Name = a.countryName,
                Id = a.id
                
            }).OrderBy(a=> a.Id).ToList();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");
            return View(v);
        }

        protected VendorSetup GetNewVendor()
        {
            VendorSetup vSetup = new VendorSetup();           
            VendorSetupAddr addr = new VendorSetupAddr();
            vSetup.VendorAddr = addr;
            vSetup.DiffPayee = false;
            vSetup.FOBOrigin = false;
            vSetup.FOBDestination = false;
            vSetup.FSAUsc = false;
            vSetup.CIToUsc = false;
            vSetup.FFLToUsc = false;
            vSetup.W9ToUsc = false;
            vSetup.Hazardous = false;
            vSetup.MSDS = false;

            VendorPhoneFax phoneFax = new VendorPhoneFax();
            vSetup.PhoneFax = phoneFax;
            vSetup.DiffAddr = new VendorSetupAddr();
            vSetup.DiffPhoneFax = new VendorPhoneFax();
            vSetup.ITContact = new VendorEdiContact();
            vSetup.CSContact = new VendorEdiContact();
            vSetup.ACContact = new VendorEdiContact();
            return vSetup;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult StoreWebVendorName(string webvendorname)
        {
            Session.Add("webvendorname", webvendorname);
            return Json("test", JsonRequestBehavior.AllowGet);
        }
        //
        // POST: /zManufacturersLogo/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "ManufacturerLogo_Id")]zManufacturersLogo zmanufacturerslogo)
        {
            //validate first
            var chkNewVendorAdd = Request.Form["chkNewVendorAdd"];
            var chkNewPOVendor = Request.Form["chkNewPOVendor"];
            var chkChangeVendor = Request.Form["chkChangeVendor"];
            if (chkNewVendorAdd == "on")
                zmanufacturerslogo.vSetup.VendorSetupType = 1;
            else if (chkNewPOVendor == "on")
                zmanufacturerslogo.vSetup.VendorSetupType = 2;
            else if (chkChangeVendor == "on")
                zmanufacturerslogo.vSetup.VendorSetupType = 3;
            else
                ModelState.AddModelError("vSetup.VendorSetupType", "Please select a vendor setup type.");
            if (zmanufacturerslogo.APlusVendorName == null || zmanufacturerslogo.APlusVendorName.Trim().Length == 0)
                ModelState.AddModelError("APlusVendorName", "Please enter APlus vendor name.");
            if (zmanufacturerslogo.WebVendorName == null || zmanufacturerslogo.WebVendorName.Trim().Length == 0)
                ModelState.AddModelError("WebVendorName", "Please enter web vendor name.");
            if (zmanufacturerslogo.Abbrev == null || zmanufacturerslogo.Abbrev.Trim().Length == 0 || zmanufacturerslogo.Abbrev.Trim().Length < 3 || zmanufacturerslogo.Abbrev.Trim().Length > 4)
                ModelState.AddModelError("Abbrev", "Please enter a valid abbreviation.");
            else if (IsAUsedAbbrev(db.zManufacturersLogoes.Select(y => y.Abbrev).ToList(), zmanufacturerslogo.Abbrev.ToUpper()))
            {
                ModelState.AddModelError("Abbrev", "Abbreviation is a duplicate of another vendor.");
            }
            if (zmanufacturerslogo.vSetup.PhoneFax.Phone == null || zmanufacturerslogo.vSetup.PhoneFax.Phone.Trim().Length < 10)
                ModelState.AddModelError("vSetup.PhoneFax.Phone", "Please enter valid phone number.");
            if (zmanufacturerslogo.vSetup.ContactName == null || zmanufacturerslogo.vSetup.ContactName.Trim().Length < 1)
                ModelState.AddModelError("vSetup.ContactName", "Please enter contact name.");

            var chkDifferentAddrYes = Request.Form["chkDifferentAddrYes"];
            if (chkDifferentAddrYes == "on")
                zmanufacturerslogo.vSetup.DiffPayee = true;
            else
                zmanufacturerslogo.vSetup.DiffPayee = false;
            if (zmanufacturerslogo.vSetup.DiffPayee == true)
            {
                if (zmanufacturerslogo.vSetup.DiffVendorNum == null || zmanufacturerslogo.vSetup.DiffVendorNum.Trim().Length < 6)
                    ModelState.AddModelError("vSetup.DiffVendorNum", "Please enter a valid vendor number.");
            }
            var chkIncludeUPS = Request.Form["chkIncludeUPS"];
            if (chkIncludeUPS == "on")
                zmanufacturerslogo.vSetup.IncludeUPS = true;
            else
                zmanufacturerslogo.vSetup.IncludeUPS = false;
            var chkOrigin = Request.Form["chkOrigin"];
            if (chkOrigin == "on")
                zmanufacturerslogo.vSetup.FOBOrigin = true;
            else
                zmanufacturerslogo.vSetup.FOBOrigin = false;
            var chkDestination = Request.Form["chkDestination"];
            if (chkDestination == "on")
                zmanufacturerslogo.vSetup.FOBDestination = true;
            else
                zmanufacturerslogo.vSetup.FOBDestination = false;

            var chkFSAUscYes = Request.Form["chkFSAUscYes"];
            if (chkFSAUscYes == "on")
                zmanufacturerslogo.vSetup.FSAUsc = true;
            else
                zmanufacturerslogo.vSetup.FSAUsc = false;
            var chkCIToUscYes = Request.Form["chkCIToUscYes"];
            if (chkCIToUscYes == "on")
                zmanufacturerslogo.vSetup.CIToUsc = true;
            else
                zmanufacturerslogo.vSetup.CIToUsc = false;
            var chkFFLToUscYes = Request.Form["chkFFLToUscYes"];
            if (chkFFLToUscYes == "on")
                zmanufacturerslogo.vSetup.FFLToUsc = true;
            else
                zmanufacturerslogo.vSetup.FFLToUsc = false;
            var chkW9ToUscYes = Request.Form["chkW9ToUscYes"];
            if (chkW9ToUscYes == "on")
                zmanufacturerslogo.vSetup.W9ToUsc = true;
            else
                zmanufacturerslogo.vSetup.W9ToUsc = false;
            var chkHazardousYes = Request.Form["chkHazardousYes"];
            if (chkHazardousYes == "on")
                zmanufacturerslogo.vSetup.Hazardous = true;
            else
                zmanufacturerslogo.vSetup.Hazardous = false;
            var chkMSDSYes = Request.Form["chkMSDSYes"];
            if (chkMSDSYes == "on")
                zmanufacturerslogo.vSetup.MSDS = true;
            else
                zmanufacturerslogo.vSetup.MSDS = false;

            if (ModelState.IsValid)
            {
                zmanufacturerslogo.Abbrev = zmanufacturerslogo.Abbrev.ToUpper();  //make sure all upcases
                db.zManufacturersLogoes.Add(zmanufacturerslogo);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                zmanufacturerslogo.ModifiedBy = userid;
                zmanufacturerslogo.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                zmanufacturerslogo.vSetup.VendorAddr.StateId = zmanufacturerslogo.VendorAddrStateId;
                zmanufacturerslogo.vSetup.zManufacturersLogoId = zmanufacturerslogo.ManufacturerLogo_Id;
                db.VendorSetupAddrs.Add(zmanufacturerslogo.vSetup.VendorAddr);
                db.SaveChanges();
                zmanufacturerslogo.vSetup.AddrId = zmanufacturerslogo.vSetup.VendorAddr.id;
                db.VendorPhoneFaxes.Add(zmanufacturerslogo.vSetup.PhoneFax);
                db.SaveChanges();
                zmanufacturerslogo.vSetup.VendorContactInfoId = zmanufacturerslogo.vSetup.PhoneFax.Id;

                if (zmanufacturerslogo.vSetup.DiffPayee.Value)
                {
                    db.VendorSetupAddrs.Add(zmanufacturerslogo.vSetup.DiffAddr);
                    db.SaveChanges();
                    zmanufacturerslogo.vSetup.DiffAddrId = zmanufacturerslogo.vSetup.DiffAddr.id;
                    db.VendorPhoneFaxes.Add(zmanufacturerslogo.vSetup.DiffPhoneFax);
                    db.SaveChanges();
                    zmanufacturerslogo.vSetup.DiffContactInfoId = zmanufacturerslogo.vSetup.DiffPhoneFax.Id;
                }
                if (NeedSaveContactInfo(zmanufacturerslogo.vSetup.ITContact))
                {
                    db.VendorEdiContacts.Add(zmanufacturerslogo.vSetup.ITContact);
                    db.SaveChanges();
                    zmanufacturerslogo.vSetup.ItContactId = zmanufacturerslogo.vSetup.ITContact.Id;
                }
                if (NeedSaveContactInfo(zmanufacturerslogo.vSetup.CSContact))
                {
                    db.VendorEdiContacts.Add(zmanufacturerslogo.vSetup.CSContact);
                    db.SaveChanges();
                    zmanufacturerslogo.vSetup.CSContactId = zmanufacturerslogo.vSetup.CSContact.Id;
                }
                if (NeedSaveContactInfo(zmanufacturerslogo.vSetup.ACContact))
                {
                    db.VendorEdiContacts.Add(zmanufacturerslogo.vSetup.ACContact);
                    db.SaveChanges();
                    zmanufacturerslogo.vSetup.AcctContactId = zmanufacturerslogo.vSetup.ACContact.Id;
                }
                db.VendorSetups.Add(zmanufacturerslogo.vSetup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var states = db.StateProvinces.Select(a => new
            {
                VendorAddrStateId = a.Id,
                Name = a.Name,
                CId = a.CountryId
            }).Where(a => a.CId == 1).ToList();
            ViewBag.States = new SelectList(states, "VendorAddrStateId", "Name");
            var countries = db.Countries.Select(a => new
            {
                Name = a.countryName,
                Id = a.id

            }).OrderBy(a => a.Id).ToList();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");
            return View(zmanufacturerslogo);
        }

        protected bool NeedSaveContactInfo(VendorEdiContact contact)
        {
            bool need = false;
            if (contact.Name != null && contact.Name.Trim().Length > 0)
                return true;
            if (contact.Phone != null && contact.Phone.Trim().Length > 0)
                return true;
            if (contact.Email != null && contact.Email.Trim().Length > 0)
                return true;
            return need;
        }

        public JsonResult GetAbbrevs(string term)
        {
            string webVendorName = Session.Contents["webvendorname"].ToString();
            string[] names = webVendorName.Split(' ');
            string sl = "";
            string tl = "";
            string fl = "";
            if (names.Length >= 2)
            {
                sl = names[1];
                sl = sl.Substring(0, 1).ToUpper();
            }
            if (names.Length >= 3)
            {
                tl = names[2];
                tl = tl.Substring(0, 1).ToUpper();
            }
            if (names.Length > 3)
            {
                fl = names[3];
                if (fl != null && fl.Trim().Length > 0)
                    fl = fl.Substring(0, 1).ToUpper();
            }
            List<string> abbrevs = new List<string>();
            List<string> usedAbbrevs = db.zManufacturersLogoes.Where(x => x.Abbrev.StartsWith(term)).Select(y => y.Abbrev).ToList();
            string abbrev = "";
            
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 3, 1);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 3, 2);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 3, 3);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 3, 4);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 3, 5);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 4, 1);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 4, 2);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 4, 3);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 4, 4);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            abbrev = GetPossibleAbbrev(term, sl, tl, fl, 4, 5);
            if (!IsAUsedAbbrev(usedAbbrevs, abbrev)) abbrevs.Add(abbrev);
            
            return Json(abbrevs, JsonRequestBehavior.AllowGet);
        }

        protected string GetPossibleAbbrev(string term, string sl, string tl, string fl, int length, int count)
        {
            string abbrev="";
            term = term.ToUpper();
            if (term.Length == 1)
            {
                if (length == 3)
                {
                    if (count == 1)
                        abbrev = term + sl + tl;
                    else if (count == 2 && tl != "A")
                        abbrev = term + sl + "A";
                    else if (count == 2 && sl != "A")
                        abbrev = term + "A" + tl;
                    else if (count == 3 && tl != "B")
                        abbrev = term + sl + "B";
                    else if (count == 3 && sl != "B")
                        abbrev = term + "B" + tl;
                    else if (count == 4 && tl != "C")
                        abbrev = term + sl + "C";
                    else if (count == 4 && sl != "C")
                        abbrev = term + "C" + tl;
                    else if (count == 5 && tl != "D")
                        abbrev = term + sl + "D";
                    else if (count == 5 && sl != "D")
                        abbrev = term + "D" + tl;
                }
                else //length == 4
                {
                    if (count == 1)
                        abbrev = term + sl + tl + fl;
                    else if (count == 2 && fl != "A")
                        abbrev = term + sl + tl + "A";
                    else if (count == 2 && tl != "A")
                        abbrev = term + sl + "A" + fl;
                    else if (count == 3 && fl != "B")
                        abbrev = term + sl + tl + "B";
                    else if (count == 3 && tl != "B")
                        abbrev = term + sl + "B" + fl;
                    else if (count == 4 && fl != "C")
                        abbrev = term + sl + tl + "C";
                    else if (count == 4 && tl != "C")
                        abbrev = term + sl + "C" + fl;
                    else if (count == 5 && fl != "D")
                        abbrev = term + sl + tl + "D";
                    else if (count == 5 && tl != "D")
                        abbrev = term + sl + "D" + fl;
                }

            }
            else if (term.Length == 2)
            {
                if (length == 3)
                {
                    if (count == 1 && tl != "")
                        abbrev = term + tl;
                    else if (count == 1 && tl == "")
                        abbrev = term + "A";
                    else if (count == 2 && tl != "B" && tl != "")
                        abbrev = term + "B";
                    else if (count == 2 && fl != "C" && fl != "")
                        abbrev = term + fl;
                    else if (count == 3 && tl != "D" && tl != "")
                        abbrev = term + "D";
                    else if (count == 3 && fl != "E" && fl != "")
                        abbrev = term + "E";
                    else if (count == 4 && tl != "F" && tl != "")
                        abbrev = term + "F";
                    else if (count == 4 && fl != "G" && fl != "")
                        abbrev = term + "G";
                    else if (count == 5 && tl != "H" && tl != "")
                        abbrev = term + "H";
                    else if (count == 5 && fl != "I" && fl != "")
                        abbrev = term + "I";
                    if (abbrev.Length < 3) abbrev += "J";
                }
                else //length == 4
                {
                    if (count == 1 && fl != "")
                        abbrev = term + tl + fl;
                    else if (count == 1 && fl == "")
                        abbrev = term + tl + "A";
                    else if (count == 2 && fl != "B")
                        abbrev = term + tl + "B";
                    else if (count == 2 && fl != "C")
                        abbrev = term + tl + "C";
                    else if (count == 3 && fl != "D")
                        abbrev = term + tl + "D";
                    else if (count == 3 && fl != "E")
                        abbrev = term + tl + "E";
                    else if (count == 4 && fl != "F")
                        abbrev = term + tl + "F";
                    else if (count == 4 && fl != "G")
                        abbrev = term + tl + "G";
                    else if (count == 5 && fl != "H")
                        abbrev = term + tl + "H";
                    else if (count == 5 && fl != "I")
                        abbrev = term + fl + "I";
                    if (abbrev.Length < 4) abbrev += "J";
                }
            }
            else if (term.Length >= 3)
            {
                term = term.Substring(0, 3);
                if (fl == "")
                {
                    switch(count)
                    {
                        case 1:
                            abbrev = term + "A";
                            break;
                        case 2:
                            abbrev = term + "B";
                            break;
                        case 3:
                            abbrev = term + "C";
                            break;
                        case 4:
                            abbrev = term + "D";
                            break;
                        case 5:
                            abbrev = term + "E";
                            break;
                        default:
                            abbrev = term + "Z";
                            break;
                    }
                }
                else
                {
                    switch (count)
                    {
                        case 1:
                            if (fl != "A")
                                abbrev = term + "A";
                            else
                                abbrev = term + "B";
                            break;
                        case 2:
                            if (fl != "C")
                                abbrev = term + "C";
                            else
                                abbrev = term + "D";
                            break;
                        case 3:
                            if (fl != "E")
                                abbrev = term + "E";
                            else
                                abbrev = term + "F";
                            break;
                        case 4:
                            if (fl != "G")
                                abbrev = term + "G";
                            else
                                abbrev = term + "H";
                            break;
                        case 5:
                            if (fl != "I")
                                abbrev = term + "I";
                            else
                                abbrev = term + "J";
                            break;
                        default:
                            abbrev = term + "Z";
                            break;
                    }
                }
            }
            if (abbrev.Length < length) abbrev += "Z";
            return abbrev;
        }

        protected bool IsAUsedAbbrev(List<string> usedAbbrevs, string str)
        {
            bool isUsed = usedAbbrevs.Contains(str);
            return isUsed;
        }
        //
        // GET: /zManufacturersLogo/Edit/5

        public ActionResult Edit(int id = 0)
        {
            zManufacturersLogo zmanufacturerslogo = db.zManufacturersLogoes.Find(id);
            if (zmanufacturerslogo == null)
            {
                return HttpNotFound();
            }
            VendorSetup vsetUp = db.VendorSetups.Where(z => z.zManufacturersLogoId == zmanufacturerslogo.ManufacturerLogo_Id).FirstOrDefault();
            if (vsetUp == null)
            { 
                zmanufacturerslogo.vSetup = GetNewVendor();
                zmanufacturerslogo.vSetup.IncludeUPS = false;
            }
            else
            {
                zmanufacturerslogo.vSetup = vsetUp;
                VendorSetupAddr addr = db.VendorSetupAddrs.Find(vsetUp.AddrId);
                if (addr == null)
                    addr = new VendorSetupAddr();
                zmanufacturerslogo.vSetup.VendorAddr = addr;
                VendorPhoneFax phoneFax = db.VendorPhoneFaxes.Find(vsetUp.VendorContactInfoId);
                if (phoneFax == null)
                    phoneFax = new VendorPhoneFax();
                zmanufacturerslogo.vSetup.PhoneFax = phoneFax;
                if (zmanufacturerslogo.vSetup.DiffPayee.Value)
                {
                    if (zmanufacturerslogo.vSetup.DiffAddrId != null && zmanufacturerslogo.vSetup.DiffAddrId.Value != 0)
                    {
                        VendorSetupAddr diffaddr = db.VendorSetupAddrs.Find(zmanufacturerslogo.vSetup.DiffAddrId.Value);
                        if (diffaddr != null)
                            zmanufacturerslogo.vSetup.DiffAddr = diffaddr;
                        else
                            zmanufacturerslogo.vSetup.DiffAddr = new VendorSetupAddr();
                    }
                    if (zmanufacturerslogo.vSetup.DiffContactInfoId != null && zmanufacturerslogo.vSetup.DiffContactInfoId.Value != 0)
                    {
                        VendorPhoneFax diffphoneFx = db.VendorPhoneFaxes.Find(zmanufacturerslogo.vSetup.DiffContactInfoId.Value);
                        if (diffphoneFx != null)
                            zmanufacturerslogo.vSetup.DiffPhoneFax = diffphoneFx;
                        else
                            zmanufacturerslogo.vSetup.DiffPhoneFax = new VendorPhoneFax();
                    }
                }
                if (zmanufacturerslogo.vSetup.ItContactId != null && zmanufacturerslogo.vSetup.ItContactId.Value != 0)
                {
                    VendorEdiContact itcontact = db.VendorEdiContacts.Find(zmanufacturerslogo.vSetup.ItContactId.Value);
                    if (itcontact != null)
                        zmanufacturerslogo.vSetup.ITContact = itcontact;
                    else
                        zmanufacturerslogo.vSetup.ITContact = new VendorEdiContact();
                }
                if (zmanufacturerslogo.vSetup.CSContactId != null && zmanufacturerslogo.vSetup.CSContactId.Value != 0)
                {
                    VendorEdiContact cscontact = db.VendorEdiContacts.Find(zmanufacturerslogo.vSetup.CSContactId.Value);
                    if (cscontact != null)
                        zmanufacturerslogo.vSetup.CSContact = cscontact;
                    else
                        zmanufacturerslogo.vSetup.CSContact = new VendorEdiContact();
                }
                if (zmanufacturerslogo.vSetup.AcctContactId != null && zmanufacturerslogo.vSetup.AcctContactId.Value != 0)
                {
                    VendorEdiContact accontact = db.VendorEdiContacts.Find(zmanufacturerslogo.vSetup.AcctContactId.Value);
                    if (accontact != null)
                        zmanufacturerslogo.vSetup.ACContact = accontact;
                    else
                        zmanufacturerslogo.vSetup.ACContact = new VendorEdiContact();
                }
            }
            
            var states = db.StateProvinces.Select(a => new
            {
                VendorAddrStateId = a.Id,
                Name = a.Name,
                CId = a.CountryId
            }).Where(a => a.CId == 1).ToList();
            ViewBag.States = new SelectList(states, "VendorAddrStateId", "Name");
            var countries = db.Countries.Select(a => new
            {
                Name = a.countryName,
                Id = a.id

            }).OrderBy(a => a.Id).ToList();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

            return View(zmanufacturerslogo);
        }

        //
        // POST: /zManufacturersLogo/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(zManufacturersLogo zmanufacturerslogo)
        {
            //ModelState.;
            var chkNewVendorAdd = Request.Form["chkNewVendorAdd"];
            var chkNewPOVendor = Request.Form["chkNewPOVendor"];
            var chkChangeVendor = Request.Form["chkChangeVendor"];
            if (chkNewVendorAdd == "on")
                zmanufacturerslogo.vSetup.VendorSetupType = 1;
            else if (chkNewPOVendor == "on")
                zmanufacturerslogo.vSetup.VendorSetupType = 2;
            else if (chkChangeVendor == "on")
                zmanufacturerslogo.vSetup.VendorSetupType = 3;
            else
                ModelState.AddModelError("vSetup.VendorSetupType", "Please select a vendor setup type.");
            if (zmanufacturerslogo.APlusVendorName == null || zmanufacturerslogo.APlusVendorName.Trim().Length == 0)
                ModelState.AddModelError("APlusVendorName", "Please enter APlus vendor name.");
            if (zmanufacturerslogo.WebVendorName == null || zmanufacturerslogo.WebVendorName.Trim().Length == 0)
                ModelState.AddModelError("WebVendorName", "Please enter web vendor name.");
            if (zmanufacturerslogo.Abbrev == null || zmanufacturerslogo.Abbrev.Trim().Length == 0 || zmanufacturerslogo.Abbrev.Trim().Length < 3 || zmanufacturerslogo.Abbrev.Trim().Length > 4)
                ModelState.AddModelError("Abbrev", "Please enter a valid abbreviation.");
            else // if (IsAUsedAbbrev(db.zManufacturersLogoes.Select(y => y.Abbrev).ToList(), zmanufacturerslogo.Abbrev.ToUpper()))
            {
                zManufacturersLogo oldOne = db.zManufacturersLogoes.Where(z => z.ManufacturerLogo_Id == zmanufacturerslogo.ManufacturerLogo_Id).FirstOrDefault();
                if (!oldOne.Abbrev.Equals(zmanufacturerslogo.Abbrev) && IsAUsedAbbrev(db.zManufacturersLogoes.Select(y => y.Abbrev).ToList(), zmanufacturerslogo.Abbrev.ToUpper()))
                    ModelState.AddModelError("Abbrev", "Abbreviation is a duplicate of another vendor.");
            }
            if (zmanufacturerslogo.vSetup.PhoneFax.Phone == null || zmanufacturerslogo.vSetup.PhoneFax.Phone.Trim().Length < 10)
                ModelState.AddModelError("vSetup.PhoneFax.Phone", "Please enter valid phone number.");
            if (zmanufacturerslogo.vSetup.ContactName == null || zmanufacturerslogo.vSetup.ContactName.Trim().Length < 1)
                ModelState.AddModelError("vSetup.ContactName", "Please enter contact name.");

            var chkDifferentAddrYes = Request.Form["chkDifferentAddrYes"];
            if (chkDifferentAddrYes == "on")
                zmanufacturerslogo.vSetup.DiffPayee = true;
            else
                zmanufacturerslogo.vSetup.DiffPayee = false;
            if (zmanufacturerslogo.vSetup.DiffPayee == true)
            {
                if (zmanufacturerslogo.vSetup.DiffVendorNum == null || zmanufacturerslogo.vSetup.DiffVendorNum.Trim().Length < 6)
                    ModelState.AddModelError("vSetup.DiffVendorNum", "Please enter a valid vendor number.");
            }
            var chkIncludeUPS = Request.Form["chkIncludeUPS"];
            if (chkIncludeUPS == "on")
                zmanufacturerslogo.vSetup.IncludeUPS = true;
            else
                zmanufacturerslogo.vSetup.IncludeUPS = false;
            var chkOrigin = Request.Form["chkOrigin"];
            if (chkOrigin == "on")
                zmanufacturerslogo.vSetup.FOBOrigin = true;
            else
                zmanufacturerslogo.vSetup.FOBOrigin = false;
            var chkDestination = Request.Form["chkDestination"];
            if (chkDestination == "on")
                zmanufacturerslogo.vSetup.FOBDestination = true;
            else
                zmanufacturerslogo.vSetup.FOBDestination = false;

            var chkFSAUscYes = Request.Form["chkFSAUscYes"];
            if (chkFSAUscYes == "on")
                zmanufacturerslogo.vSetup.FSAUsc = true;
            else
                zmanufacturerslogo.vSetup.FSAUsc = false;
            var chkCIToUscYes = Request.Form["chkCIToUscYes"];
            if (chkCIToUscYes == "on")
                zmanufacturerslogo.vSetup.CIToUsc = true;
            else
                zmanufacturerslogo.vSetup.CIToUsc = false;
            var chkFFLToUscYes = Request.Form["chkFFLToUscYes"];
            if (chkFFLToUscYes == "on")
                zmanufacturerslogo.vSetup.FFLToUsc = true;
            else
                zmanufacturerslogo.vSetup.FFLToUsc = false;
            var chkW9ToUscYes = Request.Form["chkW9ToUscYes"];
            if (chkW9ToUscYes == "on")
                zmanufacturerslogo.vSetup.W9ToUsc = true;
            else
                zmanufacturerslogo.vSetup.W9ToUsc = false;
            var chkHazardousYes = Request.Form["chkHazardousYes"];
            if (chkHazardousYes == "on")
                zmanufacturerslogo.vSetup.Hazardous = true;
            else
                zmanufacturerslogo.vSetup.Hazardous = false;
            var chkMSDSYes = Request.Form["chkMSDSYes"];
            if (chkMSDSYes == "on")
                zmanufacturerslogo.vSetup.MSDS = true;
            else
                zmanufacturerslogo.vSetup.MSDS = false;
            if (ModelState.IsValid)
            {
                var dbManuLogo = db.zManufacturersLogoes.FirstOrDefault(p => p.ManufacturerLogo_Id == zmanufacturerslogo.ManufacturerLogo_Id);
                if (dbManuLogo == null)
                {
                    return HttpNotFound();
                }
                var vsetUp = db.VendorSetups.FirstOrDefault(v => v.id == zmanufacturerslogo.vSetup.id);
                bool isNewObject = false;
                if (vsetUp == null)
                {
                    vsetUp = GetNewVendor();
                    isNewObject = true;
                }
                    
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                dbManuLogo.ModifiedBy = userid;
                dbManuLogo.ModifiedDate = DateTime.Now;
                dbManuLogo.VendorNumber = zmanufacturerslogo.VendorNumber;
                dbManuLogo.APlusVendorName = zmanufacturerslogo.APlusVendorName;
                dbManuLogo.LogoName = zmanufacturerslogo.LogoName;
                dbManuLogo.WebVendorName = zmanufacturerslogo.WebVendorName;
                dbManuLogo.Abbrev = zmanufacturerslogo.Abbrev;
                dbManuLogo.URL = zmanufacturerslogo.URL;
                dbManuLogo.Enabled = zmanufacturerslogo.Enabled;
                db.SaveChanges();

                var dbVendorAddr = db.VendorSetupAddrs.FirstOrDefault(v => v.id == zmanufacturerslogo.vSetup.VendorAddr.id);
                if (dbVendorAddr == null)
                {
                    dbVendorAddr = new VendorSetupAddr();
                }
                dbVendorAddr.Addr1 = zmanufacturerslogo.vSetup.VendorAddr.Addr1;
                dbVendorAddr.Addr2 = zmanufacturerslogo.vSetup.VendorAddr.Addr2;
                dbVendorAddr.City = zmanufacturerslogo.vSetup.VendorAddr.City;
                dbVendorAddr.StateId = zmanufacturerslogo.vSetup.VendorAddr.StateId;
                dbVendorAddr.CountryId = zmanufacturerslogo.vSetup.VendorAddr.CountryId;
                dbVendorAddr.Zip4 = zmanufacturerslogo.vSetup.VendorAddr.Zip4;
                if (isNewObject)
                    db.VendorSetupAddrs.Add(dbVendorAddr);

                db.SaveChanges();
                if (isNewObject)
                {
                    vsetUp.VendorAddr.id = dbVendorAddr.id;
                    vsetUp.AddrId = dbVendorAddr.id;
                    zmanufacturerslogo.vSetup.VendorAddr.id = dbVendorAddr.id;
                }
                    
                //db.Entry(zmanufacturerslogo.vSetup.PhoneFax).State = EntityState.Modified;
                var dbPhoneFax = db.VendorPhoneFaxes.FirstOrDefault(p => p.Id == zmanufacturerslogo.vSetup.PhoneFax.Id);
                if (dbPhoneFax == null)
                    dbPhoneFax = new VendorPhoneFax();
                dbPhoneFax.Phone = zmanufacturerslogo.vSetup.PhoneFax.Phone;
                dbPhoneFax.PhoneExt = zmanufacturerslogo.vSetup.PhoneFax.PhoneExt;
                dbPhoneFax.Fax = zmanufacturerslogo.vSetup.PhoneFax.Fax;

                if (isNewObject)
                    db.VendorPhoneFaxes.Add(dbPhoneFax);
                db.SaveChanges();
                if (isNewObject)
                {
                    vsetUp.PhoneFax.Id = dbPhoneFax.Id;
                    vsetUp.VendorContactInfoId = dbPhoneFax.Id;
                    zmanufacturerslogo.vSetup.PhoneFax.Id = dbPhoneFax.Id;
                }
                if (zmanufacturerslogo.vSetup.DiffPayee.Value)
                {
                    //db.Entry(zmanufacturerslogo.vSetup.DiffAddr).State = EntityState.Modified;
                    var diffAddr = db.VendorSetupAddrs.FirstOrDefault(v => v.id == vsetUp.DiffAddrId);
                    if (diffAddr == null) { 
                        diffAddr = new VendorSetupAddr();
                        isNewObject = true;
                    }
                    diffAddr.Addr1 = zmanufacturerslogo.vSetup.DiffAddr.Addr1;
                    diffAddr.Addr2 = zmanufacturerslogo.vSetup.DiffAddr.Addr2;
                    diffAddr.City = zmanufacturerslogo.vSetup.DiffAddr.City;
                    diffAddr.StateId = zmanufacturerslogo.vSetup.DiffAddr.StateId;
                    diffAddr.CountryId = zmanufacturerslogo.vSetup.DiffAddr.CountryId;
                    diffAddr.Zip4 = zmanufacturerslogo.vSetup.DiffAddr.Zip4;

                    if (isNewObject)
                        db.VendorSetupAddrs.Add(diffAddr);
                    db.SaveChanges();
                    
                    if (isNewObject)
                    {
                        vsetUp.DiffAddr.id = diffAddr.id;
                        vsetUp.DiffAddrId = diffAddr.id;
                        zmanufacturerslogo.vSetup.DiffAddr.id = diffAddr.id; 
                    }
                    //db.Entry(zmanufacturerslogo.vSetup.DiffPhoneFax).State = EntityState.Modified;
                    var diffPhoneFax = db.VendorPhoneFaxes.FirstOrDefault(p => p.Id == vsetUp.DiffContactInfoId);
                    if (diffPhoneFax == null)
                    {
                        diffPhoneFax = new VendorPhoneFax();
                        isNewObject = true;
                    }
                    diffPhoneFax.Phone = zmanufacturerslogo.vSetup.DiffPhoneFax.Phone;
                    diffPhoneFax.PhoneExt = zmanufacturerslogo.vSetup.DiffPhoneFax.PhoneExt;
                    diffPhoneFax.Fax = zmanufacturerslogo.vSetup.DiffPhoneFax.Fax;

                    if (isNewObject)
                        db.VendorPhoneFaxes.Add(diffPhoneFax);
                    db.SaveChanges();
                    if (isNewObject)
                    {
                        vsetUp.DiffPhoneFax.Id = diffPhoneFax.Id;
                        vsetUp.DiffContactInfoId = diffPhoneFax.Id;
                        zmanufacturerslogo.vSetup.DiffContactInfoId = diffPhoneFax.Id;
                    }
                }

                //db.Entry(zmanufacturerslogo.vSetup.ITContact).State = EntityState.Modified;
                var itContact = db.VendorEdiContacts.FirstOrDefault(v => v.Id == vsetUp.ItContactId);
                if (itContact == null)
                {
                    itContact = new VendorEdiContact();
                    isNewObject = true;
                }
                itContact.Name = zmanufacturerslogo.vSetup.ITContact.Name;
                itContact.Phone = zmanufacturerslogo.vSetup.ITContact.Phone;
                itContact.Email = zmanufacturerslogo.vSetup.ITContact.Email;
                if (isNewObject && NeedSaveContact(itContact))
                    db.VendorEdiContacts.Add(itContact);
                db.SaveChanges();
                if (isNewObject)
                {
                    vsetUp.ITContact = itContact;
                    vsetUp.ItContactId = itContact.Id;
                    zmanufacturerslogo.vSetup.ITContact = itContact;
                }

                    //db.Entry(zmanufacturerslogo.vSetup.CSContact).State = EntityState.Modified;
                var csContact = db.VendorEdiContacts.FirstOrDefault(v => v.Id == vsetUp.CSContactId);
                if (csContact == null)
                {
                    csContact = new VendorEdiContact();
                    isNewObject = true;
                }
                    
                csContact.Name = zmanufacturerslogo.vSetup.CSContact.Name;
                csContact.Phone = zmanufacturerslogo.vSetup.CSContact.Phone;
                csContact.Email = zmanufacturerslogo.vSetup.CSContact.Email;
                if (isNewObject && NeedSaveContact(csContact))
                    db.VendorEdiContacts.Add(csContact);
                db.SaveChanges();
                if (isNewObject)
                {
                    vsetUp.CSContact = csContact;
                    vsetUp.CSContactId = csContact.Id;
                    zmanufacturerslogo.vSetup.CSContact = csContact;
                }

                //db.Entry(zmanufacturerslogo.vSetup.ACContact).State = EntityState.Modified;
                var acContact = db.VendorEdiContacts.FirstOrDefault(v => v.Id == vsetUp.AcctContactId);
                if (acContact == null)
                {
                    acContact = new VendorEdiContact();
                    isNewObject = true;
                }
                acContact.Name = zmanufacturerslogo.vSetup.ACContact.Name;
                acContact.Phone = zmanufacturerslogo.vSetup.ACContact.Phone;
                acContact.Email = zmanufacturerslogo.vSetup.ACContact.Email;
                if (isNewObject && NeedSaveContact(acContact))
                    db.VendorEdiContacts.Add(acContact);
                db.SaveChanges();
                if (isNewObject)
                {
                    vsetUp.ACContact = acContact;
                    vsetUp.AcctContactId = acContact.Id;
                    zmanufacturerslogo.vSetup.ACContact = acContact;
                }
                //db.Entry(zmanufacturerslogo.vSetup).State = EntityState.Modified;
                
                vsetUp.ContactName = zmanufacturerslogo.vSetup.ContactName;
                vsetUp.Comments = zmanufacturerslogo.vSetup.Comments;
                vsetUp.DiffPayee = zmanufacturerslogo.vSetup.DiffPayee;
                vsetUp.DiffVendorNum = zmanufacturerslogo.vSetup.DiffVendorNum;
                vsetUp.DiffVendorName = zmanufacturerslogo.vSetup.DiffVendorName;
                vsetUp.DiffAddrId = zmanufacturerslogo.vSetup.DiffAddrId;
                vsetUp.DiffContactInfoId = zmanufacturerslogo.vSetup.DiffContactInfoId;
                vsetUp.FreightCompany = zmanufacturerslogo.vSetup.FreightCompany;
                vsetUp.IncludeUPS = zmanufacturerslogo.vSetup.IncludeUPS;
                vsetUp.FOBOrigin = zmanufacturerslogo.vSetup.FOBOrigin;
                vsetUp.FOBDestination = zmanufacturerslogo.vSetup.FOBDestination;
                vsetUp.FreightTerms = zmanufacturerslogo.vSetup.FreightTerms;
                vsetUp.MinValue = zmanufacturerslogo.vSetup.MinValue;
                vsetUp.MinUnits = zmanufacturerslogo.vSetup.MinUnits;
                vsetUp.MinWeight = zmanufacturerslogo.vSetup.MinWeight;
                vsetUp.MinSize = zmanufacturerslogo.vSetup.MinSize;
                vsetUp.CO_OPPercent = zmanufacturerslogo.vSetup.CO_OPPercent;
                vsetUp.VendorLeadTime = zmanufacturerslogo.vSetup.VendorLeadTime;
                vsetUp.PTDscPercent = zmanufacturerslogo.vSetup.PTDscPercent;
                vsetUp.PTDscDays = zmanufacturerslogo.vSetup.PTDscDays;
                vsetUp.PTPayDays = zmanufacturerslogo.vSetup.PTPayDays;
                vsetUp.FSAUsc = zmanufacturerslogo.vSetup.FSAUsc;
                vsetUp.CIToUsc = zmanufacturerslogo.vSetup.CIToUsc;
                vsetUp.FFLToUsc = zmanufacturerslogo.vSetup.FFLToUsc;
                vsetUp.W9ToUsc = zmanufacturerslogo.vSetup.W9ToUsc;
                vsetUp.Hazardous = zmanufacturerslogo.vSetup.Hazardous;
                vsetUp.MSDS = zmanufacturerslogo.vSetup.MSDS;
                vsetUp.AcctContactName = zmanufacturerslogo.vSetup.AcctContactName;
                vsetUp.AcctComments = zmanufacturerslogo.vSetup.AcctComments;
                vsetUp.zManufacturersLogoId = zmanufacturerslogo.ManufacturerLogo_Id;
                vsetUp.VendorSetupType = zmanufacturerslogo.vSetup.VendorSetupType;
                if (vsetUp.id == 0)
                    db.VendorSetups.Add(vsetUp);
                db.SaveChanges();
         
                return RedirectToAction("Index");
            }
            var states = db.StateProvinces.Select(a => new
            {
                VendorAddrStateId = a.Id,
                Name = a.Name,
                CId = a.CountryId
            }).Where(a => a.CId == 1).ToList();
            ViewBag.States = new SelectList(states, "VendorAddrStateId", "Name");
            var countries = db.Countries.Select(a => new
            {
                Name = a.countryName,
                Id = a.id

            }).OrderBy(a => a.Id).ToList();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

            return View(zmanufacturerslogo);
        }

        protected bool NeedSaveContact(VendorEdiContact ec)
        {
            if (ec.Phone != null && ec.Phone.Trim().Length > 0)
                return true;
            if (ec.Name != null && ec.Name.Trim().Length > 0)
                return true;
            if (ec.Email != null && ec.Email.Trim().Length > 0)
                return true;
            return false;
        }

        //
        // GET: /zManufacturersLogo/Delete/5

        public ActionResult Delete(int id = 0)
        {
            zManufacturersLogo zmanufacturerslogo = db.zManufacturersLogoes.Find(id);
            if (zmanufacturerslogo == null)
            {
                return HttpNotFound();
            }
            return View(zmanufacturerslogo);
        }

        //
        // POST: /zManufacturersLogo/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            zManufacturersLogo zmanufacturerslogo = db.zManufacturersLogoes.Find(id);
            db.zManufacturersLogoes.Remove(zmanufacturerslogo);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}