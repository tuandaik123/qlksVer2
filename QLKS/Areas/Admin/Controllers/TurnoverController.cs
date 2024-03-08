using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKS.DatabaseConnection;
using QLKS.Models;


namespace QLKS.Areas.Admin.Controllers
{
    public class TurnoverController : Controller
    {
        public DataQLKSDataContext data;
        // GET: Extra
        public TurnoverController()
        {
            data = DBconnection.GetConnect();
        }
        private void Check()
        {
            db_User check = Session["admin"] as db_User;
            if (check != null && check.qlThongKe == 0)
            {
                throw new HttpException(404, "Not Found");
            }
        }
        [CheckSession]               
        public ActionResult Index()
        {
            Check();

            return View();
        }

        [CheckSession]
        public ActionResult GetTotalPaymentByMonth()
        {
            Check();

            var payments = data.db_Payments.ToList();

            var result = payments
                .Where(payment => payment.method == 0 || payment.method == 1)
                .GroupBy(payment => new { Month = payment.PaymentDate.Month, Year = payment.PaymentDate.Year })
                .OrderBy(group => group.Key.Year)
                .ThenBy(group => group.Key.Month)
                .Select(group => new
                {
                    MonthYear = $"{group.Key.Month}/{group.Key.Year}",

                    Cash = group.Where(payment => payment.method == 0).Sum(payment => payment.Amount),

                    BankingAmount = group.Where(payment => payment.method == 1).Sum(payment => payment.Amount)
                })
                .ToList();

            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }

        [CheckSession]
        public ActionResult getYear()
        {
            Check();

            var distinctYears = data.db_Payments.Select(x => x.PaymentDate.Year).Distinct();
            return Json(new { Data = distinctYears, Total = distinctYears.Count() }, JsonRequestBehavior.AllowGet);
        }

        [CheckSession]
        public ActionResult GetTotalPaymentByDay()
        {
            Check();

            var payments = data.db_Payments.ToList();

            var result = payments
                .Where(payment => payment.method == 0 || payment.method == 1)
                .GroupBy(payment => new { Day = payment.PaymentDate.Day, Month = payment.PaymentDate.Month, Year = payment.PaymentDate.Year })
                .OrderBy(group => group.Key.Day)
                .ThenBy(group => group.Key.Month)
                .ThenBy(group => group.Key.Year)
                .Select(group => new
                {
                    Date = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day),
                    Cash = group.Sum(payment => payment.method == 0 ? payment.Amount : 0),
                    BankingAmount = group.Sum(payment => payment.method == 1 ? payment.Amount : 0)
                })
                .GroupBy(item => item.Date)
                .Select(group => new
                {
                    MonthYear = $"{group.Key.Day}/{group.Key.Month}/{group.Key.Year}",
                    Cash = group.Sum(item => item.Cash),
                    BankingAmount = group.Sum(item => item.BankingAmount)
                })
                .ToList();

            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }

        [CheckSession]
        public ActionResult Today()
        {
            Check();


            DateTime date = DateTime.UtcNow.Date;
            var bk = data.db_Bookings.Where(x => x.CheckInDate == date).Count();
            var revenueToday = data.db_Payments.Where(x => x.PaymentDate.Date == date)
                                             .Select(x => x.Amount)
                                             .Sum();
            var i = data.db_RoomServices.Where(x => x.datetime.Value.Date == date)
               .Select(x => x.TotalPrice).Sum();
            var dv = data.db_RoomServices.Where(x => x.datetime.Value.Date == date).Count();
            return Json(new {Booking = bk , revenue = revenueToday , service = i , bookingOnline = dv}, JsonRequestBehavior.AllowGet);
        }

    }
}