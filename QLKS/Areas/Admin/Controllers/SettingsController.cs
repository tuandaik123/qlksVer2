using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKS.DatabaseConnection;
using System.IO;
using System.Drawing;

namespace QLKS.Areas.Admin.Controllers
{
    public class SettingsController : Controller
    {
        public DataQLKSDataContext data;

        public SettingsController()
        {
            data = DBconnection.GetConnect();
        }
        private void Check()
        {
            db_User check = Session["admin"] as db_User;
            if (check != null && check.qlSetting == 0)
            {
                throw new HttpException(404, "Not Found");
            }
        }
        public ActionResult Index()
        {
            Check();

            return View();
        }

        [HttpPost]
        public ActionResult settingsTax(string url , FormCollection f)
        {
            Check();

            int tax = int.Parse(f["tax"]);
            var i = data.db_Taxes.Single(x => x.id == 1);
            i.tax = tax;
            data.SubmitChanges();
            return Redirect(url);
        }

        [HttpPost]
        public ActionResult settingsDiscount(string url , FormCollection f)
        {
            Check();


            var name = f["name"];
            DateTime end = DateTime.Parse(f["end"]).Date;
            DateTime start = DateTime.Parse(f["start"]).Date;
            int discount = int.Parse(f["discount"]);
            var i = data.Discounts.SingleOrDefault(x => x.id == 1);
            i.name = name;
            i.start = start;
            i.end = end;
            i.discount1 = discount;
            data.SubmitChanges();
            return Redirect(url);
        }

