using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class ReplacementItemController : Controller
    {
        //
        // GET: /ReplacementItem/
        private IMS_V1Entities db = new IMS_V1Entities();

        public ActionResult Index(int id)
        {
            ViewBag.ItemId = id;
            var replacementitems = db.ReplacementItems.Where(r => r.ItmId == id);
            var replacementItemCodes = db.ReplacementItemCodes;
            foreach (var rItem in replacementitems)
                rItem.replacementCode = rItem.replacementCode + " - " + replacementItemCodes.Where(c => c.replacementCode == rItem.replacementCode).FirstOrDefault().replacementCodeDesc;
            return View(replacementitems.ToList());
        }

        public ActionResult Create()
        {
            string url = Request.Url.ToString();
            int indexl = url.LastIndexOf("Create");
            int ReplacementItemId = Int32.Parse(url.Substring(indexl + 7));
            ReplacementItem replaceItem = new ReplacementItem();
            replaceItem.ItmId = ReplacementItemId;
            ViewBag.ReplacementItemId = ReplacementItemId;
            ViewBag.ReplacementCodes = new SelectList(db.ReplacementItemCodes, "replacementCode", "replacementCodeDesc");
            replaceItem.replacementCreateDate = DateTime.Now;
            return View(replaceItem);
        }

        [HttpPost] //, ActionName("_Create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReplacementItem replaceItem)
        {
            if (replaceItem.existingItm_Num == null || replaceItem.existingItm_Num.Length == 0)
                ModelState.AddModelError("existingItm_Num", "Please enter replacement item number");
            else if (replaceItem.existingItm_Num.Length != 7)
                ModelState.AddModelError("existingItm_Num", "Replacement item number must be 7 digits.");
            if (replaceItem.replacementCode == null)
                ModelState.AddModelError("replacementCode", "Please select replacement code.");
            if (ModelState.IsValid)
            {
                replaceItem.replacementCreateDate = DateTime.Now;
                db.ReplacementItems.Add(replaceItem);
                db.SaveChanges();
                return RedirectToAction("Index", "ReplacementItem", new { id = replaceItem.ItmId }); // View("Index", new { id = replaceItem.ItmId });
            }
            else
            {
                ViewBag.ReplacementCodes = new SelectList(db.ReplacementItemCodes, "replacementCode", "replacementCodeDesc");
                return View(replaceItem);
            }
        }

        public ActionResult Edit()
        {
            string url = Request.Url.ToString();
            int indexl = url.LastIndexOf("Edit");
            int ReplacementId = Int32.Parse(url.Substring(indexl + 5));
            ReplacementItem replaceItem = db.ReplacementItems.Where(i => i.id == ReplacementId).FirstOrDefault();
            replaceItem.existingItm_Num = replaceItem.existingItm_Num.Trim();
            ViewBag.ReplacementCodes = new SelectList(db.ReplacementItemCodes, "replacementCode", "replacementCodeDesc");
            return View(replaceItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReplacementItem replaceItem)
        {
            if (replaceItem.existingItm_Num == null || replaceItem.existingItm_Num.Length == 0)
                ModelState.AddModelError("existingItm_Num", "Please enter replacement item number");
            else if (replaceItem.existingItm_Num.Trim().Length != 7)
                ModelState.AddModelError("existingItm_Num", "Replacement item number must be 7 digits.");
            if (replaceItem.replacementCode == null)
                ModelState.AddModelError("replacementCode", "Please select replacement code.");
            if (ModelState.IsValid)
            {
                ReplacementItem replaceItemOld = db.ReplacementItems.Where(i => i.id == replaceItem.id).FirstOrDefault();
                replaceItemOld.existingItm_Num = replaceItem.existingItm_Num;
                replaceItemOld.replacementCode = replaceItem.replacementCode;
                replaceItem.replacementCreateDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index", "ReplacementItem", new { id = replaceItem.ItmId }); // View("Index", new { id = replaceItem.ItmId });
            }
            else
            {
                ViewBag.ReplacementCodes = new SelectList(db.ReplacementItemCodes, "replacementCode", "replacementCodeDesc");
                return View(replaceItem);
            }
        }

        public ActionResult Delete()
        {
            string url = Request.Url.ToString();
            int indexl = url.LastIndexOf("Delete");
            int ReplacementId = Int32.Parse(url.Substring(indexl + 7));
            ReplacementItem replaceItem = db.ReplacementItems.Where(i => i.id == ReplacementId).FirstOrDefault();
            var replacementItemCodes = db.ReplacementItemCodes;
            replaceItem.replacementCode = replaceItem.replacementCode + " - " + replacementItemCodes.Where(c => c.replacementCode == replaceItem.replacementCode).FirstOrDefault().replacementCodeDesc;
            return View(replaceItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReplacementItem replaceItem)
        {
            ReplacementItem item = db.ReplacementItems.Find(replaceItem.id);
            db.ReplacementItems.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index", "ReplacementItem", new { id = replaceItem.ItmId });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}