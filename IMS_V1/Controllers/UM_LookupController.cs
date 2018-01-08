using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class UM_LookupController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /UM_Lookup/

        public ActionResult Index()
        {
            return View(db.UM_Lookup.ToList());
        }

        //
        // GET: /UM_Lookup/Details/5

        public ActionResult Details(int id = 0)
        {
            UM_Lookup um_lookup = db.UM_Lookup.Find(id);
            if (um_lookup == null)
            {
                return HttpNotFound();
            }
            return View(um_lookup);
        }

        //
        // GET: /UM_Lookup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /UM_Lookup/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "UM_Id")]UM_Lookup um_lookup)
        {
            if (ModelState.IsValid)
            {
                db.UM_Lookup.Add(um_lookup);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                um_lookup.ModifiedBy = userid;
                um_lookup.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(um_lookup);
        }

        //
        // GET: /UM_Lookup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UM_Lookup um_lookup = db.UM_Lookup.Find(id);
            if (um_lookup == null)
            {
                return HttpNotFound();
            }
            return View(um_lookup);
        }

        //
        // POST: /UM_Lookup/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UM_Lookup um_lookup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(um_lookup).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                um_lookup.ModifiedBy = userid;
                um_lookup.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(um_lookup);
        }

        //
        // GET: /UM_Lookup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UM_Lookup um_lookup = db.UM_Lookup.Find(id);
            if (um_lookup == null)
            {
                return HttpNotFound();
            }
            return View(um_lookup);
        }

        //
        // POST: /UM_Lookup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            UM_Lookup um_lookup = db.UM_Lookup.Find(id);
            db.UM_Lookup.Remove(um_lookup);
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