using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class CategoryAttributeController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /CategoryAttribute/

        public ActionResult Index()
        {
            var categoryattributes = db.CategoryAttributes.Include(c => c.AttributeType).Include(c => c.CategoryClass).Include(c => c.SubClass).Include(c => c.FineLineClass).OrderBy(c => c.CategoryClass_Id).ThenBy(c1 => c1.SubClass.SubClass_Id).ThenBy(c1 => c1.FineLineClass.FineLine_Id).ThenBy(c1 => c1.AttributeType);
            return View(categoryattributes.ToList());
        }

        //
        // GET: /CategoryAttribute/Details/5

        public ActionResult Details(int id = 0)
        {
            CategoryAttribute categoryattribute = db.CategoryAttributes.Find(id);
            if (categoryattribute == null)
            {
                return HttpNotFound();
            }
            return View(categoryattribute);
        }

        //
        // GET: /CategoryAttribute/Create

        public ActionResult Create()
        {
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1");
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description");
            var SubClass = db.SubClasses.Select(s => new
            {
                SubClassCode_Id = s.SubClassCode_Id,
                Description = s.SubClass_Id + " - " + s.SubClassName
            })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description");
            var FineLineClass = db.FineLineClasses.Select(f => new
            {
                FineLineCode_Id = f.FineLineCode_Id,
                Description = f.FineLine_Id + " - " + f.FinelineName
            })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description");

            //            ViewBag.CategoryClass_Id = new SelectList(db.CategoryClasses, "CategoryClass_Id", "Category_Id & CategoryName");
            return View();
        }

        //
        // POST: /CategoryAttribute/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "CategoryAttribute_Id")]CategoryAttribute categoryattribute)
        {
            if (ModelState.IsValid)
            {
                db.CategoryAttributes.Add(categoryattribute);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                categoryattribute.ModifiedBy = userid;
                categoryattribute.ModifiedDate = DateTime.Now;
                if (categoryattribute.SubClassCode_Id == 0)
                {
                    categoryattribute.SubClassCode_Id = null;
                }
                if (categoryattribute.FineLineCode_Id == 0)
                {
                    categoryattribute.FineLineCode_Id = null;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", categoryattribute.AttributeType_Id);
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", categoryattribute.CategoryClass_Id);
            var SubClass = db.SubClasses.Select(s => new
            {
                SubClassCode_Id = s.SubClassCode_Id,
                Description = s.SubClass_Id + " - " + s.SubClassName
            })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", categoryattribute.SubClassCode_Id);
            var FineLineClass = db.FineLineClasses.Select(f => new
            {
                FineLineCode_Id = f.FineLineCode_Id,
                Description = f.FineLine_Id + " - " + f.FinelineName
            })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", categoryattribute.FineLineCode_Id);


            //            ViewBag.CategoryClass_Id = new SelectList(db.CategoryClasses, "CategoryClass_Id", "Category_Id", categoryattribute.CategoryClass_Id);
            return View(categoryattribute);
        }

        //
        // GET: /CategoryAttribute/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CategoryAttribute categoryattribute = db.CategoryAttributes.Find(id);
            if (categoryattribute == null)
            {
                return HttpNotFound();
            }
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", categoryattribute.AttributeType_Id);
            //ViewBag.CategoryClass_Id = new SelectList(db.CategoryClasses, "CategoryClass_Id", "Category_Id", categoryattribute.CategoryClass_Id);

            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", categoryattribute.CategoryClass_Id);
            var CategoryClassId = db.CategoryClasses.Where(c1 => c1.CategoryClass_Id == categoryattribute.CategoryClass_Id)
                                    .Select(c1 => c1.Category_Id).FirstOrDefault();


            ViewBag.CategoryClassDisplay = db.CategoryClasses.Where(cc => cc.CategoryClass_Id == categoryattribute.CategoryClass_Id)
                                            .Select(cc => cc.Category_Id + "-" + cc.CategoryName).FirstOrDefault();
            var SubClass = db.SubClasses.Where(s => s.Category_Id == CategoryClassId)
                .Select(s => new
                {
                    SubClassCode_Id = s.SubClassCode_Id,
                    Description = s.SubClass_Id + " - " + s.SubClassName
                })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", categoryattribute.SubClassCode_Id);
            var SubClassId = db.SubClasses.Where(s1 => s1.SubClassCode_Id == categoryattribute.SubClassCode_Id)
                 .Select(s1 => s1.SubClass_Id).FirstOrDefault();
            ViewBag.SubClassDisplay = db.SubClasses.Where(sc => sc.SubClassCode_Id == categoryattribute.SubClassCode_Id)
                                            .Select(sc => sc.SubClass_Id + "-" + sc.SubClassName).FirstOrDefault();
            var FineLineClass = db.FineLineClasses.Where(f => f.Category_Id == CategoryClassId && f.SubClass_id == SubClassId)
                .Select(f => new
                {
                    FineLineCode_Id = f.FineLineCode_Id,
                    Description = f.FineLine_Id + " - " + f.FinelineName

                })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", categoryattribute.FineLineCode_Id);
            ViewBag.FineLineDisplay = db.FineLineClasses.Where(fl => fl.FineLineCode_Id == categoryattribute.FineLineCode_Id)
                                            .Select(fl => fl.FineLine_Id + "-" + fl.FinelineName).FirstOrDefault();







            return View(categoryattribute);
        }

        //
        // POST: /CategoryAttribute/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CategoryAttribute categoryattribute)
        {
            if (ModelState.IsValid)
            {
                db.Entry(categoryattribute).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                categoryattribute.ModifiedBy = userid;
                categoryattribute.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", categoryattribute.AttributeType_Id);
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", categoryattribute.CategoryClass_Id);
            var SubClass = db.SubClasses.Select(s => new
            {
                SubClassCode_Id = s.SubClassCode_Id,
                Description = s.SubClass_Id + " - " + s.SubClassName
            })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", categoryattribute.SubClassCode_Id);
            var FineLineClass = db.FineLineClasses.Select(f => new
            {
                FineLineCode_Id = f.FineLineCode_Id,
                Description = f.FineLine_Id + " - " + f.FinelineName
            })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", categoryattribute.FineLineCode_Id);

            //            ViewBag.CategoryClass_Id = new SelectList(db.CategoryClasses, "CategoryClass_Id", "Category_Id", categoryattribute.CategoryClass_Id);
            return View(categoryattribute);
        }

        //
        // GET: /CategoryAttribute/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CategoryAttribute categoryattribute = db.CategoryAttributes.Find(id);
            if (categoryattribute == null)
            {
                return HttpNotFound();
            }
            return View(categoryattribute);
        }

        //
        // POST: /CategoryAttribute/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            CategoryAttribute categoryattribute = db.CategoryAttributes.Find(id);
            db.CategoryAttributes.Remove(categoryattribute);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadSubClassesByCategoryId(int categoryclassid)
        {

            var SubClassList = this.GetSubClasses(categoryclassid);
            var SubClassData = SubClassList.Select(s => new
            {
                Text = s.SubClass_Id + " - " + s.SubClassName,
                Value = s.SubClassCode_Id,
            });
            return Json(SubClassData, JsonRequestBehavior.AllowGet);

        }

        private IList<SubClass> GetSubClasses(int categoryclassid)
        {
            return (from s in db.SubClasses
                    join c in db.CategoryClasses on s.Category_Id equals c.Category_Id
                    where c.CategoryClass_Id == categoryclassid
                    select s).ToList();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadFineLineClassesByCateogryIdSubClassId(int categoryclassid, int subclasscodeid)
        {
            var FineLineList = this.GetFineLineClasses(categoryclassid, subclasscodeid);
            var FineLineData = FineLineList.Select(f => new
            {
                Text = f.FineLine_Id + " - " + f.FinelineName,
                Value = f.FineLineCode_Id,
            });
            return Json(FineLineData, JsonRequestBehavior.AllowGet);
        }

        //        private IList<FineLineClass> GetFineLineClasses(string categoryid, string subclassid)
        private IList<FineLineClass> GetFineLineClasses(int categoryclassid, int subclasscodeid)
        {
            return (from f in db.FineLineClasses
                    join c in db.CategoryClasses on f.Category_Id equals c.Category_Id
                    join s in db.SubClasses on f.SubClass_id equals s.SubClass_Id
                    where c.CategoryClass_Id == categoryclassid && s.SubClassCode_Id == subclasscodeid
                    select f).ToList();
        }

    }
}