using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class Freight_LookupController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /Freight_Lookup/

        public ActionResult Index()
        {
            return View(db.Freight_Lookup.ToList());
        }

        //
        // GET: /Freight_Lookup/Details/5

        public ActionResult Details(int id = 0)
        {
            Freight_Lookup freight_lookup = db.Freight_Lookup.Find(id);
            if (freight_lookup == null)
            {
                return HttpNotFound();
            }
            return View(freight_lookup);
        }

        //
        // GET: /Freight_Lookup/Create

        public ActionResult Create()
        {
            ViewBag.Hazardous = LoadHazardous("");
            return View();
        }

        //
        // POST: /Freight_Lookup/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "Freight_Id")]Freight_Lookup freight_lookup)
        {
            if (ModelState.IsValid)
            {
                db.Freight_Lookup.Add(freight_lookup);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                freight_lookup.ModifiedBy = userid;
                freight_lookup.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(freight_lookup);
        }

        //
        // GET: /Freight_Lookup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Freight_Lookup freight_lookup = db.Freight_Lookup.Find(id);
            ViewBag.Hazardous = LoadHazardous(freight_lookup.Freight_Hazard);

            if (freight_lookup == null)
            {
                return HttpNotFound();
            }
            return View(freight_lookup);
        }

        //
        // POST: /Freight_Lookup/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Freight_Lookup freight_lookup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(freight_lookup).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                freight_lookup.ModifiedBy = userid;
                freight_lookup.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(freight_lookup);
        }

        //
        // GET: /Freight_Lookup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Freight_Lookup freight_lookup = db.Freight_Lookup.Find(id);
            if (freight_lookup == null)
            {
                return HttpNotFound();
            }
            return View(freight_lookup);
        }

        //
        // POST: /Freight_Lookup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Freight_Lookup freight_lookup = db.Freight_Lookup.Find(id);
            db.Freight_Lookup.Remove(freight_lookup);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        private List<SelectListItem> LoadHazardous(string defaultValue)
        {
            var Hazardous = new List<SelectListItem>
                                    {new SelectListItem { Text = "Yes", Value = "Y ", Selected = (defaultValue == "Y ")},
                                            new SelectListItem {Text = "No", Value  = "N ", Selected = (defaultValue == "N ")}
                                        };
            return Hazardous.ToList();
        }

    }
}