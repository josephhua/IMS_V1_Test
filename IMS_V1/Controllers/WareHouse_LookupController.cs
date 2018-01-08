using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class WareHouse_LookupController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /WareHouse_Lookup/

        public ActionResult Index()
        {
            return View(db.WareHouse_Lookup.ToList());
        }

        //
        // GET: /WareHouse_Lookup/Details/5

        public ActionResult Details(int id = 0)
        {
            WareHouse_Lookup warehouse_lookup = db.WareHouse_Lookup.Find(id);
            if (warehouse_lookup == null)
            {
                return HttpNotFound();
            }
            return View(warehouse_lookup);
        }

        //
        // GET: /WareHouse_Lookup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /WareHouse_Lookup/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "WareHouse_id")]WareHouse_Lookup warehouse_lookup)
        {
            if (ModelState.IsValid)
            {
                db.WareHouse_Lookup.Add(warehouse_lookup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(warehouse_lookup);
        }

        //
        // GET: /WareHouse_Lookup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            WareHouse_Lookup warehouse_lookup = db.WareHouse_Lookup.Find(id);
            if (warehouse_lookup == null)
            {
                return HttpNotFound();
            }
            return View(warehouse_lookup);
        }

        //
        // POST: /WareHouse_Lookup/Edit/5

        [HttpPost]
        public ActionResult Edit(WareHouse_Lookup warehouse_lookup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(warehouse_lookup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(warehouse_lookup);
        }

        //
        // GET: /WareHouse_Lookup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            WareHouse_Lookup warehouse_lookup = db.WareHouse_Lookup.Find(id);
            if (warehouse_lookup == null)
            {
                return HttpNotFound();
            }
            return View(warehouse_lookup);
        }

        //
        // POST: /WareHouse_Lookup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            WareHouse_Lookup warehouse_lookup = db.WareHouse_Lookup.Find(id);
            db.WareHouse_Lookup.Remove(warehouse_lookup);
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