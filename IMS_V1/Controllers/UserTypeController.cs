using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class UserTypeController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /UserType/

        public ActionResult Index()
        {
            return View(db.UserTypes.ToList());
        }

        //
        // GET: /UserType/Details/5

        public ActionResult Details(int id = 0)
        {
            UserType usertype = db.UserTypes.Find(id);
            if (usertype == null)
            {
                return HttpNotFound();
            }
            return View(usertype);
        }

        //
        // GET: /UserType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /UserType/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "UserType_id")]UserType usertype)
        {
            if (ModelState.IsValid)
            {
                db.UserTypes.Add(usertype);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                usertype.ModifiedBy = userid;
                usertype.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(usertype);
        }

        //
        // GET: /UserType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UserType usertype = db.UserTypes.Find(id);
            if (usertype == null)
            {
                return HttpNotFound();
            }
            return View(usertype);
        }

        //
        // POST: /UserType/Edit/5

        [HttpPost]
        public ActionResult Edit(UserType usertype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(usertype).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                usertype.ModifiedBy = userid;
                usertype.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(usertype);
        }

        //
        // GET: /UserType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UserType usertype = db.UserTypes.Find(id);
            if (usertype == null)
            {
                return HttpNotFound();
            }
            return View(usertype);
        }

        //
        // POST: /UserType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            UserType usertype = db.UserTypes.Find(id);
            db.UserTypes.Remove(usertype);
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