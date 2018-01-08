using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class ItemDescriptionOrderController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /ItemDescriptionOrder/

        public ActionResult Index()
        {
            var itemdescriptionorder = db.ItemDescriptionOrders.Include(i => i.AttributeType).Include(i => i.CategoryClass).Include(i => i.SubClass).Include(i => i.FineLineClass).OrderBy(i => i.CategoryClass_Id).ThenBy(i => i.SubClass.SubClass_Id).ThenBy(i => i.FineLineClass.FineLine_Id).ThenBy(i => i.DescriptionField).ThenBy(i => i.OrderNumber);
            return View(itemdescriptionorder.ToList());
        }

        //
        // GET: /ItemDescriptionOrder/Details/5

        public ActionResult Details(int id = 0)
        {
            ItemDescriptionOrder itemdescriptionorder = db.ItemDescriptionOrders.Find(id);
            if (itemdescriptionorder == null)
            {
                return HttpNotFound();
            }
            return View(itemdescriptionorder);
        }

        //
        // GET: /ItemDescriptionOrder/Create

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
            ViewBag.SystemType = LoadSystemType("");
            ViewBag.DescriptionField = LoadDescription("");


            return View();
        }

        //
        // POST: /ItemDescriptionOrder/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "ItemDescriptionOrder_Id")]ItemDescriptionOrder itemdescriptionorder)
        {
            if (ModelState.IsValid)
            {
                db.ItemDescriptionOrders.Add(itemdescriptionorder);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                itemdescriptionorder.ModifiedBy = userid;
                itemdescriptionorder.ModifiedDate = DateTime.Now;
                if (itemdescriptionorder.SubClassCode_Id == 0)
                {
                    itemdescriptionorder.SubClassCode_Id = null;
                }
                if (itemdescriptionorder.FineLineCode_Id == 0)
                {
                    itemdescriptionorder.FineLineCode_Id = null;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
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
            ViewBag.SystemType = LoadSystemType("");
            ViewBag.DescriptionField = LoadDescription("");
            return View(itemdescriptionorder);
        }

        //
        // GET: /ItemDescriptionOrder/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ItemDescriptionOrder itemdescriptionorder = db.ItemDescriptionOrders.Find(id);
            if (itemdescriptionorder == null)
            {
                return HttpNotFound();
            }
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", itemdescriptionorder.AttributeType_Id);
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", itemdescriptionorder.CategoryClass_Id);
            var SubClass = db.SubClasses.Select(s => new
            {
                SubClassCode_Id = s.SubClassCode_Id,
                Description = s.SubClass_Id + " - " + s.SubClassName
            })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", itemdescriptionorder.SubClassCode_Id);
            var FineLineClass = db.FineLineClasses.Select(f => new
            {
                FineLineCode_Id = f.FineLineCode_Id,
                Description = f.FineLine_Id + " - " + f.FinelineName
            })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", itemdescriptionorder.FineLineCode_Id);
            ViewBag.SystemType = LoadSystemType(itemdescriptionorder.SystemType.Trim());
            ViewBag.DescriptionField = LoadDescription(itemdescriptionorder.DescriptionField);

            return View(itemdescriptionorder);
        }

        //
        // POST: /ItemDescriptionOrder/Edit/5

        [HttpPost]
        public ActionResult Edit(ItemDescriptionOrder itemdescriptionorder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(itemdescriptionorder).State = EntityState.Modified;
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                itemdescriptionorder.ModifiedBy = userid;
                itemdescriptionorder.ModifiedDate = DateTime.Now;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.AttributeType_Id = new SelectList(db.AttributeTypes, "AttributeType_Id", "AttributeType1", itemdescriptionorder.AttributeType_Id);
            var CategoryClass = db.CategoryClasses.Select(c => new
            {
                CategoryClass_Id = c.CategoryClass_Id,
                Description = c.Category_Id + " - " + c.CategoryName
            })
                                                    .ToList();
            ViewBag.CategoryClass = new SelectList(CategoryClass, "CategoryClass_Id", "Description", itemdescriptionorder.CategoryClass_Id);
            var SubClass = db.SubClasses.Select(s => new
            {
                SubClassCode_Id = s.SubClassCode_Id,
                Description = s.SubClass_Id + " - " + s.SubClassName
            })
                                                    .ToList();
            ViewBag.SubClass = new SelectList(SubClass, "SubClassCode_Id", "Description", itemdescriptionorder.SubClassCode_Id);
            var FineLineClass = db.FineLineClasses.Select(f => new
            {
                FineLineCode_Id = f.FineLineCode_Id,
                Description = f.FineLine_Id + " - " + f.FinelineName
            })
                                                    .ToList();
            ViewBag.FineLineClass = new SelectList(FineLineClass, "FineLineCode_Id", "Description", itemdescriptionorder.FineLineCode_Id);
            ViewBag.SystemType = LoadSystemType(itemdescriptionorder.SystemType);
            ViewBag.DescriptionField = LoadDescription(itemdescriptionorder.DescriptionField);

            return View(itemdescriptionorder);
        }

        //
        // GET: /ItemDescriptionOrder/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ItemDescriptionOrder itemdescriptionorder = db.ItemDescriptionOrders.Find(id);
            if (itemdescriptionorder == null)
            {
                return HttpNotFound();
            }
            return View(itemdescriptionorder);
        }

        //
        // POST: /ItemDescriptionOrder/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ItemDescriptionOrder itemdescriptionorder = db.ItemDescriptionOrders.Find(id);
            db.ItemDescriptionOrders.Remove(itemdescriptionorder);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private List<SelectListItem> LoadDescription(string defaultValue)
        {
            var Description = new List<SelectListItem>
                                    {new SelectListItem { Text = "Desc1", Value = "Desc1", Selected = (defaultValue == "Desc1")},
                                            new SelectListItem {Text = "Desc2", Value  = "Desc2", Selected = (defaultValue == "Desc2")},
                                            new SelectListItem {Text = "Web", Value  = "Web", Selected = (defaultValue == "Web")}
                                        };
            return Description.ToList();
        }

        private List<SelectListItem> LoadSystemType(string defaultValue)
        {
            var SystemType = new List<SelectListItem>
                                    {new SelectListItem { Text = "Actual", Value = "Actual", Selected = (defaultValue == "Actual")},
                                            new SelectListItem {Text = "APlus", Value  = "APlus", Selected = (defaultValue == "APlus")},
                                            new SelectListItem {Text = "Web", Value  = "Web", Selected = (defaultValue == "Web")}
                                        };
            return SystemType.ToList();
        }

    }
}