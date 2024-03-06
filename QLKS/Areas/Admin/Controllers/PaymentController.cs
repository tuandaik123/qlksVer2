using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VNPAY_CS_ASPX;
using QLKS.DatabaseConnection;
using QLKS.Models;

namespace QLKS.Areas.Admin.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Admin/Payment
        public ActionResult Index()
        {
            return View();
        }

        public DataQLKSDataContext data = DBconnection.GetConnect();
        public PaymentController()
        {
            data = DBconnection.GetConnect();
        }

        private decimal Total(int id)
        {
            var bk = data.db_Bookings.SingleOrDefault(x => x.BookingID == id);
            var room = data.db_Rooms.SingleOrDefault(x => x.RoomID == bk.RoomID);

            var checkInDate = bk.CheckInDate;
            var checkOutDate = bk.CheckOutDate;
            var roomType = data.db_RoomTypes.SingleOrDefault(x => x.id == room.idroomtype);

            int numberOfDays = ((DateTime)checkOutDate - (DateTime)checkInDate).Days;

            decimal totalRoom = (int)(numberOfDays * roomType.PricePerNight);
            decimal? totalService = data.db_RoomServices
                .Where(x => x.BookingID == bk.BookingID)
                .Sum(x => x.TotalPrice);
            var tax = data.db_Taxes.SingleOrDefault(x => x.id == 1);
            int? taxInt = tax.tax;
            var dis = data.Discounts.SingleOrDefault(x => x.id == 1 && x.start<=DateTime.Today && x.end>= DateTime.Today);
            int? disInt = dis.discount1;
            if(taxInt == null)
            {
                taxInt = 0;
            }
            if(disInt == null)
            {
                disInt = 0;
            }
            decimal total1 = (int)(totalRoom + (totalService ?? 0));

            decimal total = total1 + total1*((int)taxInt.Value)/100 - total1*(int)(disInt.Value)/100;

            return total;

        }

        [HttpPost]
        [CheckSession]
        public ActionResult Payment(FormCollection f, int roomid)
        {
            string paymentType = "1";
            var bk = data.db_Bookings.SingleOrDefault(x => x.RoomID == roomid && x.status == 0);
            var room = data.db_Rooms.SingleOrDefault(x => x.RoomID == roomid);
            string method = f["method"];
            int methodInt = int.Parse(method);
            var checkInDate = bk.CheckInDate;
            var checkOutDate = bk.CheckOutDate;
            var roomType = data.db_RoomTypes.SingleOrDefault(x => x.id == room.idroomtype);

            int numberOfDays = ((DateTime)checkOutDate - (DateTime)checkInDate).Days;

            decimal totalRoom = (int)(numberOfDays * roomType.PricePerNight);
            decimal? totalService = data.db_RoomServices
                .Where(x => x.BookingID == bk.BookingID)
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
            if (methodInt == 0)
            {
                db_Payment pm = new db_Payment()
                {
                    BookingID = bk.BookingID,
                    PaymentDate = DateTime.Now,
                    Amount = total,
                    method = 0,
                    Discount = disInt,
                    Tax = taxInt
                };
                data.db_Payments.InsertOnSubmit(pm);
                room.RoomStatus = 1;
                bk.status = 1;
                data.SubmitChanges();
                return RedirectToAction("Roomdiagram", "RoomDiagram");
            }
            else
            {

                var url = UrlPayment(paymentType, bk.BookingID);
                return Redirect(url);
            }
        }

        [CheckSession]
        public ActionResult PaymentReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                int bookingID = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];
                var bk = data.db_Bookings.SingleOrDefault(x => x.BookingID ==  bookingID && x.status == 0);
                var room = data.db_Rooms.SingleOrDefault(x => x.RoomID == bk.RoomID);
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
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        db_Payment pm = new db_Payment()
                        {
                            BookingID = bookingID,
                            Amount = Total(bookingID),
                            PaymentDate = DateTime.Now,
                            method = 1,
                            Discount = disInt,
                            Tax = taxInt
                        };
                        data.db_Payments.InsertOnSubmit(pm);
                        data.SubmitChanges();
                        bk.status = 1;
                        data.SubmitChanges();
                        room.RoomStatus = 1;
                        data.SubmitChanges();
                        ViewBag.Msg = "Thanh toán của bạn đã thành công";
                        ViewBag.icon = "http://pluspng.com/img-png/success-png-png-svg-512.png";
                        ViewBag.color = "rgb(65, 209, 161)";
                        Session["GioHang"] = null;
                    }
                    else
                    {

                        ViewBag.Msg = "Có lỗi xảy ra trong quá trình xử lý";
                        ViewBag.icon = "https://images.onlinelabels.com/images/clip-art/molumen/molumen_red_round_error_warning_icon.png";
                        ViewBag.color = "red";
                    }
                    ViewBag.IDweb = "Mã Website (Terminal ID) : " + TerminalID;
                    ViewBag.SymbolsPayment = "Mã giao dịch thanh toán : " ;
                    ViewBag.SymbolsVNPAY = "Mã giao dịch tại VNPAY : " + vnpayTranId.ToString();
                    ViewBag.totalPrice = "Số tiền thanh toán : " + vnp_Amount.ToString("0,000,000 ") + "VND";
                    ViewBag.bank = "Ngân hàng thanh toán : " + bankCode;
                }
                else
                {
                    ViewBag.Msg = "Có lỗi xảy ra trong quá trình xử lý";
                    ViewBag.icon = "https://images.onlinelabels.com/images/clip-art/molumen/molumen_red_round_error_warning_icon.png";
                    ViewBag.color = "red";
                }
            }
            return View();
        }

        private string UrlPayment(string paymentType, int id)
        {
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

            //Get payment input
            var bk = data.db_Bookings.SingleOrDefault(o => o.BookingID == id);
            //Save order to db

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (Total(bk.BookingID) * 100).ToString());
            if (paymentType.Equals("VNPAYQR"))
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (paymentType.Equals("VNBANK"))
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (paymentType.Equals("INTCARD"))
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }

            DateTime createDate = Convert.ToDateTime(DateTime.Now);
            vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan hoa don:" + bk.BookingID);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", bk.BookingID.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }
    }
}