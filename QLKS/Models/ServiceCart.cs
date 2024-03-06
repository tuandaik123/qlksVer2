using QLKS.DatabaseConnection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace QLKS.Models
{
    public class ServiceCart
    {
        public int iServiceId { get; set; }
        public string sName { get; set; }
        public string sImage { get; set; }
        public int price { get; set; }
        public int quantity { get; set; }
        public int totalPrice { get { return quantity * price; } }
        public DataQLKSDataContext data;

        public ServiceCart(int ms)
        {
            data = DBconnection.GetConnect();
            iServiceId = ms;
            db_Service s = data.db_Services.Single(n => n.ServiceID == iServiceId);
            List<db_Service> e = new List<db_Service>();
            sName = s.ServiceName;
            sImage = s.image;
            price = int.Parse(s.Price.ToString());
            quantity = 1;
        }
    }
}