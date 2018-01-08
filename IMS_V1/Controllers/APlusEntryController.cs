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

namespace IMS_V1.Controllers
{
    public class APlusEntryController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /APlusEntry/

        public ActionResult Index()
        {
            int userid = int.Parse(Session.Contents["UserId"].ToString());
            var items = db.Items.Include(i => i.ABC_Lookup).Include(i => i.CategoryClass).Include(i => i.FFLType).Include(i => i.FineLineClass).Include(i => i.Freight_Lookup).Include(i => i.SubClass).Include(i => i.UM_Lookup).Include(i => i.WebCode).Include(i => i.zManufacturersLogo)
                .Where(i => (i.Approved == "Y" || i.FastTrack == "Y") && (i.Printed != "Y" || i.Printed != null));
            if (userid == 25)
            {
                items = items.Where(i => i.CreatedBy == 17 && i.CreatedBy == 18 && i.CreatedBy == 25);
            }
            else
            {
                if (userid == 27)
                {
                    items = items.Where(i => i.CreatedBy == 27 && i.CreatedBy == 28);
                }
                else
                {
                    if (userid == 29)
                    {
                        items = items.Where(i => i.CreatedBy == 29);
                    }
                    else
                    {
                        if (userid == 47 || userid == 48 || userid == 49)
                        {
                            items = items.Where(i => i.CreatedBy == 47 && i.CreatedBy == 48 && i.CreatedBy == 49);
                        }
                        else
                        {
                            items = items.Where(i => i.CreatedBy != 17 && i.CreatedBy != 18 && i.CreatedBy != 25 && i.CreatedBy != 27 && i.CreatedBy != 28 && i.CreatedBy != 29 && i.CreatedBy != 47 && i.CreatedBy != 48 && i.CreatedBy != 49);
                        }
                    }
                }
            }
            return View(items.Take(1).ToList());
        }

        //
        // GET: /APlusEntry/Details/5

        public ActionResult APlusList()
        {
            int userid = int.Parse(Session.Contents["UserId"].ToString());
            var items = db.Items
                .Where(i => (i.Approved == "Y" || i.FastTrack == "Y") && (i.Printed != "Y" || i.Printed == null));
            if (userid == 25)
            {
                items = items.Where(i => i.CreatedBy == 17 || i.CreatedBy == 18 || i.CreatedBy == 25);
            }
            else
            {
                if (userid == 27)
                {
                    items = items.Where(i => i.CreatedBy == 27 || i.CreatedBy == 28);
                }
                else
                {
                    if (userid == 29)
                    {
                        items = items.Where(i => i.CreatedBy == 29);
                    }
                    else
                    {
                        if (userid == 47)
                        {
                            items = items.Where(i => i.CreatedBy == 47);
                        }
                        else
                        {
                            if (userid == 49)
                            {
                                items = items.Where(i => i.CreatedBy == 49);
                            }
                            else
                            {
                                items = items.Where(i => i.CreatedBy != 17 && i.CreatedBy != 18 && i.CreatedBy != 25 && i.CreatedBy != 27 && i.CreatedBy != 28 && i.CreatedBy != 29 && i.CreatedBy != 47 && i.CreatedBy != 48 && i.CreatedBy != 49);
                            }
                        }
                    }
                }
            }
            if (items.Count() > 0)
            {
                var lookup = items.Take(1).ToList();
                foreach (var lookupitemid in lookup)
                {
                    ViewBag.itemid = lookupitemid.Item_id;
                }

                return RedirectToAction("Details", new { id = ViewBag.itemid });
            }
            else
            {
                ViewBag.NoRecords = "No Records to Process.";
                return View("NoRecords");
            }

        }

        public ActionResult Details(int id = 0)
        {
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            //1 Chapin
            //2 Reno
            //3 Pittston
            //4 Simmons
            //5 OSHI
            //7 Newberry
            ViewBag.Item_Id = id;
            ViewBag.Chapin = InWarehouse(item.Item_id, 1);
            ViewBag.Reno = InWarehouse(id, 2);
            ViewBag.Pittson = InWarehouse(id, 3);
            ViewBag.Simmons = InWarehouse(id, 4);
            ViewBag.OSHI = InWarehouse(id, 5);
            ViewBag.NewBerry = InWarehouse(id, 7);


            return View(item);
        }

        [HttpPost]
        public ActionResult NextItem(int id)
        {

            var result1 = db.Database.ExecuteSqlCommand("APlusEntryItem @ItemId,@UserId", new SqlParameter("@ItemId", id),
                                                                                        new SqlParameter("@UserId", int.Parse(Session.Contents["UserId"].ToString())));

            return RedirectToAction("APlusList");

        }




        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private dynamic InWarehouse(int Itemid, int warehouse)
        {
            var whd = db.ItemWareHouses.Where(i => i.Item_id == Itemid && i.WareHouse_id == warehouse).Select(i => i.WareHouse_id).SingleOrDefault();
            return whd.ToString();
        }
    }
}