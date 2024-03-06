using QLKS.DatabaseConnection;
using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKS.Controllers
{
    public class PageController : Controller
    {
        public DataQLKSDataContext data;
        // GET: Extra
        public PageController()
        {
            data = DBconnection.GetConnect();
        }
        // GET: Page
        public ActionResult Rooms()
        {
            return View();
        }
        public ActionResult Bookings(int id)
        {
            var imgList = data.ImageTypeRooms
                               .Where(x => x.idType == id)
                               .Take(5)
                               .ToList();

            return View(imgList);
        }
        public ActionResult Sevices()
        {
            return View(data.db_Facilities.ToList());  
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult OurTeams()
        {
            return View();
        }
    }
}