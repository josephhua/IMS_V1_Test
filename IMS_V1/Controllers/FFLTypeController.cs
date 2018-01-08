using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class FFLTypeController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /FFLType/

        public ActionResult Index()
        {
            return View(db.FFLTypes.ToList());
        }

        //
        // GET: /FFLType/Details/5

        public ActionResult Details(int id = 0)
        {
            FFLType ffltype = db.FFLTypes.Find(id);
            if (ffltype == null)
            {
                return HttpNotFound();
            }
            return View(ffltype);
        }

        //
        // GET: /FFLType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /FFLType/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "FFLType_Id")]FFLType ffltype)
        {
            if (ModelState.IsValid)
            {
                db.FFLTypes.Add(ffltype);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                ffltype.ModifiedBy = userid;
                ffltype.ModifiedDate = DateTime.Now;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ffltype);
        }

        //
        // GET: /FFLType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            FFLType ffltype = db.FFLTypes.Find(id);
            if (ffltype == null)
            {
                return HttpNotFound();
            }
            return View(ffltype);
        }

        //
        // POST: /FFLType/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FFLType ffltype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ffltype).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                ffltype.ModifiedBy = userid;
                ffltype.ModifiedDate = DateTime.Now;
                return RedirectToAction("Index");
            }
            return View(ffltype);
        }

        //
        // GET: /FFLType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            FFLType ffltype = db.FFLTypes.Find(id);
            if (ffltype == null)
            {
                return HttpNotFound();
            }
            return View(ffltype);
        }

        //
        // POST: /FFLType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            FFLType ffltype = db.FFLTypes.Find(id);
            db.FFLTypes.Remove(ffltype);
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