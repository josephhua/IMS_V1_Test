using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class ABC_LookupController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /ABC_Lookup/

        public ActionResult Index()
        {
            return View(db.ABC_Lookup.ToList());
        }

        //
        // GET: /ABC_Lookup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ABC_Lookup/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "ABC_Id")]ABC_Lookup abc_lookup)
        {
            if (ModelState.IsValid)
            {
                db.ABC_Lookup.Add(abc_lookup);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                abc_lookup.ModifiedBy = userid;
                abc_lookup.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(abc_lookup);
        }

        //
        // GET: /ABC_Lookup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ABC_Lookup abc_lookup = db.ABC_Lookup.Find(id);
            if (abc_lookup == null)
            {
                return HttpNotFound();
            }
            return View(abc_lookup);
        }

        //
        // POST: /ABC_Lookup/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ABC_Lookup abc_lookup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(abc_lookup).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                abc_lookup.ModifiedBy = userid;
                abc_lookup.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(abc_lookup);
        }

        //
        // GET: /ABC_Lookup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ABC_Lookup abc_lookup = db.ABC_Lookup.Find(id);
            if (abc_lookup == null)
            {
                return HttpNotFound();
            }
            return View(abc_lookup);
        }

        //
        // POST: /ABC_Lookup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ABC_Lookup abc_lookup = db.ABC_Lookup.Find(id);
            db.ABC_Lookup.Remove(abc_lookup);
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