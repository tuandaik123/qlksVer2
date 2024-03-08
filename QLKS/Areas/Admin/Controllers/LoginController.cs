using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QLKS.DatabaseConnection;
using System.Web.Mvc;
using System.Web.Helpers;
using Microsoft.Ajax.Utilities;

namespace QLKS.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        public DataQLKSDataContext data;
        // GET: Extra
        public LoginController()
        {
            data = DBconnection.GetConnect();
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection f)
        {
            var user = f["user"];
            var pass = f["pass"];

            db_User userEntity = data.db_Users.SingleOrDefault(x => x.Username.Equals(user) && x.Password.Equals(pass)) as db_User;

            if (userEntity != null)
            {
                Session["admin"] = userEntity;
                    return RedirectToAction("Index", "HomeAdmin");
            }

            ViewBag.msg = "Sai tài khoản hoặc mật khẩu";
            ViewBag.color = "red";
            return this.Index();
        }

        public ActionResult Logout()
        {
            Session["admin"] = null;
            return RedirectToAction("Index");
        }
    }
}