using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKS.DatabaseConnection;


namespace QLKS.Areas.Admin.Controllers
{
    public class ReservationsController : Controller
    {
        public DataQLKSDataContext data;
        // GET: Extra
        public ReservationsController()
        {
            data = DBconnection.GetConnect();
        }
        [CheckSession]
        public ActionResult Index(int id = 0)
        {
            if (id == 0)
            {
                return View(data.BookingsOnlines.Where(x => x.status == 0).ToList());
            }
            return View(data.BookingsOnlines.Where(x => x.status == 1).ToList());
        }
        private void Check()
        {
            db_User check = Session["admin"] as db_User;
            if (check != null && check.qlKS == 0)
            {
                throw new HttpException(404, "Not Found");
            }
        }

        [CheckSession]
        public ActionResult check(int id, string url)
        {
            Check();

            var i = data.BookingsOnlines.SingleOrDefault(x => x.id == id);
            i.Verification = 1;
            data.SubmitChanges();
            return Redirect(url);
        }

        [CheckSession]
        public ActionResult receive(int id, string url)
        {
            Check();

            var i = data.BookingsOnlines.SingleOrDefault(x => x.id == id);
            i.status = 1;
            data.SubmitChanges();
            db_Booking bk = new db_Booking()
            {
                CustomerID = i.custommerID,
                RoomID = i.RoomID,
                CheckInDate = i.CheckInDate,
                CheckOutDate = i.CheckOutDate,
                status = 0
            };
            data.db_Bookings.InsertOnSubmit(bk);
            data.SubmitChanges();
            var room = data.db_Rooms.SingleOrDefault(x => x.RoomID == i.RoomID);
            room.RoomStatus = 0;
            data.SubmitChanges();
            return Redirect(url);
        }
    }
}