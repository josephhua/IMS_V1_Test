using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
	public class UploadFileController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();
        public ActionResult Index(string currentFilter, string SearchString, int? page, string orderType)
        {
            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = currentFilter;
            }
            int userid = int.Parse(Session.Contents["UserId"].ToString());
            int usertypeid = int.Parse(Session.Contents["UserTypeId"].ToString());
            ViewBag.userid = userid;
            ViewBag.CurrentFileFilter = SearchString;
            var uploadFiles = from r in
                            db.UploadFiles
                              select r;
            
            if (!String.IsNullOrEmpty(SearchString))
            {
                var user = db.Users.Where(u => u.FirstName.ToUpper().Contains(SearchString.ToUpper()) || u.LastName.ToUpper().Contains(SearchString.ToUpper())).FirstOrDefault();
                if (user != null)
                    uploadFiles = uploadFiles.Where(r => r.FileName.ToUpper().Contains(SearchString.ToUpper()) || r.UploadNote.ToUpper().Contains(SearchString.ToUpper()) || r.UserId == user.User_id);
                else
                    uploadFiles = uploadFiles.Where(r => r.FileName.ToUpper().Contains(SearchString.ToUpper()) || r.UploadNote.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (usertypeid == 6)
            {
                uploadFiles = uploadFiles.Where(r => r.UserId == userid);
                if (!String.IsNullOrEmpty(SearchString))
                {
                    uploadFiles = uploadFiles.Where(r => r.FileName.ToUpper().Contains(SearchString.ToUpper()) || r.UploadNote.ToUpper().Contains(SearchString.ToUpper()));
                }
            }
            foreach (var u in uploadFiles)
            {
                u.UserName = GetUserFullName(u.UserId);
            }
            int pageSize = 30;
            int pageNumber = (page ?? 1);
            string OT = "MR";
            ViewBag.OrderType = OT;
            return View(uploadFiles.OrderBy(i => i.File_id).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveUpload(UploadFile model, HttpPostedFileBase file)
        {
            List<string> fTypes = GetAllowedFileTypes();
            if (FileIsValid(file, fTypes))
            {            
                int userid = int.Parse(Session.Contents["UserId"].ToString());
                int usertypeid = int.Parse(Session.Contents["UserTypeId"].ToString());
                string path0 = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path0))
                {
                    Directory.CreateDirectory(path0);
                }
                string path = Server.MapPath("~/Uploads/" + userid.ToString() + "/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileAndPath = Path.Combine(Server.MapPath("~/Uploads/"), userid.ToString()+ "/"+ Path.GetFileName(file.FileName));
                if (System.IO.File.Exists(fileAndPath))
                    System.IO.File.Delete(fileAndPath);
                file.SaveAs(path + Path.GetFileName(file.FileName));
                UploadFile m = new UploadFile();
                m.UserId = userid;
                if (model.UploadNote != null && model.UploadNote.Length > 255)
                    m.UploadNote = model.UploadNote.Substring(0, 254);
                else
                    m.UploadNote = model.UploadNote;
                m.FileName = Path.GetFileName(file.FileName);
                m.LoadDate = DateTime.Now;
                db.UploadFiles.Add(m);
                db.SaveChanges();
                var user = db.Users.Where(u => u.User_id == m.UserId).FirstOrDefault();
                if (user != null)
                    m.UserName = user.FirstName + " " + user.LastName;
                SendEmailto(m);
                return RedirectToAction("Index");
            }   
            else
            {
                string errorMessage = GetFileErrorMessage(file, fTypes);
                ModelState.AddModelError("FileName", errorMessage);
                return View("Upload");
            }
                
        }

        public FileResult DownloadFile(int fid)
        {
            var m = db.UploadFiles.Where(f => f.File_id == fid).FirstOrDefault();
            if (m != null)
            {
                string fileName = m.FileName;
                byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\Users\josephh\source\repos\IMS_V1_Test\IMS_V1\Uploads\" + m.UserId.ToString() +"\\" + m.FileName); //intranet
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else
                return null;           
        }

        protected void SendEmailto(UploadFile m)
        {
            List<emailList> list = db.emailLists.Where(e => e.active == true).ToList();
            MailMessage mail = new MailMessage();
            foreach(var e in list)
            {
                mail.To.Add(e.email);
            }
            mail.From = new MailAddress("CCUSC@unitedsporting.com");
            mail.Subject = "File Uploaded to IMS";
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString("The file " + m.FileName + " has been uploaded to IMS by " + m.UserName +". \n" +
                "The upload note is '" + m.UploadNote + "'.");
            mail.AlternateViews.Add(htmlView);
            SmtpClient smtp = new SmtpClient("10.2.101.58");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = true;
            smtp.EnableSsl = false;
            smtp.Send(mail);
        }

        protected bool FileIsValid(HttpPostedFileBase postedFile, List<string> fTypes)
        {
            if (postedFile != null && postedFile.FileName != null && postedFile.FileName.Length != 0)
                return fTypes.Contains(Path.GetExtension(postedFile.FileName.ToLower()));
            else
                return false;
        }

        protected string GetFileErrorMessage(HttpPostedFileBase postedFile, List<string> fTypes)
        {
            string msg = "";
            if (postedFile == null || postedFile.FileName == null && postedFile.FileName.Length == 0)
                msg = "Please select a file";
            else if (!fTypes.Contains(Path.GetExtension(postedFile.FileName.ToLower())))
                msg = "Please select a valid file type. Valid types: xls, jpg, jpeg, png, doc, xlsx, txt, docx, pdf, tif";
            return msg;
        }

        protected List<string> GetAllowedFileTypes()
        {
            List<string> aList = new List<string>();
            aList.Add(".xls");
            aList.Add(".jpg");
            aList.Add(".jpeg");
            //aList.Add(".gif");
            aList.Add(".png");
            aList.Add(".doc");
            aList.Add(".xlsx");
            aList.Add(".txt");
            aList.Add(".docx");
            aList.Add(".pdf");
            aList.Add(".tif");
            return aList;
        }

        protected string GetUserFullName(int uid)
        {
            User u = db.Users.Where(ur => ur.User_id == uid).FirstOrDefault();
            if (u != null)
                return u.FirstName + " " + u.LastName;
            else
                return " ";
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}