using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class Attribute_LookupController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /Attribute_Lookup/

        public ActionResult Index(int id)
        {
            var attributelookup = db.Attribute_Lookup.Where(al => al.AttributeType_Id == id).OrderByDescending(al => al.Active).ThenBy(al => al.WebsiteAttributeValue);
            ViewBag.AttributeType = db.AttributeTypes.Where(at => at.AttributeType_Id == id).Select(at => at.AttributeType1).Single();
            ViewBag.AttributeType_Id = id;

            return View(attributelookup.ToList());
        }

        //
        // GET: /Attribute_Lookup/Details/5

        public ActionResult Details(int id = 0)
        {
            Attribute_Lookup attribute_lookup = db.Attribute_Lookup.Find(id);
            if (attribute_lookup == null)
            {
                return HttpNotFound();
            }
            return View(attribute_lookup);
        }

        //
        // GET: /Attribute_Lookup/Create

        public ActionResult Create(int id)
        {
            ViewBag.AttributeType_Id = id;

            ViewBag.AttributeType = db.AttributeTypes.Where(at => at.AttributeType_Id == id).Select(at => at.AttributeType1).Single();
            return View();
        }

        //
        // POST: /Attribute_Lookup/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "AttributeLookup_Id")]Attribute_Lookup attribute_lookup)
        {
            if (ModelState.IsValid)
            {
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                db.Attribute_Lookup.Add(attribute_lookup);
                attribute_lookup.AttributeType_Id = attribute_lookup.AttributeType_Id;
                attribute_lookup.ModifiedBy = userid;
                attribute_lookup.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                int attributetype_id = attribute_lookup.AttributeType_Id;//db.ItemAttributes.Where(ia => ia.ItemAttribute_Id == id).Select(ia => ia.Item_Id).Single();
                var attributelookup = db.Attribute_Lookup.Where(al => al.AttributeType_Id == attributetype_id && al.Active == true).OrderBy(al => al.APlusAttributeValue);
                ViewBag.AttributeType = db.AttributeTypes.Where(at => at.AttributeType_Id == attributetype_id).Select(at => at.AttributeType1).Single();
                ViewBag.AttributeType_Id = attributetype_id;

                return RedirectToAction("Index", new { id = attribute_lookup.AttributeType_Id });
            }

            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", attribute_lookup.AttributeType_Id);
            return View(attribute_lookup);
        }

        //
        // GET: /Attribute_Lookup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Attribute_Lookup attribute_lookup = db.Attribute_Lookup.Find(id);
            if (attribute_lookup == null)
            {
                return HttpNotFound();
            }
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", attribute_lookup.AttributeType_Id);
            int attributetypeid = attribute_lookup.AttributeType_Id;
            ViewBag.AttributeType = db.AttributeTypes.Where(at => at.AttributeType_Id == attributetypeid).Select(at => at.AttributeType1).Single();
            return View(attribute_lookup);
        }

        //
        // POST: /Attribute_Lookup/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Attribute_Lookup attribute_lookup)
        {
            if (ModelState.IsValid)
            {
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                db.Attribute_Lookup.Add(attribute_lookup);
                attribute_lookup.ModifiedBy = userid;
                attribute_lookup.ModifiedDate = DateTime.Now;
                db.Entry(attribute_lookup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = attribute_lookup.AttributeType_Id });
            }
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", attribute_lookup.AttributeType_Id);
            return View(attribute_lookup);
        }

        //
        // GET: /Attribute_Lookup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Attribute_Lookup attribute_lookup = db.Attribute_Lookup.Find(id);
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", attribute_lookup.AttributeType_Id);
            int attributetypeid = attribute_lookup.AttributeType_Id;
            ViewBag.AttributeType = db.AttributeTypes.Where(at => at.AttributeType_Id == attributetypeid).Select(at => at.AttributeType1).Single();
            if (attribute_lookup == null)
            {
                return HttpNotFound();
            }
            return View(attribute_lookup);
        }

        //
        // POST: /Attribute_Lookup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Attribute_Lookup attribute_lookup = db.Attribute_Lookup.Find(id);
            db.Attribute_Lookup.Remove(attribute_lookup);
            db.SaveChanges();
            return RedirectToAction("Index", new { id = attribute_lookup.AttributeType_Id });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}