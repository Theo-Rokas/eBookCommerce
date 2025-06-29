using eBookCommerce.Models;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
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
                                bookDescription = book.bookDescription,
                                genreId = book.Genre.genreId,
                                genreName = book.Genre.genreName,
                            };

            string jsonString = JsonConvert.SerializeObject(booksJson.ToList());

            return jsonString;
        }

        [HttpPost]
        public string AddToBasket(string email, int bookId)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);

            Basket basketItem = new Basket()
            {
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
                                bookDescription = book.bookDescription,
                                genreId = book.Genre.genreId,
                                genreName = book.Genre.genreName,
                            };

            string jsonString = JsonConvert.SerializeObject(booksJson.ToList());

            return jsonString;
        }

        public string GetGenresMobile()
        {
            var genres = ebcDB.Genres.ToList();

            var genresJson = from genre in genres
                             select new
                             {
                                 genreId = genre.genreId,
                                 genreName = genre.genreName
                             };

            string jsonString = JsonConvert.SerializeObject(genresJson.ToList());

            return jsonString;
        }

        [HttpPost]
        public string CreateOrEditBook(eBookCommerceViewModel model, int bookId = 0)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == model.email);

            var status = "";
            var message = "";

            if (ModelState.ContainsKey("webBookImageFile"))
            {
                ModelState["webBookImageFile"].Errors.Clear();
            }

            if (!ModelState.IsValid)
            {
                status = "NOK";
                message = "Fill all the required fields!!!";

                var jsonObject = new
                {
                    status = status,
                    message = message
                };

                string jsonString = JsonConvert.SerializeObject(jsonObject);
                return jsonString;
            }

            var s3BookImageUrl = Helpers.S3Helper.UploadFile(model.mobileBookImageFile);

            if (bookId == 0)
            {
                Book book = new Book()
                {
                    bookName = model.book.bookName,
                    bookAuthor = model.book.bookAuthor,
                    bookPages = model.book.bookPages,
                    bookPrice = model.book.bookPrice,
                    bookImageUrl = s3BookImageUrl,
                    bookDescription = model.book.bookDescription,
                    personId = user.Id,
                    genreId = model.book.genreId
                };

                ebcDB.Books.Add(book);
                ebcDB.SaveChanges();

                status = "OK";
                message = "Book was created successfully!";
            }
            else
            {
                var book = ebcDB.Books.Single(a => a.bookId == bookId);

                book.bookName = model.book.bookName;
                book.bookAuthor = model.book.bookAuthor;
                book.bookPages = model.book.bookPages;
                book.bookPrice = model.book.bookPrice;
                book.bookImageUrl = s3BookImageUrl;
                book.bookDescription = model.book.bookDescription;
                book.personId = user.Id;
                book.genreId = model.book.genreId;
                ebcDB.SaveChanges();

                status = "OK";
                message = "Book was edited successfully!";
            }

            var jsonObject2 = new
            {
                status = status,
                message = message
            };

            string jsonString2 = JsonConvert.SerializeObject(jsonObject2);
            return jsonString2;
        }

        [HttpPost]
        public string RemoveBook(int bookId)
        {
            var book = ebcDB.Books.Single(a => a.bookId == bookId);
            ebcDB.Books.Remove(book);
            ebcDB.SaveChanges();

            var jsonObject = new
            {
                status = "OK",
                message = "Book was removed successfully!!!"
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject);
            return jsonString;
        }

        [HttpPost]
        public string RemoveAllBooks(string email)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);
            var books = ebcDB.Books.Where(a => a.personId == user.Id).ToList();

            foreach (var book in books)
            {
                ebcDB.Books.Remove(book);
            }

            ebcDB.SaveChanges();

            var jsonObject = new
            {
                status = "OK",
                message = "Books were removed successfully!!!"
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject);
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
                                bookDescription = basketItem.Book.bookDescription,
                                genreId = basketItem.Book.Genre.genreId,
                                genreName = basketItem.Book.Genre.genreName,
                            };

            string jsonString = JsonConvert.SerializeObject(booksJson.ToList());

            return jsonString;
        }

        [HttpPost]
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
        public string RemoveAllItemsFromBasket(string email)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);
            var basketItems = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();

            foreach (var basketItem in basketItems)
            {
                ebcDB.Baskets.Remove(basketItem);
            }

            ebcDB.SaveChanges();

            var jsonObject = new
            {
                status = "OK",
                message = "Books were removed from basket successfully!!!"
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject);
            return jsonString;
        }

        private static readonly string domain = "Put domain here";
        private static readonly string userName = "eBookCommerce-api";
        private static readonly string password = "ceyijX6";

        [HttpGet]
        public string PaymentInit(string email)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == email);
            var basketItems = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();

            var amount = (int)(basketItems.Sum(a => a.Book.bookPrice.Value) * 100);
            email = user.Email;

            var payment = new Payment()
            {
                paymentAmount = amount,
                paymentUserName = userName,
                paymentEmail = email,
                paymentCreatedAt = DateTime.UtcNow
            };

            ebcDB.Payments.Add(payment);
            ebcDB.SaveChanges();

            foreach (var basketItem in basketItems)
            {
                basketItem.paymentId = payment.paymentId;
            }
            ebcDB.SaveChanges();

            var orderNumber = payment.paymentId + "_" + DateTime.UtcNow.Ticks;
            payment.paymentOrderNumber = orderNumber;
            ebcDB.SaveChanges();

            var returnUrl = domain + Url.Action("PaymentComplete", "Api");

            var paymentUrl = "https://gateway-test.jcc.com.cy/payment/rest/register.do";

            var client = new RestClient(paymentUrl);
            var request = new RestRequest();

            request.Method = Method.Post;
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("amount", amount);
            request.AddParameter("userName", userName);
            request.AddParameter("password", password);
            request.AddParameter("orderNumber", orderNumber);
            request.AddParameter("returnUrl", returnUrl);
            request.AddParameter("email", email);
            request.AddParameter("language", "en");

            var response = client.Execute(request);

            var content = response.Content;
            dynamic jsonResponse = JsonConvert.DeserializeObject(content);


            if (jsonResponse.errorCode != null && jsonResponse.errorCode > 0)
            {
                int errorCode = jsonResponse.errorCode;
                string errorMessage = jsonResponse.errorMessage;

                payment.paymentErrorCode = errorCode.ToString();
                payment.paymentErrorMessage = errorMessage;
                ebcDB.SaveChanges();

                var jsonObject = new
                {
                    status = "NOK",
                    message = "Payment Init Failure!!!",
                    redirectUrl = ""
                };

                string jsonString = JsonConvert.SerializeObject(jsonObject);
                return jsonString;
            }

            string formUrl = jsonResponse.formUrl;
            string orderId = jsonResponse.orderId;

            payment.paymentOrderId = orderId;
            ebcDB.SaveChanges();

            var jsonObject2 = new
            {
                status = "OK",
                message = "Payment Init Success!!!",
                redirectUrl = formUrl
            };

            string jsonString2 = JsonConvert.SerializeObject(jsonObject2);
            return jsonString2;
        }

        [HttpGet]
        public string PaymentComplete(string orderId)
        {
            var statusUrl = "https://gateway-test.jcc.com.cy/payment/rest/getOrderStatusExtended.do";

            var client = new RestClient(statusUrl);
            var request = new RestRequest();

            request.Method = Method.Post;
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("userName", userName);
            request.AddParameter("password", password);
            request.AddParameter("orderId", orderId);
            request.AddParameter("language", "en");

            var response = client.Execute(request);

            dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);

            int orderStatus = jsonResponse.orderStatus;
            int approvedAmount = jsonResponse.paymentAmountInfo.approvedAmount;
            string maskedPan = jsonResponse.cardAuthInfo.maskedPan;
            string cardholderName = jsonResponse.cardAuthInfo.cardholderName;

            var payment = ebcDB.Payments.Single(a => a.paymentOrderId == orderId);
            payment.paymentStatus = orderStatus;
            payment.paymentApprovedAmount = approvedAmount;
            payment.paymentMaskedPan = maskedPan;
            payment.paymentCardholderName = cardholderName;
            payment.paymentUpdatedAt = DateTime.UtcNow;
            ebcDB.SaveChanges();

            if (orderStatus == 2)
            {
                var basketItems = payment.Baskets.ToList();

                foreach (var basketItem in basketItems)
                {
                    ebcDB.Books.Remove(basketItem.Book);
                    ebcDB.Baskets.Remove(basketItem);                    
                }

                ebcDB.SaveChanges();

                var jsonObject = new
                {
                    status = "OK",
                    message = "Payment Complete Success!!!"
                };

                string jsonString = JsonConvert.SerializeObject(jsonObject);
                return jsonString;
            }
            else
            {
                if (jsonResponse.errorCode != null && jsonResponse.errorCode > 0)
                {
                    int errorCode = jsonResponse.errorCode;
                    string errorMessage = jsonResponse.errorMessage;

                    payment.paymentErrorCode = errorCode.ToString();
                    payment.paymentErrorMessage = errorMessage;
                    ebcDB.SaveChanges();
                }

                var jsonObject = new
                {
                    status = "NOK",
                    message = "Payment Complete Failure!!!"
                };

                string jsonString = JsonConvert.SerializeObject(jsonObject);
                return jsonString;
            }
        }        
    }
}
