using eBookCommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace eBookCommerce.Controllers
{
    public class HomeController : Controller
    {
        eBookCommerceEntities ebcDB = new eBookCommerceEntities();

        public ActionResult Index(int? page = null)
        {
            if (User.Identity.Name == "")
            {
                return RedirectToAction("Login", "Account");
            }

            eBookCommerceViewModel viewModel = new eBookCommerceViewModel(0, 0, page);
            return View(viewModel);
        }

        [HttpPost]
        public JsonResult AddToBasket(Basket model)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);
            var basketItems = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();

            if(!basketItems.Any(a => a.bookId == model.bookId))
            {
                ebcDB.Baskets.Add(model);
                ebcDB.SaveChanges();
            }
            
            return Json(true);
        }
    }
}