using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCeTicaret.Models;

namespace MVCeTicaret.Controllers
{
    public class LoginController : Controller
    {
        Context db = new Context();

        public LoginController()
        {

        }

        public ActionResult Register()
        {
            TemporaryUserData.LastPage = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
            return View();
        }

        [HttpPost]
        public ActionResult Register(FormCollection frm)
        {
            string kullaniciAdi = frm["username"];
            Customer customer = db.Customers.FirstOrDefault(x => x.UserName == kullaniciAdi);

            if (customer != null)
            {
                return View();
            }
            else
            {
                customer = new Customer();

                string isim = frm["name"];
                string soyisim = frm["surname"];
                string sifre = frm["password"];
                bool cinsiyet = frm["gender"] == "on" ? true : false;
                DateTime dogumTarihi = DateTime.Parse(frm["birthdate"]);

                customer.FirstName = isim;
                customer.LastName = soyisim;
                customer.UserName = kullaniciAdi;
                customer.Password = sifre;
                customer.Gender = cinsiyet;
                customer.BirthDate = dogumTarihi;
                customer.CreatedDate = DateTime.Now;
                customer.LastLogin = DateTime.Now;

                db.Customers.Add(customer);
                db.SaveChanges();

                return Redirect(TemporaryUserData.LastPage);
            }
        }

        public ActionResult Login()
        {
            TemporaryUserData.LastPage = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection frm)
        {
            string kullaniciAdi = frm["username"];
            string sifre = frm["password"];

            Customer customer = db.Customers.Where(x => x.UserName == kullaniciAdi && x.Password == sifre).FirstOrDefault();
            if (db.Customers.Where(x => x.UserName == kullaniciAdi && x.Password == sifre).FirstOrDefault() != null)
            {
                Session["OnlineKullanici"] = kullaniciAdi;
                TemporaryUserData.UserID = customer.CustomerID;

                return Redirect(TemporaryUserData.LastPage);
            }
            return View();
        }

        public ActionResult Logout()
        {
            db.Customers.Find(TemporaryUserData.UserID).LastLogin = DateTime.Now;
            db.SaveChanges();


            Session["OnlineKullanici"] = null;
            TemporaryUserData.UserID = 0;

            return RedirectToAction("Index", "Home");
        }
    }
}