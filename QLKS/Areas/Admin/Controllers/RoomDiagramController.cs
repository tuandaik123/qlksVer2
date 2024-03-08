using QLKS.DatabaseConnection;
using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace QLKS.Areas.Admin.Controllers
{
    public class RoomDiagramController : Controller
    {
        public DataQLKSDataContext data;
        // GET: Extra
        public RoomDiagramController()
        {
            data = DBconnection.GetConnect();
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
        public ActionResult Roomdiagram()
        {
            Check();
            var rooms = data.db_Rooms.Where(x=>x.ActiveStatus == 1).ToList();
            List<db_Room> availableRooms = new List<db_Room>();

            foreach (var room in rooms)
            {
                var bookings = data.BookingsOnlines
                    .Where(x => x.RoomID == room.RoomID)
                    .ToList();

                bool roomBooked = bookings.Any(r =>
                    (DateTime.Today.AddDays(1) >= r.CheckInDate  && r.status == 0) ||
                    (DateTime.Today.AddDays(1) > r.CheckInDate  && r.status == 0)  ||
                    (DateTime.Today.AddDays(1) <= r.CheckInDate && r.status == 0));

                if (!roomBooked)
                {
                    availableRooms.Add(room);
                }
            }

            return View(availableRooms);
        }

        public ActionResult ListFloor()
        {
            Check();

            var floors = data.db_Floors.ToList();
            return PartialView(floors);
        }

        public ActionResult FindFloor(int id)
        {
            Check();

            var floors = data.db_Rooms.Where(x => x.floorID == id).ToList();
            return PartialView(floors);
        }

        public ActionResult ListTypeRoom()
        {
            Check();

            var floors = data.db_RoomTypes.ToList();
            return PartialView(floors);
        }

        public ActionResult FindTypeRoom(int id)
        {
            Check();

            var rooms = data.db_Rooms.Where(x =>x.idroomtype == id).ToList();
            return PartialView(rooms);
        }

        public ActionResult SelectRooms(int id)
        {
            Check();

            var inf = data.db_Bookings.SingleOrDefault(x => x.RoomID == id && x.status == 0);
            var room = data.db_Rooms.SingleOrDefault(x => x.RoomID == inf.RoomID);
            var ctm = data.Custommers.SingleOrDefault(x => x.CustomerID == inf.CustomerID);
            var roomss = data.db_Rooms.Where(x => x.RoomID == id);

            var checkInDate = inf.CheckInDate;
            var checkOutDate = inf.CheckOutDate;
            var roomType = data.db_RoomTypes.SingleOrDefault(x => x.id == room.idroomtype);

            int numberOfDays = ((DateTime)checkOutDate - (DateTime)checkInDate).Days;
            if (((DateTime)checkOutDate - (DateTime)checkInDate).TotalHours % 24 != 0)
            {
                numberOfDays++;
            }
            decimal totalRoom = (int)(numberOfDays * roomType.PricePerNight);
            decimal? totalService = data.db_RoomServices
                .Where(x => x.BookingID == inf.BookingID)
                .Sum(x => x.TotalPrice);
            var tax = data.db_Taxes.SingleOrDefault(x => x.id == 1);
            int? taxInt = tax.tax;
            var dis = data.Discounts.SingleOrDefault(x => x.id == 1 && x.start <= DateTime.Today && x.end >= DateTime.Today);
            int? disInt = dis?.discount1;
            if (taxInt == null)
            {
                taxInt = 0;
            }
            if (disInt == null)
            {
                disInt = 0;
            }
            decimal total1 = (int)(totalRoom + (totalService ?? 0));
            decimal total = total1 + total1 * ((int)taxInt.Value) / 100 - total1 * (int)(disInt.Value) / 100;

            ViewBag.rooms = room.RoomNumber;
            ViewBag.sdt = ctm.Phone;
            ViewBag.name = ctm.FullName;
            ViewBag.cccd = ctm.UserID;
            ViewBag.email = ctm.Email;
            ViewBag.checkout = checkOutDate;
            ViewBag.totalRoom = totalRoom;
            ViewBag.totalService = totalService ?? 0;
            ViewBag.total = total;
            ViewBag.checkin = DateTime.Now;
            return View(roomss);
        }

        [CheckSession]
        public ActionResult SelectRoomsEmpty(int id)
        {
            Check();

            var rooms = data.db_Rooms.SingleOrDefault(x => x.RoomID == id);
            return View(rooms);

        }

        [CheckSession]
        [HttpPost]
        public ActionResult BookingOffline(int id , FormCollection f)
        {
            Check();

            DateTime checkout = DateTime.Parse(f["checkout"]);
            var name = f["name"];
            var email = f["email"];
            var cccd = f["cccd"];
            var sdt = f["sdt"];
            Custommer cmt = new Custommer()
            {
                UserID = cccd,
                FullName = name,
                Email = email,
                Phone = sdt
            };
            data.Custommers.InsertOnSubmit(cmt);
            data.SubmitChanges();
            db_Booking bk = new db_Booking()
            {
                CustomerID = cmt.CustomerID,
                RoomID = id,
                CheckInDate = DateTime.Today,
                CheckOutDate = checkout,
                status = 0
            };
            data.db_Bookings.InsertOnSubmit(bk);
            data.SubmitChanges();
            var room = data.db_Rooms.SingleOrDefault(x => x.RoomID == id);
            room.RoomStatus = 0;
            data.SubmitChanges();
            return RedirectToAction("Roomdiagram");
        }

        [CheckSession]
        public ActionResult IndexServiceCart(int id)
        {
            Check();

            var bk = data.db_Bookings.SingleOrDefault(x => x.RoomID == id && x.status == 0);
            var lst = data.db_RoomServices.Where(x => x.BookingID == bk.BookingID && x.status == 1).ToList();
            return PartialView(lst);
        }

        [CheckSession]
        public ActionResult Service()
        {
            Check();

            return PartialView(data.db_Services.ToList());
        }

        [CheckSession]
        public ActionResult IndexServiceCartUsed(int id)
        {
            Check();

            var bk = data.db_Bookings.SingleOrDefault(x => x.RoomID == id && x.status == 0);
            var lst = data.db_RoomServices.Where(x => x.BookingID == bk.BookingID && x.status == 1).ToList();
            return PartialView(lst);
        }

        [CheckSession]
        public ActionResult renewCheckout(int id , FormCollection f , string url)
        {
            Check();

            int checkout = int.Parse(f["checkout"]);
            var bk = data.db_Bookings.SingleOrDefault(x => x.RoomID == id && x.status == 0);
            bk.CheckOutDate = bk.CheckOutDate?.AddDays(checkout);
            data.SubmitChanges();
            return Redirect(url);
        }

        public ActionResult ChangeRoomList()
        {
            Check();

            var bookedRooms = data.BookingsOnlines
                .Where(r =>
                    (DateTime.Now.AddDays(1) >= r.CheckInDate && r.status == 0) ||
                    (DateTime.Now.AddDays(1)> r.CheckInDate && r.status == 0) ||
                    (DateTime.Now.AddDays(1) <= r.CheckInDate && r.status == 0))
                .Select(r => r.RoomID)
                .ToList();

            var availableRooms = data.db_Rooms
                .Where(x => x.RoomStatus == 1 && !bookedRooms.Contains(x.RoomID))
                .ToList();

            return PartialView(availableRooms);
        }

        [CheckSession]
        public ActionResult ChangeRoom(int idRoom , FormCollection f)
        {
            Check();

            string cr = f["changeroom"];
            int room = int.Parse(cr);
            var bk = data.db_Bookings.SingleOrDefault(x => x.RoomID == idRoom && x.status == 0);
            bk.RoomID = room;
            data.SubmitChanges();

            var r = data.db_Rooms.SingleOrDefault(x => x.RoomID == idRoom);
            r.RoomStatus = 1;
            data.SubmitChanges();

            var roomchange = data.db_Rooms.SingleOrDefault(x => x.RoomID == room);
            roomchange.RoomStatus = 0;
            data.SubmitChanges();
           
            data.SubmitChanges();
            return RedirectToAction("Roomdiagram");
        }

    }
}