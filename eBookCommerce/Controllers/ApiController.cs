using eBookCommerce.Models;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace eBookCommerce.Controllers
{
    public class ApiController : Controller
    {
        eBookCommerceEntities ebcDB = new eBookCommerceEntities();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApiController()
        {
        }

        public ApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public string GetOthersBooksMobile(string email)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);
            var books = ebcDB.Books.Where(a => a.personId != user.Id && !a.Baskets.Any(b => b.personId == user.Id)).ToList();

            var booksJson = from book in books
                            select new
                            {
                                bookId = book.bookId,
                                bookName = book.bookName,
                                bookAuthor = book.bookAuthor,
                                bookPages = book.bookPages,
                                bookPrice = book.bookPrice,
                                bookImageUrl = book.bookImageUrl,
                                bookDescription = book.bookDescription
                            };

            string jsonString = JsonConvert.SerializeObject(booksJson.ToList());

            return jsonString;
        }

        public string GetOwnBooksMobile(string email)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);
            var books = ebcDB.Books.Where(a => a.personId == user.Id).ToList();

            var booksJson = from book in books
                            select new
                            {
                                bookId = book.bookId,
                                bookName = book.bookName,
                                bookAuthor = book.bookAuthor,
                                bookPages = book.bookPages,
                                bookPrice = book.bookPrice,
                                bookImageUrl = book.bookImageUrl,
                                bookDescription = book.bookDescription
                            };

            string jsonString = JsonConvert.SerializeObject(booksJson.ToList());

            return jsonString;
        }

        public string GetBasketItemsMobile(string email)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);
            var basketItems = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();

            var booksJson = from basketItem in basketItems
                            select new
                            {
                                basketItemId = basketItem.basketId,
                                personEmail = basketItem.AspNetUser.Email,
                                bookId = basketItem.Book.bookId,
                                bookName = basketItem.Book.bookName,
                                bookAuthor = basketItem.Book.bookAuthor,
                                bookPages = basketItem.Book.bookPages,
                                bookPrice = basketItem.Book.bookPrice,
                                bookImageUrl = basketItem.Book.bookImageUrl,
                                bookDescription = basketItem.Book.bookDescription
                            };

            string jsonString = JsonConvert.SerializeObject(booksJson.ToList());

            return jsonString;
        }

        public string AddToBasket(string email, int bookId)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);

            Basket basketItem = new Basket() {
                personId = user.Id,
                bookId = bookId
            };

            ebcDB.Baskets.Add(basketItem);
            ebcDB.SaveChanges();

            var jsonObject = new
            {
                status = "OK",
                message = "Book was added to basket successfully!!!"
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject);
            return jsonString;
        }
        
        public string RemoveItemFromBasket(int itemId)
        {
            var basketItem = ebcDB.Baskets.Single(a => a.basketId == itemId);

            ebcDB.Baskets.Remove(basketItem);
            ebcDB.SaveChanges();

            var jsonObject = new
            {
                status = "OK",
                message = "Book was removed from basket successfully!!!"
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject);
            return jsonString;
        }
        

        [HttpPost]
        [AllowAnonymous]
        public async Task<string> Login(LoginViewModel model)
        {
            var status = "";
            var message = "";

            if (!ModelState.IsValid)
            {
                status = "NOK";
                message = "Wrong email or password!";
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    status = "OK";
                    message = "Success!";
                    break;
                case SignInStatus.LockedOut:
                    status = "NOK";
                    message = "Locked out!";
                    break;
                case SignInStatus.RequiresVerification:
                    status = "NOK";
                    message = "Requires verification";
                    break;
                case SignInStatus.Failure:
                    status = "NOK";
                    message = "Failure!";
                    break;
                default:
                    status = "NOK";
                    message = "Invalid login attempt!";
                    break;
            }

            var jsonObject = new
            {
                status = status,
                message = message
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject);
            return jsonString;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<string> Register(RegisterViewModel model)
        {
            var status = "";
            var message = "";

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    status = "OK";
                    message = "Success!";
                }
                else
                {
                    status = "NOK";
                    message = "Failure!";
                }
            }
            else
            {
                status = "NOK";
                message = "Wrong email or password format!";
            }

            var jsonObject = new
            {
                status = status,
                message = message
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject);
            return jsonString;           
        }
    }
}