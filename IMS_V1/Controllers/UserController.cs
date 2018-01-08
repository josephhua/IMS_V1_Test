using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace IMS_V1.Controllers
{
    public class UserController : Controller
    {
        private IMS_V1Entities db = new IMS_V1Entities();

        //
        // GET: /User/

        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.UserType);
            return View(users.ToList());
        }

        //
        // GET: /User/Details/5

        public ActionResult Details(int id = 0)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            ViewBag.UserType_Id = new SelectList(db.UserTypes, "UserType_id", "UserTypeDescription");
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "User_Id")]User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                int userid = int.Parse(Session.Contents["UserID"].ToString());
                user.ModifiedBy = userid;
                user.ModifiedDate = DateTime.Now;
                user.EncryptPwd = GetSHA1(user.UserName, user.Password);
                user.Password = RandomString(16);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserType_Id = new SelectList(db.UserTypes, "UserType_id", "UserTypeDescription", user.UserType_Id);
            return View(user);
        }

        //
        // GET: /User/Edit/5

        public ActionResult Edit(int id = 0)
        {
            if (id == 0) id = int.Parse(Session["UserID"].ToString());
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            int userTypeId = int.Parse(Session.Contents["UserTypeID"].ToString());
            if (userTypeId != 1)
                user.Password = "";

            ViewBag.UserType_Id = new SelectList(db.UserTypes, "UserType_id", "UserTypeDescription", user.UserType_Id);
            ViewBag.UserId = Session["UserID"].ToString();
            return View(user);
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            User userOld = db.Users.Find(user.User_id);
            string UserId = Request.Form["UserId"];
            int loggedInUserId = 0;
            if (UserId != null)
                loggedInUserId = int.Parse(UserId);
            if (user.ResetPassword != null && user.ResetPassword.Value && !ValidatePassword(user.Password))
            {
                ModelState.AddModelError("Password", "Password has to be at least 4 characters.");
            }

            if (ModelState.IsValid)
            {
                User userToSave = db.Users.Find(user.User_id);
                int userTypeId = int.Parse(Session.Contents["UserTypeID"].ToString());
                if (user.UserType_Id == null)
                {
                    userToSave.UserType_Id = userTypeId;
                }
                else
                    userToSave.UserType_Id = user.UserType_Id;

                userToSave.ModifiedBy = loggedInUserId;
                userToSave.ModifiedDate = DateTime.Now;
                if (user.ResetPassword != null && user.ResetPassword.Value && ValidatePassword(user.Password))
                {
                    userToSave.EncryptPwd = GetSHA1(user.UserName, user.Password);
                    userToSave.Password = RandomString(16);
                }
                userToSave.UserName = user.UserName;
                userToSave.ResetPassword = user.ResetPassword;
                userToSave.EmailAddress = user.EmailAddress;
                userToSave.FirstName = user.FirstName;
                userToSave.LastName = user.LastName;
                userToSave.Active = user.Active;

                db.SaveChanges();
                if (userTypeId == 1)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Index", "Item");
            }
            ViewBag.UserType_Id = new SelectList(db.UserTypes, "UserType_id", "UserTypeDescription", user.UserType_Id);
            return View(user);
        }

        private bool ValidatePassword(string pwd)
        {
            // bool ValidatePassword(string passWord)
            //{
            //   int validConditions = 0; 
            //   foreach(char c in passWord)
            //   {
            //      if (c >= 'a' && c <= 'z')
            //      {
            //         validConditions++;
            //         break;
            //      } 
            //   } 
            //   foreach(char c in passWord)
            //   {
            //      if (c >= 'A' && c <= 'Z')
            //      {
            //         validConditions++;
            //         break;
            //      } 
            //   } 
            //   if (validConditions == 0) return false; 
            //   foreach(char c in passWord)
            //   {
            //      if (c >= '0' && c <= '9')
            //      {
            //         validConditions++;
            //         break;
            //      } 
            //   } 
            //   if (validConditions == 1) return false; 

            //   if(validConditions == 2)
            //   {       
            //      char[] special = {'@', '#', '$', '%', '^', '&', '+', '='}; // or whatever
            //      if (passWord.IndexOfAny(special) == -1) return false;
            //   } 
            //  return true;
            //}
            return pwd.Trim().Length > 3;
        }
        //
        // GET: /User/Delete/5

        public ActionResult Delete(int id = 0)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private byte[] GetSHA1(string userID, string password)
        {
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(userID + password));
        }


        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}