        public ActionResult getTaxandDiscount()
        {
            Check();

            var i = data.db_Taxes.Select(x => x.tax);
            var z = data.Discounts.SingleOrDefault(x => x.id == 1);
            return Json(new {jsTax = i , jsDis = z}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RoomsIndex(int? type = 0, int? floor = 0)
        {
            Check();


            if (type != 0)
            {
                return View(data.db_Rooms.Where(x => x.idroomtype == type));
            }

            if (floor != 0)
            {
                return  View(data.db_Rooms.Where(x => x.floorID == floor));
            }

            return View(data.db_Rooms.ToList());
        }

        public ActionResult TypeAndFloor()
        {
            Check();


            var f = data.db_Floors.Select(x => new { Id = x.id, floor = x.Floor }).ToList();
            var t = data.db_RoomTypes.Select(x => new { Id = x.id, type = x.Type }).ToList();
            return Json(new {floor = f , floorCount = f .Count, type = t, typeCount =t.Count}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult addRooms(FormCollection f , string url)
        {
            Check();


            int id = int.Parse(f["exId"]);
            int numberR = int.Parse(f["numberR"]);
            int slot = int.Parse(f["slotRoom"]);
            int status = int.Parse(f["status"]);
            int active = int.Parse(f["active"]);
            int floor = int.Parse(f["floor"]);
            int type = int.Parse(f["type"]);
            if(id > 0)
            {
                var i = data.db_Rooms.SingleOrDefault(x => x.RoomID == id);
                i.RoomNumber = numberR;
                i.Slot =slot;
                i.RoomStatus = status;
                i.ActiveStatus = active;
                i.floorID = floor;
                i.idroomtype = type;
                data.SubmitChanges();
                return Redirect(url);
            }
            db_Room r = new db_Room()
            {
                RoomNumber = numberR,
                Slot = slot,
                RoomStatus = status,
                ActiveStatus = active,
                floorID = floor,
                idroomtype = type,
            };
            data.db_Rooms.InsertOnSubmit(r);
            data.SubmitChanges();
            return Redirect(url);
        }

        public ActionResult deleteRooms(int id , string url)
        {
            Check();

            data.db_Rooms.DeleteOnSubmit(data.db_Rooms.SingleOrDefault(x => x.RoomID == id));
            data.SubmitChanges();
            return Redirect(url);
        }

        public ActionResult updateRooms(int id)
        {

            var i = data.db_Rooms.Where(x => x.RoomID == id).Select(
                x => new {ID = x.RoomID , roomId = x.RoomNumber , slot = x.Slot , status = x.RoomStatus , active = x.ActiveStatus , 
                floor = data.db_Floors.Where(y => y.id == x.floorID).Select(y => y.id) , 
                type = data.db_RoomTypes.Where(z => z.id == x.idroomtype).Select(z => z.id)}  );
            return Json(new { Data = i }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TypeRoomIndex()
        {
            Check();

            var i = data.db_RoomTypes.ToList();
            return View(i);
        }

        [HttpPost]
        public ActionResult addTypeRoom(FormCollection f, HttpPostedFileBase imageMain, HttpPostedFileBase[] imageExtra)
        {
            Check();

            int id = int.Parse(f["exId"]);
            var type = f["nameR"];
            int per = int.Parse(f["perOnnight"]);
            int bed = int.Parse(f["bed"]);
            int bath = int.Parse(f["bath"]);
            var dis = f["mota"];

            if (id == 0)
            {

                string mainFileName = "main_image" + Guid.NewGuid() + ".jpg";
                string mainPath = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), mainFileName);
                imageMain.SaveAs(mainPath);
                db_RoomType roomType = new db_RoomType()
                {
                    Type = type,
                    PricePerNight = per,
                    bed = bed,
                    bath = bath,
                    Description = dis,
                    img = mainFileName
                };
                data.db_RoomTypes.InsertOnSubmit(roomType);
                data.SubmitChanges();


                foreach (var image in imageExtra)
                {
                    string extraFileName = "extra_image_" + Guid.NewGuid() + ".jpg";
                    string extraPath = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), extraFileName);
                    image.SaveAs(extraPath);
                    ImageTypeRoom tr = new ImageTypeRoom()
                    {
                        img = extraFileName,
                        idType = roomType.id
                    };
                    data.ImageTypeRooms.InsertOnSubmit(tr);
                    data.SubmitChanges();
                }
                return RedirectToAction("TypeRoomIndex");
            }
            else
            {
                var i = data.db_RoomTypes.SingleOrDefault(x => x.id == id);
                if (imageMain != null && imageMain.ContentLength > 0)
                {
                    string mainFileName = "main_image" + Guid.NewGuid() + ".jpg";
                    string mainPath = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), mainFileName);
                    imageMain.SaveAs(mainPath);
                    string deleteImgMain = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), i.img);
                    System.IO.File.Delete(deleteImgMain);
                    
                    i.img = mainFileName;
                }
                i.Type = type;
                i.PricePerNight = per;
                i.bed = bed;
                i.bath = bath;
                i.Description = dis;
                data.SubmitChanges();
                if (imageExtra != null && imageExtra[0] != null)
                {
                    
                    var imgE = data.ImageTypeRooms.Where(x => x.idType == i.id).ToList();
                    foreach(var img in imgE)
                    {
                        string imagePath = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), img.img);
                        System.IO.File.Delete(imagePath);
                    }
                    data.ImageTypeRooms.DeleteAllOnSubmit(imgE);
                    data.SubmitChanges();
                    foreach (var image in imageExtra)
                    {
                        string extraFileName = "extra_image_" + Guid.NewGuid() + ".jpg";
                        string extraPath = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), extraFileName);
                        image.SaveAs(extraPath);
                        ImageTypeRoom tr = new ImageTypeRoom()
                        {
                            img = extraFileName,
                            idType = i.id
                        };
                        data.ImageTypeRooms.InsertOnSubmit(tr);
                        data.SubmitChanges();
                    }
                }

            }
                return RedirectToAction("TypeRoomIndex");
        }

        public ActionResult DeleteTypeRoom(int id, string url)
        {
            Check();


            var imagesToDelete = data.ImageTypeRooms.Where(x => x.idType == id).ToList();
            foreach (var image in imagesToDelete)
            {
                string imagePath = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), image.img);
                System.IO.File.Delete(imagePath);
            }

            data.ImageTypeRooms.DeleteAllOnSubmit(imagesToDelete);
            data.SubmitChanges();

            var typeR = data.db_RoomTypes.SingleOrDefault(x => x.id == id);
            string img = Path.Combine(Server.MapPath("~/Content/Client/img/img-Rooms"), typeR.img);
            System.IO.File.Delete(img);
            data.db_RoomTypes.DeleteOnSubmit(typeR);
            data.SubmitChanges();
            return Redirect(url);
        }

        public ActionResult updateTypeRoom(int id)
        {
            Check();


            var t = data.db_RoomTypes.Where(x => x.id == id).Select(x => new
            {
                Id = x.id,
                TypeRoom = x.Type,
                PricePerNight = x.PricePerNight,
                Bed = x.bed,
                Bath = x.bath,
                Img = x.img,
                Dis = x.Description
            });
            var img = data.ImageTypeRooms.Where(x => x.idType == id).Select(x => new
            {
                Id = x.id,
                Img = x.img,
                idType = x.idType,
            });
            return Json(new { Data = t , Img = img , imgCount = img.Count()}, JsonRequestBehavior.AllowGet);
        }


        //Floor
        public ActionResult FloorIndex()
        {
            Check();

            return View(data.db_Floors.ToList());
        }

        public ActionResult addFloor(FormCollection f , string url)
        {
            Check();


            var floor = Int32.Parse(f["floor"]);
            int id = int.Parse(f["exId"]);
            if (id == 0)
            {
                db_Floor fl = new db_Floor()
                {
                    Floor = floor,
                };
                data.db_Floors.InsertOnSubmit(fl);
                data.SubmitChanges();
            }
            else
            {
                var i = data.db_Floors.SingleOrDefault(x => x.id == id);
                i.Floor = floor;
                data.SubmitChanges();
            }    
            return Redirect(url); 
        }

        public ActionResult deleteFloor(int id , string url)
        {
            Check();

            var i = data.db_Floors.SingleOrDefault(x => x.id == id);
            data.db_Floors.DeleteOnSubmit(i);
            data.SubmitChanges();
            return Redirect(url);
        }

        public ActionResult updateFloor(int id)
        {
            Check();


            var i = data.db_Floors.Where(x => x.id == id).Select(x => new
            {
                id = x.id,
                floor = x.Floor,
            });
            return Json(new { Data = i}, JsonRequestBehavior.AllowGet);
        }

        //end flooor
    }
}