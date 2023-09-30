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
            ebcDB.Baskets.Add(model);
            ebcDB.SaveChanges();
            return Json(true);
        }
    }
}