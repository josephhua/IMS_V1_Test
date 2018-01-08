using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome " + Session.Contents["LogedUserFullName"] + " to the Item Management System.";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contacts";

            return View();
        }

        public ActionResult Login(int Login = 0, string ErrorMessage = "")
        {
            ViewBag.LoginError = ErrorMessage;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User u)
        {
            // this action is for handle post (login)
            if (ModelState.IsValid) // this is check validity
            {
                using (IMS_V1Entities db = new IMS_V1Entities())
                {
                    var v = db.Users.Where(a => a.UserName.Equals(u.UserName)).FirstOrDefault(); // && a.Password.Equals(u.Password)).FirstOrDefault();
                    if (v != null)
                    {
                        byte[] enPwd = GetSHA1(v.UserName, u.Password);
                        if (MatchSHA1(enPwd, v.EncryptPwd) && (v.Active != null && v.Active.Value))
                        {
                            Session.Add("UserID", v.User_id);
                            Session.Add("UserTypeID", v.UserType_Id);
                            Session.Add("LogedUserFullName", v.FirstName.ToString() + " " + v.LastName.ToString());
                            Session.Add("Logout", "false");
                            //Session.Add("CreateAPlusImport", v.CreateAPlusImport_MarineShooting);
                            int usertypeid = int.Parse(Session.Contents["UserTypeId"].ToString());
                            if (usertypeid == 2)
                            {
                                return RedirectToAction("Index", "Item");
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            Session.Add("LogedUserFullName", "");
                            Session.Add("Logout", "true");
                            return RedirectToAction("Login", new { Login = 1, ErrorMessage = "UserName or Password is incorrect.  Please try again." });
                        }
                    }
                    else
                    {
                        Session.Add("LogedUserFullName", "");
                        Session.Add("Logout", "true");
                        return RedirectToAction("Login", new { Login = 1, ErrorMessage = "UserName or Password is incorrect.  Please try again." });
                    }

                }
            }
            return View(u);
        }

        private byte[] GetSHA1(string userID, string password)
        {
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(userID + password));
        }

        protected bool MatchSHA1(byte[] p1, byte[] p2)
        {
            bool result = false;
            if (p1 != null && p2 != null)
            {
                if (p1.Length == p2.Length)
                {
                    result = true;
                    for (int i = 0; i < p1.Length; i++)
                    {
                        if (p1[i] != p2[i])
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public ActionResult Logout()
        {
            ViewBag.Message = "Good Bye " + Session.Contents["LogedUserFullname"] + ". You have been logged out.";
            Session.Add("Logout", "true");
            Session.Add("UserTypeID", null);
            Session.Add("UserID", null);
            Session.Abandon();
            return RedirectToAction("Login");
            //return View();
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveUpload(HttpPostedFileBase file)
        {

            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/Uploads"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }

            return View();
        }

        public ActionResult DownloadExcel()
        {
            return View();
        }

        public FileResult DownLoadLongGunSheet()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\inetpub\wwwroot\IMSProd\SpreadFiles\LongGunWorkbook082017r8.xlsm");
            string fileName = "LongGunWorkbook082017r8.xlsm";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult DownLoadHandGunSheet()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\inetpub\wwwroot\IMSProd\SpreadFiles\HandGunWorkbook082017r8.xlsm");
            string fileName = "HandGunWorkbook082017r8.xlsm";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult DownLoadAmmoSheet()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\inetpub\wwwroot\IMSProd\SpreadFiles\AmmoWorkbook082017r9.xlsm");
            string fileName = "AmmoWorkbook082017r9.xlsm";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult DownLoadIMSSheet()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\inetpub\wwwroot\IMSProd\SpreadFiles\ImsWorkbook082017r8.xlsm");
            string fileName = "ImsWorkbook082017r8.xlsm";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult DownLoadMarineSheet()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\inetpub\wwwroot\IMSProd\SpreadFiles\MarineWorkbook082017r82.xlsm");
            string fileName = "MarineWorkbook082017r82.xlsm";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult DownLoadNFASheet()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\inetpub\wwwroot\IMSProd\SpreadFiles\NFAWorkbook092017r1.xlsm");
            string fileName = "NFAWorkbook092017r1.xlsm";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}