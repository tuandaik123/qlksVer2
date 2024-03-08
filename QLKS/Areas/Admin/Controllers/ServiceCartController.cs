using QLKS.DatabaseConnection;
using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKS.Areas.Admin.Controllers
{
    public class ServiceCartController : Controller
    {
        public DataQLKSDataContext data = DBconnection.GetConnect();
        public ServiceCartController()
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
        public List<ServiceCart> GetServiceCart()
        {
            Check();

            List<ServiceCart> lst = Session["ServiceCart"] as List<ServiceCart>;
            if (lst == null)
            {
                lst = new List<ServiceCart>();
                Session["ServiceCart"] = lst;
            }
            return lst;
        }
        public ActionResult IndexServiceCart()
        {

            List<ServiceCart> lst = GetServiceCart();
            return PartialView(lst);
        }

        public ActionResult AddServiceCart(int ms, string url)
        {
            Check();

            List<ServiceCart> lst = GetServiceCart();
            ServiceCart gh = lst.Find(n => n.iServiceId == ms);

            if (gh == null)
            {
                gh = new ServiceCart(ms);
                lst.Add(gh);
            }
            else
            {
                gh.quantity++;
            }

            return Redirect(url);
        }

        public ActionResult UpdateServiceCart(int ms, FormCollection f, string url)
        {
            Check();


            List<ServiceCart> lstGioHang = GetServiceCart();
            ServiceCart sp = lstGioHang.SingleOrDefault(n => n.iServiceId == ms);
            if (sp != null)
            {
                sp.quantity = int.Parse(f["txtSoLuong"].ToString());
            }
            return Redirect(url);
        }
        public ActionResult DeleteServiceCart(int ms, string url)
        {
            Check();


            List<ServiceCart> lst = GetServiceCart();
            ServiceCart sp = lst.SingleOrDefault(n => n.iServiceId == ms);
            if (sp != null)
            {
                lst.RemoveAll(n => n.iServiceId == ms);
                if (lst.Count == 0)
                {
                    return Redirect(url);
                }
            }
            return Redirect(url);
        }
        private int TotalPrice()
        {
            Check();

            int tt = 0;
            List<ServiceCart> lst = Session["ServiceCart"] as List<ServiceCart>;
            if (lst != null)
            {
                tt = lst.Sum(n => n.totalPrice);
            }
            return tt;
        }
        public ActionResult OrderService(int id, string url)
        {
            Check();

            List<ServiceCart> lst = GetServiceCart();
            var bk = data.db_Bookings.SingleOrDefault(x => x.RoomID == id && x.status == 0);
            foreach (var gioHangItem in lst)
            {
                db_RoomService odd = new db_RoomService();
                odd.BookingID = bk.BookingID;
                odd.ServiceID = gioHangItem.iServiceId;
                odd.Quantity = gioHangItem.quantity;
                odd.TotalPrice = gioHangItem.price * gioHangItem.quantity;
                odd.status = 1;
                odd.datetime = DateTime.Now;
                data.db_RoomServices.InsertOnSubmit(odd);
                data.SubmitChanges();
            }
            Session["ServiceCart"] = null;
            return Redirect(url);
        }

        public ActionResult deleteAll(string url)
        {
            Check();

            Session["ServiceCart"] = null;
            return Redirect(url);
        }
    }
}