using eBookCommerce.Models;
using Newtonsoft.Json;
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

            eBookCommerceViewModel viewModel = new eBookCommerceViewModel(0, 0);
            return View(viewModel);
        }
        
        [HttpPost]
        public JsonResult BasketToGrid(int draw, Dictionary<string, string> search, int start, int length)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);
            var basketItems = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();

            var recordsTotal = basketItems.Count();

            string searchValue = search["value"].ToLower();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                basketItems = basketItems.Where(
                    a => a.AspNetUser.Email.ToLower().Contains(searchValue) ||
                    a.Book.bookName.ToLower().Contains(searchValue) ||
                    a.Book.bookAuthor.ToLower().Contains(searchValue)).ToList();
            }

            basketItems = basketItems.Skip(start).Take(length).ToList();
            var recordsFiltered = basketItems.Count();

            var data = new List<object>();

            foreach (var basketItem in basketItems)
            {
                data.Add(new object[]
                {
                    basketItem.AspNetUser.Email,
                    basketItem.Book.bookName,
                    basketItem.Book.bookAuthor,
                    basketItem.Book.bookPages,
                    basketItem.Book.bookPrice,
                    "<img src='" + basketItem.Book.bookImageUrl + "' class='rounded-circle' style='width: 30px; height: 30px; object-fit: cover'>",
                    "<button type='button' class='btn btn-danger my-button' onclick='removeItemFromBasket(" + basketItem.basketId + ")'>Remove from Basket</button>"
                });
            }

            var totalRecords = data.Count();

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
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

        [HttpPost]
        public JsonResult RemoveAllItemsFromBasket()
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);
            var basketItems = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();

            foreach(var basketItem in basketItems)
            {
                ebcDB.Baskets.Remove(basketItem);
            }
            
            ebcDB.SaveChanges();
            return Json(true);
        }
    }
}