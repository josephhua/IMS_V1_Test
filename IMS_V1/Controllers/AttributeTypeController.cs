using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class AttributeTypeController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /AttributeType/

        public ActionResult Index()
        {
            var attributes = from at in db.AttributeTypes
                             orderby at.AttributeType1
                             select at;

            return View(attributes.ToList());
        }

        //
        // GET: /AttributeType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /AttributeType/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "AttributeType_Id")]AttributeType attributetype)
        {
            if (ModelState.IsValid)
            {
                db.AttributeTypes.Add(attributetype);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                attributetype.ModifiedBy = userid;
                attributetype.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(attributetype);
        }

        //
        // GET: /AttributeType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AttributeType attributetype = db.AttributeTypes.Find(id);
            if (attributetype == null)
            {
                return HttpNotFound();
            }
            return View(attributetype);
        }

        //
        // POST: /AttributeType/Edit/5

        [HttpPost]
        public ActionResult Edit(AttributeType attributetype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(attributetype).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                attributetype.ModifiedBy = userid;
                attributetype.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(attributetype);
        }

        //
        // GET: /AttributeType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AttributeType attributetype = db.AttributeTypes.Find(id);
            if (attributetype == null)
            {
                return HttpNotFound();
            }
            return View(attributetype);
        }

        //
        // POST: /AttributeType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            AttributeType attributetype = db.AttributeTypes.Find(id);
            db.AttributeTypes.Remove(attributetype);
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