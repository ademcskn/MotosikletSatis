using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCeTicaret.Models;

namespace MVCeTicaret.Controllers
{
    public class ShoppingController : Controller
    {
        Context db;
        public ShoppingController()
        {
            db = new Context();
        }

        private void ControlWishlist(int id)
        {
            Wishlist wishlist = db.Wishlists.FirstOrDefault(x => x.ProductID == id && x.CustomerID == TemporaryUserData.UserID && x.IsActive == true);

            if (wishlist == null)
            {
                wishlist = new Wishlist();
                wishlist.ProductID = id;
                wishlist.CustomerID = TemporaryUserData.UserID;
                wishlist.IsActive = true;
                db.Wishlists.Add(wishlist);
                OrderDetail orderDetail = db.OrderDetails.FirstOrDefault(x => x.CustomerID == TemporaryUserData.UserID && x.ProductID == id && x.IsCompleted == false);
                db.Wishlists.Add(wishlist);
                db.SaveChanges();
            }
        }

        private void ControlCart(int id, int quantity = 1)
        {
            OrderDetail od = db.OrderDetails.FirstOrDefault(x => x.ProductID == id && x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false);//TODO: CustomerID Dinamik olacak.!!

            if (od == null) //yeni kayıt
            {
                od = new OrderDetail();
                od.ProductID = id;
                od.CustomerID = TemporaryUserData.UserID;
                od.IsCompleted = false;
                od.UnitPrice = db.Products.Find(id).UnitPrice;
                od.Discount = db.Products.Find(id).Discount;
                od.OrderDate = DateTime.Now;

                if (db.Products.Find(id).UnitInStock > quantity)
                    od.Quantity = quantity;
                else
                    od.Quantity = db.Products.Find(id).UnitInStock;

                od.TotalAmount = od.Quantity * od.UnitPrice * (1 - od.Discount);
                db.OrderDetails.Add(od);
            }
            else //update işlemi
            {
                if (od.Product.UnitInStock >= od.Quantity + quantity)
                {
                    od.Quantity += quantity;
                    od.TotalAmount = od.Quantity * od.UnitPrice * (1 - od.Discount);
                }
            }
            db.SaveChanges();
        }

        public ActionResult AddToCart(FormCollection frm, int id)
        {

            if (TemporaryUserData.UserID == 0)
                return RedirectToAction("Login", "Login");

            int quantity = Convert.ToInt32(frm["quantity"]);
            ControlCart(id, quantity);

            return RedirectToAction("ProductDetail", "Product", new { id = id });
        }

        public ActionResult AddToWishlist(int id)
        {
            if (TemporaryUserData.UserID == 0)
            {
                return RedirectToAction("Login", "Login");
            }
            ControlWishlist(id);
            return RedirectToAction("ProductDetail", "Product", new { id = id });
        }

        public ActionResult Cart()
        {
            return View(db.OrderDetails.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false).ToList());
        }

        [HttpPost]
        public ActionResult UpdateCart(FormCollection frm)
        {
            int quantity = Convert.ToInt32(frm["Quantity"]);
            int id = Convert.ToInt32(frm["orderDetailID"]);
            OrderDetail od = db.OrderDetails.Find(id);
            od.Quantity = quantity;
            db.SaveChanges();
            return RedirectToAction("Cart");
        }

        public ActionResult Wishlist()
        {
            return View(db.Wishlists.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsActive == true).ToList());
        }

        public ActionResult AddToWishlistFromCart()
        {
            return View();
        }

        public ActionResult RemoveFromCart(int id)
        {
            OrderDetail od = db.OrderDetails.Find(id);
            db.OrderDetails.Remove(od);
            db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult AddWishlistFromCart(int id)
        {
            ControlWishlist(id);
            return RedirectToAction("Cart");
        }

        public ActionResult RemoveWishlistFromCart(int id)
        {
            Wishlist wishlist = db.Wishlists.FirstOrDefault(x => x.ProductID == id && x.CustomerID == TemporaryUserData.UserID && x.IsActive == true);

            if (wishlist != null)
            {

                wishlist.IsActive = false;
                db.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        public ActionResult AddToWishlistFormCart(int id)
        {
            ControlWishlist(id);

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult RemoveFromWishlist(int id)
        {
            Wishlist wishlists = db.Wishlists.Find(id);
            wishlists.IsActive = false;
            db.SaveChanges();

            return RedirectToAction("Wishlist", "Shopping");
        }

        public ActionResult AddToCartFromWishlist(int id)
        {
            int productID = db.Wishlists.Find(id).ProductID;
            ControlCart(productID);

            Wishlist wishlists = db.Wishlists.Find(id);
            wishlists.IsActive = false;
            db.SaveChanges();

            return RedirectToAction("Wishlist", "Shopping");
        }

        public ActionResult GoToPayment()
        {
            List<OrderDetail> cart = db.OrderDetails.Where(x => x.IsCompleted == false && x.CustomerID == TemporaryUserData.UserID).ToList();
            foreach (var item in cart)
            {
                if (item.Product.UnitInStock < item.Quantity)
                    return RedirectToAction("UpdateCart","Shopping");
            }

            ViewBag.OrderDetails = db.OrderDetails.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false).ToList();
            ViewBag.PaymentTypes = db.PaymentTypes.ToList();

            return View(db.Customers.Find(TemporaryUserData.UserID));
        }

        public ActionResult UpdateCart()
        {
            return View(db.OrderDetails.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false).ToList());
        }

        public ActionResult CompleteShopping(FormCollection frm)
        {
            Payment payment = new Payment();
            payment.Type = int.Parse(frm["paymentType"]);
            payment.CreditAmount = 25000;
            payment.DebitAmount = 25000;
            payment.Balance = 25000;
            payment.PaymentDateTime = DateTime.Now;

            db.Payments.Add(payment);
            db.SaveChanges();

            if (frm["update"] == "on")
            {
                Customer customer = db.Customers.Find(TemporaryUserData.UserID);
                customer.FirstName = frm["FirstName"];
                customer.LastName = frm["LastName"];
                customer.Address = frm["Address"];
                customer.City = frm["City"];
            }
            ShippingDetail sp = new ShippingDetail();
            sp.FirstName = frm["FirstName"];
            sp.LastName = frm["LastName"];
            sp.Address = frm["Address"];
            sp.City = frm["City"];

            db.ShippingDetails.Add(sp);
            db.SaveChanges();


            List<OrderDetail> cart = db.OrderDetails.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false).ToList();
            foreach (var item in cart)
            {
                item.IsCompleted = true;
                item.Product.UnitInStock -= item.Quantity;
                Order order = new Order()
                {
                    PaymentID = payment.PaymentID,
                    ShippingID = sp.ShippingID,
                    OrderDetailID = item.OrderDetailID,
                    Discount = item.Discount,
                    TotalAmount = item.TotalAmount,
                    IsCompleted = true,
                    OrderDate = DateTime.Now,
                    Dispatched = false,
                    DispatchedDate = DateTime.Now.AddDays(4),
                    Shipped = false,
                    ShippedDate = DateTime.Now.AddDays(5),
                    Deliver = false,
                    DeliveryDate = DateTime.Now.AddDays(5),
                    CancelOrder = false

                };
                db.SaveChanges();
            }

            return RedirectToAction("FinishedShopping", "Shopping");
        }

        public ActionResult FinishedShopping()
        {
            return View();
        }
    }
}