using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKS.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Err404()
        {
            return View();
        }
    }
}