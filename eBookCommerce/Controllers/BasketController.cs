using eBookCommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBookCommerce.Controllers
{
    public class BasketController : Controller
    {
        eBookCommerceEntities ebcDB = new eBookCommerceEntities();
        
        public ActionResult Index()
        {
            if (User.Identity.Name == "")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        
        [HttpPost]
        public JsonResult BasketToGrid(int draw)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);

            var basketList = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();
            
            var data = new List<object>();

            foreach (var basket in basketList)
            {
                data.Add(new object[]
                {
                    basket.AspNetUser.Email,
                    basket.Book.bookName,
                    "<button type='button' class='btn btn-danger' onclick='removeItemFromBasket(" + basket.basketId + ")'>Remove from Basket</button>"
                });
            }

            var totalRecords = data.Count();

            var json = new
            {
                draw = draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data = data
            };

            return Json(json);
        }

        [HttpPost]
        public JsonResult RemoveItemFromBasket(int itemId)
        {
            var basketItem = ebcDB.Baskets.Single(a => a.basketId == itemId);
            ebcDB.Baskets.Remove(basketItem);
            ebcDB.SaveChanges();
            return Json(true);
        }

    }
}