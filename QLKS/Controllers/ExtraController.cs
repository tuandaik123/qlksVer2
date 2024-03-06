using QLKS.DatabaseConnection;
using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKS.Controllers
{
    public class ExtraController : Controller
    {
        public DataQLKSDataContext data;
        // GET: Extra
        public ExtraController()
        {
            data = DBconnection.GetConnect();
        }

        public ActionResult Slider()
        {
            return PartialView(data.Sliders.ToList());
        }

        public ActionResult ImagePages()
        {
            return PartialView(data.Sliders.FirstOrDefault());
        }

        public ActionResult TypeRooms()
        {
            return PartialView(data.db_RoomTypes.ToList());
        }

        [HttpPost]
        public ActionResult ChooseDate(FormCollection f, int id)
        {
            DateTime checkin = DateTime.Parse(f["checkin"]).Date;
            DateTime checkout = DateTime.Parse(f["checkout"]).Date;

            var name = f["name"];
            var email = f["email"];
            var cccd = f["cccd"];
            var sdt = f["sdt"];
            var rooms = data.db_Rooms.Where(x => x.idroomtype == id).ToList();
            Custommer ctm = new Custommer()
            {
                UserID = cccd,
                Email = email,
                Phone = sdt,
                FullName = name
            };
            data.Custommers.InsertOnSubmit(ctm);
            data.SubmitChanges();
            foreach (var room in rooms)
            {
                var bookings = data.BookingsOnlines.Where(x => x.RoomID == room.RoomID).ToList();
                var bookingsOfline = data.db_Bookings.Where(x => x.RoomID == room.RoomID).ToList();
                bool roomBooked = bookings.Any(r =>
                    (checkin >= r.CheckInDate && checkin < r.CheckOutDate && r.status == 0) ||
                    (checkout > r.CheckInDate && checkout <= r.CheckOutDate && r.status == 0) ||
                    (checkin <= r.CheckInDate && checkout >= r.CheckOutDate && r.status == 0));

                bool roomBooked2 = bookingsOfline.Any(r =>
                    (checkin >= r.CheckInDate && checkin < r.CheckOutDate && r.status == 0) ||
                    (checkout > r.CheckInDate && checkout <= r.CheckOutDate && r.status == 0) ||
                    (checkin <= r.CheckInDate && checkout >= r.CheckOutDate && r.status == 0));
                if (!roomBooked && !roomBooked2)
                {
                    BookingsOnline t = new BookingsOnline()
                    {
                        RoomID = room.RoomID,
                        CheckInDate = checkin,
                        CheckOutDate = checkout,
                        custommerID = ctm.CustomerID,
                        Verification = 0,
                        status = 0
                    };

                    data.BookingsOnlines.InsertOnSubmit(t);
                    data.SubmitChanges();

                    return RedirectToAction("Success", "Extra");
                }
            }
            data.Custommers.DeleteOnSubmit(ctm);
            data.SubmitChanges();
            return RedirectToAction("Index", "Home", new { message = "No available rooms for the selected dates." });
        }

        public ActionResult facilities()
        {
            return PartialView(data.db_Facilities.ToList());
        }

        public ActionResult AboutUS()
        {
            ViewBag.SumRoom = SumRoom();
            ViewBag.SumFacilities = SumFacilities();
            ViewBag.SumCustomers = SumCustomers();
            return PartialView();
        }
        public int SumRoom()
        {
            var result = data.db_Rooms
                .GroupBy(room => room.RoomID)
                .Select(group => new
                {
                    RoomID = group.Key,
                    TotalRooms = group.Count(),
                })
                .ToList();

            int totalRooms = result.Sum(item => item.TotalRooms);

            return totalRooms;
        }
        public int SumFacilities()
        {
            var result = data.db_Facilities
                .GroupBy(facility => facility.FacilityID)
                .Select(group => new
                {
                    FacilityID = group.Key,
                    TotalFacilities = group.Count(),
                })
                .ToList();

            int totalFacilities = result.Sum(item => item.TotalFacilities);

            return totalFacilities;
        }
        public int SumCustomers()
        {
            var result = data.Custommers
                .GroupBy(customer => customer.CustomerID)
                .Select(group => new
                {
                    CustomerID = group.Key,
                    TotalCustomers = group.Count(),
                })
                .ToList();

            int totalCustomers = result.Sum(item => item.TotalCustomers);

            return totalCustomers;
        } 

    }

}